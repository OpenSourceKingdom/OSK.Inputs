using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OSK.Functions.Outputs.Abstractions;
using OSK.Functions.Outputs.Logging.Abstractions;
using OSK.Inputs.Abstractions;
using OSK.Inputs.Options;
using OSK.Inputs.Ports;
using OSK.Inputs.Abstractions.Configuration;
using OSK.Inputs.Internal.Models;
using OSK.Inputs.Abstractions.Runtime;
using OSK.Inputs.Abstractions.Notifications;

namespace OSK.Inputs.Internal.Services;

internal partial class InputUserManager(IInputConfigurationProvider configurationProvider, IInputNotificationPublisher notificationPublisher,
    IInputSchemeRepository schemeRepository, ILogger<InputUserManager> logger, IOutputFactory<InputUserManager> outputFactory) 
    : IInputUserManager
{
    #region Variables

    // TODO: Encapsulation Better
    internal readonly Dictionary<int, InputUser> _users = [];
    internal readonly Dictionary<int, Dictionary<string, PreferredInputScheme>> _userPreferredSchemesLookup = [];

    #endregion

    #region IInputUserManager

    public IOutput<IInputUser> CreateUser(UserJoinOptions options)
    {
        if (options is null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        if (configurationProvider.Configuration.JoinPolicy.MaxUsers <= _users.Count)
        {
            return outputFactory.Fail<IInputUser>($"Unable to create user as the maximum number of users ({configurationProvider.Configuration.JoinPolicy.MaxUsers}) has been reached.", OutputSpecificityCode.InvalidParameterInputRange);
        }

        var devicesToPair = options.DevicesToPair ?? [];
        var alreadyPairedDevices = devicesToPair.Where(device => GetInputUserForDevice(device.DeviceId) is not null);
        if (alreadyPairedDevices.Any())
        {
            var inputDeviceError = string.Join(",", alreadyPairedDevices.Select(device => device.DeviceFamily));
            return outputFactory.Fail<IInputUser>($"Unable to create user as one more devices have already been paired to the input system: {inputDeviceError}");
        }

        var newUserId = _users.Count is 0
            ? 1
            : _users.Values.Max(user => user.Id) + 1;

        InputDefinition? inputDefinition = null;
        var useActiveDefinition = !string.IsNullOrWhiteSpace(options.ActiveScheme?.DefinitionName);
        if (useActiveDefinition)
        {
            inputDefinition = configurationProvider.Configuration.Definitions.FirstOrDefault(definition
                => definition.Name.Equals(options.ActiveScheme!.Value.DefinitionName, StringComparison.OrdinalIgnoreCase));
        }

        inputDefinition = inputDefinition ?? configurationProvider.Configuration.Definitions.FirstOrDefault(definition 
            => definition.IsDefault)
            ?? configurationProvider.Configuration.Definitions.First();

        if (useActiveDefinition && !inputDefinition.Name.Equals(options.ActiveScheme!.Value.DefinitionName, StringComparison.OrdinalIgnoreCase))
        {
            LogCreateUseActiveDefinitionNameNotFoundWarning(logger, options.ActiveScheme.Value.DefinitionName, inputDefinition.Name);
        }

        var activeScheme = GetActiveScheme(newUserId, inputDefinition);

        if (!string.IsNullOrWhiteSpace(options.ActiveScheme?.SchemeName)
             && !activeScheme.SchemeName.Equals(options.ActiveScheme!.Value.SchemeName, StringComparison.OrdinalIgnoreCase)) 
        {
            LogCreateUserActiveSchemeNameNotFoundWarning(logger, inputDefinition.Name, options.ActiveScheme.Value.SchemeName, activeScheme.SchemeName);
        }

        _users[newUserId] = new InputUser(newUserId, activeScheme);

        notificationPublisher.Notify(new InputUserJoinedNotification(_users[newUserId]));

        foreach (var device in devicesToPair)
        {
            _users[newUserId].AddDevice(device);
            notificationPublisher.Notify(new DevicePairedNotification(newUserId, device));
        }

        return outputFactory.Succeed((IInputUser)_users[newUserId]);
    }

    public IOutput SetActiveDefinition(int userId, string definitionName)
    {
        if (!_users.TryGetValue(userId, out var user))
        {
            LogSetActiveDefinitionForBadUserInformation(logger, userId);
            return outputFactory.NotFound($"User {userId} does not exist.");
        }

        if (string.IsNullOrWhiteSpace(definitionName))
        {
            LogActiveDefinitionNameNotFoundWarning(logger, "{Empty}");
            return outputFactory.Fail("Input definition name cannot be null.");
        }

        var definition = configurationProvider.Configuration.GetDefinition(definitionName);
        if (definition is null)
        {
            LogActiveDefinitionNameNotFoundWarning(logger, definitionName);
            return outputFactory.NotFound($"Input definition with name {definition} does not exist.");
        }

        user.ActiveScheme = GetActiveScheme(userId, definition);
        notificationPublisher.Notify(new InputUserSchemeChangeNotification(userId, user.ActiveScheme));
        return outputFactory.Succeed();
    }

    public IInputUser? GetInputUserForDevice(int deviceId)
        => _users.Values.FirstOrDefault(user => user.GetPairedDevice(deviceId) is not null);

    public IInputUser? GetUser(int userId)
        => _users.TryGetValue(userId, out var user)
            ? user
            : null;

    public IEnumerable<IInputUser> GetUsers()
        => _users.Values;

    public bool RemoveUser(int userId)
    {
        if (_users.TryGetValue(userId, out var user))
        {
            foreach (var deviceIdentifier in user.GetPairedDevices().Select(device => device.DeviceIdentifier).ToArray())
            {
                UnpairDevice(userId, deviceIdentifier.DeviceId);
            }

            _users.Remove(userId);
            notificationPublisher.Notify(new InputUserRemovedNotification(userId));
            return true;
        }

        return false;
    }

    public IOutput PairDevice(int userId, RuntimeDeviceIdentifier device)
    {
        if (!_users.TryGetValue(userId, out var user))
        {
            return outputFactory.NotFound($"Unable to pair device {device.DeviceId}, {device.DeviceFamily.Name}, because there is no user with id {userId}.");
        }

        var pairedUser = GetInputUserForDevice(device.DeviceId);
        if (pairedUser is not null)
        {
            return pairedUser.Id == userId
                ? outputFactory.Succeed()
                : outputFactory.Fail($"Unable to pair device {device.DeviceId}, {device.DeviceFamily.Name}, to user {userId} because it is already paired to {pairedUser.Id}.");
        }

        user.AddDevice(device);
        return outputFactory.Succeed();
    }

    public bool UnpairDevice(int userId, int deviceId)
    {
        if (!_users.TryGetValue(userId, out var user))
        {
            return false;
        }

        var pairedDevice = user.RemoveDevice(deviceId);
        if (pairedDevice is not null)
        {
            notificationPublisher.Notify(new DeviceUnpairedNotification(userId, pairedDevice.DeviceIdentifier));
            return true;
        }

        return false;
    }

    public async Task<IOutput> SavePreferredSchemeAsync(PreferredInputScheme scheme, CancellationToken cancellationToken = default)
    {
        if (scheme.UserId < 0 || scheme.UserId >= configurationProvider.Configuration.JoinPolicy.MaxUsers)
        {
            return outputFactory.Fail($"The provided user id must be non-zero and less than the max users ({configurationProvider.Configuration.JoinPolicy.MaxUsers}) for the input system.");
        }

        if (string.IsNullOrWhiteSpace(scheme.DefinitionName))
        {
            return outputFactory.Fail("Definition name can not be empty.");
        }

        var definition = configurationProvider.Configuration.GetDefinition(scheme.DefinitionName);
        if (definition is null)
        {
            return outputFactory.NotFound($"No input definition with the name '{scheme.DefinitionName}' exists.");
        }

        if (string.IsNullOrWhiteSpace(scheme.SchemeName))
        {
            return outputFactory.Fail("Scheme name can not be empty.");
        }

        if (definition.GetScheme(scheme.SchemeName) is null)
        {
            return outputFactory.NotFound($"No input scheme named '{scheme.SchemeName}' exists on the definition '{scheme.DefinitionName}'.");
        }

        // Fix scheme not taking effect
        return await schemeRepository.SavePreferredSchemeAsync(scheme, cancellationToken);
    }

    public async Task<IOutput> LoadUserConfigurationAsync(CancellationToken cancellationToken = default)
    {
        configurationProvider.Configuration.Reset();

        var getUserPreferredSchemes = await schemeRepository.GetPreferredSchemesAsync(cancellationToken);
        if (getUserPreferredSchemes.IsSuccessful)
        {
            // Only take one preferred scheme for each definition, even if the repository returns multiples.
            // There should only ever be 1 preferred scheme per definition, so multiples would indicate either a
            // mistake in the repository or some malicious intent
            foreach (var userPreferredSchemes in getUserPreferredSchemes.Value
                .Where(preferredScheme =>
                {
                    if (preferredScheme.UserId < 0 || preferredScheme.UserId > configurationProvider.Configuration.JoinPolicy.MaxUsers)
                    {
                        return false;
                    }

                    var definition = configurationProvider.Configuration.GetDefinition(preferredScheme.DefinitionName);
                    if (definition is null)
                    {
                        return false;
                    }

                    return definition.GetScheme(preferredScheme.SchemeName) is not null;
                }).GroupBy(scheme => scheme.UserId))
            {
                _userPreferredSchemesLookup[userPreferredSchemes.Key] = 
                    userPreferredSchemes.GroupBy(scheme => scheme.DefinitionName)
                                        .ToDictionary(schemeGroup => schemeGroup.Key, schemeGroup => schemeGroup.First());
            }
        }
        else
        {
            LogLoadActiveInputFailedWarning(logger, getUserPreferredSchemes.GetErrorString());
        }

        var getCustomSchemesOutput = await schemeRepository.GetCustomSchemesAsync(cancellationToken);
        if (getCustomSchemesOutput.IsSuccessful)
        {
            configurationProvider.Configuration.ApplyCustomInputSchemes(getCustomSchemesOutput.Value);
        }
        else
        {
            LogLoadCustomSchemesFailedWarning(logger, getCustomSchemesOutput.GetErrorString());
        }

        return outputFactory.Succeed();
    }

    #endregion

    #region Helpers

    private ActiveInputScheme GetActiveScheme(int userId, InputDefinition definition, string? preferredSchemeName = null)
    {
        if (!string.IsNullOrWhiteSpace(preferredSchemeName))
        {
            var activeScheme = definition.GetScheme(preferredSchemeName);
            if (activeScheme is not null)
            {
                return new ActiveInputScheme(definition.Name, preferredSchemeName);
            }
        }

        if (_userPreferredSchemesLookup.TryGetValue(userId, out var userPreferredSchemeLookup)
             && userPreferredSchemeLookup.TryGetValue(definition.Name, out var preferredInputScheme)
             && definition.GetScheme(preferredInputScheme.SchemeName) is not null)
        {
            return new ActiveInputScheme(definition.Name, preferredInputScheme.SchemeName);
        }

        var defaultScheme = definition.Schemes.FirstOrDefault(scheme => scheme.IsDefault) ?? definition.Schemes.First();
        return new ActiveInputScheme(definition.Name, defaultScheme.Name);
    }

    #endregion

    #region Logging

    [LoggerMessage(eventId: 1, LogLevel.Warning, "An error was encountered when attempting to get active input schemes, using an empty list; error: {error}")]
    private static partial void LogLoadActiveInputFailedWarning(ILogger logger, string error);

    [LoggerMessage(eventId: 2, LogLevel.Warning, "An error was encountered when attmepting to get custom input shcemes, using an empty list; error: {error}")]
    private static partial void LogLoadCustomSchemesFailedWarning(ILogger logger, string error);

    [LoggerMessage(eventId: 3, LogLevel.Warning, "An attempt was made to create a user with a specified active definition name '{definitionName}' that was not found in the input system, defaulting to using '{defaultDefinitionName}'.")]
    private static partial void LogCreateUseActiveDefinitionNameNotFoundWarning(ILogger logger, string definitionName, string defaultDefinitionName);

    [LoggerMessage(eventId: 4, LogLevel.Warning, "An attempt was made to create a user for the definition name '{definitionName}' with the active input scheme '{schemeName}' but the scheme was not found, defaulting to using '{defaultSchemeName}'.")]
    private static partial void LogCreateUserActiveSchemeNameNotFoundWarning(ILogger logger,  string definitionName, string schemeName, string defaultSchemeName);

    [LoggerMessage(eventId: 5, LogLevel.Warning, "An attempt was made set a user with a specified active definition name '{definitionName}' that was not found in the input system, ignoring.")]
    private static partial void LogActiveDefinitionNameNotFoundWarning(ILogger logger, string definitionName);

    [LoggerMessage(eventId: 6, LogLevel.Information, "An attempt was made to set a user input scheme for the definition name '{definitionName}' with the active input scheme '{schemeName}' but the scheme was not found, ignoring.")]
    private static partial void LogActiveSchemeNameNotFoundInformation(ILogger logger, string definitionName, string schemeName);

    [LoggerMessage(eventId: 7, LogLevel.Information, "An attempt was made to set the active definition for user {userId} but that user does not exist, ignoring.")]
    private static partial void LogSetActiveDefinitionForBadUserInformation(ILogger logger, int userId);

    #endregion
}
