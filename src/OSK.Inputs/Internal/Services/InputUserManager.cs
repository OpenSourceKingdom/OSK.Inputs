using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OSK.Functions.Outputs.Abstractions;
using OSK.Functions.Outputs.Logging.Abstractions;
using OSK.Inputs.Abstractions;
using OSK.Inputs.Abstractions.Events;
using OSK.Inputs.Options;
using OSK.Inputs.Ports;
using OSK.Inputs.Abstractions.Configuration;
using OSK.Inputs.Internal.Models;

namespace OSK.Inputs.Internal.Services;

internal partial class InputUserManager(IInputConfigurationProvider configurationProvider, IInputNotificationPublisher notificationPublisher,
    IInputSchemeRepository schemeRepository, ILogger<InputUserManager> logger, IOutputFactory<InputUserManager> outputFactory) 
    : IInputUserManager
{
    #region Variables

    private readonly Dictionary<int, InputUser> _users = [];
    private readonly Dictionary<int, Dictionary<string, PreferredInputScheme>> _userPreferredSchemesLookup = [];

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
            var inputDeviceError = string.Join(",", alreadyPairedDevices.Select(device => device.Identity));
            return outputFactory.Fail<IInputUser>($"Unable to create user as one more devices have already been paired to the input system: {inputDeviceError}");
        }

        var newUserId = _users.Count is 0
            ? 1
            : _users.Values.Max(user => user.Id) + 1;

        var inputDefinition = configurationProvider.Configuration.Definitions.FirstOrDefault(definition => definition.IsDefault)
            ?? configurationProvider.Configuration.Definitions.First();

        var activeScheme = GetActiveScheme(newUserId, inputDefinition);
        _users[newUserId] = new InputUser(newUserId, activeScheme);

        notificationPublisher.Notify(new InputUserJoinedEvent(_users[newUserId]));

        foreach (var device in devicesToPair)
        {
            _users[newUserId].AddDevice(device);
            notificationPublisher.Notify(new UserDeviceEvent(newUserId, device, DeviceEventType.Paired));
        }

        return outputFactory.Succeed((IInputUser)_users[newUserId]);
    }

    public void SetActiveDefinition(int userId, string definitionName)
    {
        if (string.IsNullOrWhiteSpace(definitionName))
        {
            return;
        }

        var definition = configurationProvider.Configuration.GetDefinition(definitionName);
        if (definition is null)
        {
            return;
        }

        if (!_users.TryGetValue(userId, out var user))
        {
        }

        user.ActiveScheme = GetActiveScheme(userId, definition);
    }

    public IInputUser? GetInputUserForDevice(int deviceId)
        => _users.Values.FirstOrDefault(user => user.GetPairedDevice(deviceId) is not null);

    public IInputUser? GetUser(int userId)
        => _users.TryGetValue(userId, out var user)
            ? user
            : null;

    public IEnumerable<IInputUser> GetUsers()
        => _users.Values;

    public void RemoveUser(int userId)
    {
        if (_users.Remove(userId))
        {
            notificationPublisher.Notify(new InputUserRemovedEvent(userId));
        }
    }

    public IOutput PairDevice(int userId, RuntimeDeviceIdentifier device)
    {
        var pairedUser = GetInputUserForDevice(userId);
        if (pairedUser is not null)
        {
            return pairedUser.Id == userId
                ? outputFactory.Succeed()
                : outputFactory.Fail($"Unable to pair device {device.DeviceId}, {device.Identity.Name}, to user {userId} because it is already paired to {pairedUser.Id}.");
        }

        if (!_users.TryGetValue(userId, out var user))
        {
            return outputFactory.Fail($"Unable to pair device {device.DeviceId}, {device.Identity.Name}, because there is no user with id {userId}.");
        }

        user.AddDevice(device);
        return outputFactory.Succeed();
    }

    public void UnpairDevice(int userId, int deviceId)
    {
        if (!_users.TryGetValue(userId, out var user))
        {
            return;
        }

        var pairedDevice = user.RemoveDevice(deviceId);
        if (pairedDevice is not null)
        {
            notificationPublisher.Notify(new UserDeviceEvent(userId, pairedDevice.DeviceIdentifier, DeviceEventType.Unpaired));
        }
    }

    public async Task<IOutput> SavePreferredSchemeAsync(PreferredInputScheme scheme, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(scheme.DefinitionName))
        {
            return outputFactory.Fail("Definition name can not be empty.");
        }
        if (string.IsNullOrWhiteSpace(scheme.SchemeName))
        {
            return outputFactory.Fail("Scheme name can not be empty.");
        }
        if (scheme.UserId < 0 || scheme.UserId >= configurationProvider.Configuration.JoinPolicy.MaxUsers)
        {
            return outputFactory.Fail($"The provided user id must be non-zero and less than the max users ({configurationProvider.Configuration.JoinPolicy.MaxUsers}) for the input system.");
        }

        var definition = configurationProvider.Configuration.GetDefinition(scheme.DefinitionName);
        if (definition is null)
        {
            return outputFactory.NotFound($"No input definition with the name '{scheme.DefinitionName}' exists.");
        }

        if (definition.GetScheme(scheme.SchemeName) is null)
        {
            return outputFactory.NotFound($"No input scheme named '{scheme.SchemeName}' exists on the definition '{scheme.DefinitionName}'.");
        }

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
            foreach (var userPreferredSchemes in getUserPreferredSchemes.Value.GroupBy(scheme => scheme.UserId))
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

    private ActiveInputScheme GetActiveScheme(int userId, InputDefinition definition)
    {
        if (_userPreferredSchemesLookup.TryGetValue(userId, out var userPreferredSchemeLookup)
             && userPreferredSchemeLookup.TryGetValue(definition.Name, out var preferredInputScheme)
             && definition.GetScheme(preferredInputScheme.SchemeName) is not null)
        {
            return new ActiveInputScheme()
            {
                DefinitionName = definition.Name,
                SchemeName = preferredInputScheme.SchemeName
            };
        }

        var defaultScheme = definition.Schemes.FirstOrDefault(scheme => scheme.IsDefault) ?? definition.Schemes.First();
        return new ActiveInputScheme()
        {
            DefinitionName = definition.Name,
            SchemeName = defaultScheme.Name
        };
    }

    #endregion

    #region Logging

    [LoggerMessage(eventId: 1, LogLevel.Warning, "An error was encountered when attempting to get active input schemes, using an empty list; error: {error}")]
    private static partial void LogLoadActiveInputFailedWarning(ILogger logger, string error);

    [LoggerMessage(eventId: 2, LogLevel.Warning, "An error was encountered when attmepting to get custom input shcemes, using an empty list; error: {error}")]
    private static partial void LogLoadCustomSchemesFailedWarning(ILogger logger, string error);

    #endregion
}
