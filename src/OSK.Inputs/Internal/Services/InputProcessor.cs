using System;
using Microsoft.Extensions.Logging;
using OSK.Inputs.Abstractions;
using OSK.Inputs.Ports;
using System.Collections.Generic;
using OSK.Inputs.Abstractions.Events;
using System.Linq;
using OSK.Inputs.Options;
using OSK.Inputs.Abstractions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OSK.Inputs.Internal.Models;

namespace OSK.Inputs.Internal.Services;

internal partial class InputProcessor: IInputProcessor
{
    #region Variables

    private bool _paused;
    private readonly Dictionary<int, UserInputTracker> _userInputTrackerLookup = [];

    private readonly IInputUserManager _userManager;
    private readonly IInputNotificationPublisher _notificationPublisher;
    private readonly IInputConfigurationProvider _configurationProvider;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<InputProcessor> _logger;

    private readonly ObjectFactory<UserInputTracker> _inputTrackerFactory
        = ActivatorUtilities.CreateFactory<UserInputTracker>([typeof(ActiveInputScheme), typeof(IEnumerable<DeviceSchemeActionMap>)]);

    #endregion

    #region Constructors

    public InputProcessor(IInputUserManager userManager, IInputNotificationPublisher notificationPublisher, 
        IInputConfigurationProvider configurationProvider, IServiceProvider serviceProvider, ILogger<InputProcessor> logger)
    {
        _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        _configurationProvider = configurationProvider ?? throw new ArgumentNullException(nameof(configurationProvider));
        _notificationPublisher = notificationPublisher ?? throw new ArgumentNullException(nameof(notificationPublisher));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        notificationPublisher.OnUserEvent += HandleUserEvent;
    }

    #endregion

    #region IInputProcessor

    public void ProcessInputs(TimeSpan deltaTime)
    {
        if (_paused)
        {
            return;
        }

        foreach (var inputTracker in _userInputTrackerLookup.Values)
        {
            var triggeredActivations = inputTracker.Process(deltaTime);
            foreach (var triggeredActivation in triggeredActivations)
            {
                triggeredActivation.ActionMap.Action.Execute(triggeredActivation.ActivationContext);
            }
        }
    }

    public void ProcessActivation(InputActivation activation)
    {
        if (activation is null)
        {
            throw new ArgumentNullException(nameof(activation));
        }
        if (_paused)
        {
            return;
        }
        if (activation is not DeviceInputActivation deviceInputActivation)
        {
            return;
        }

        var inputUser = _userManager.GetInputUserForDevice(deviceInputActivation.DeviceIdentifier.DeviceId) ?? TryDevicePairing(_configurationProvider.Configuration, deviceInputActivation.DeviceIdentifier);
        if (inputUser is null)
        {
            LogNoInputUserForDeviceWarning(_logger, deviceInputActivation.DeviceIdentifier.Identity);
            _notificationPublisher.Notify(new UnrecognizedDeviceEvent(deviceInputActivation.DeviceIdentifier));
            return;
        }

        if (!_userInputTrackerLookup.TryGetValue(inputUser.Id, out var inputTracker))
        {
            inputTracker = CreateTracker(_configurationProvider.Configuration, inputUser.ActiveScheme);
        }

        var triggeredActivation = inputTracker.Track(activation);
        if (triggeredActivation is not null)
        {
            triggeredActivation.Value.ActionMap.Action.Execute(triggeredActivation.Value.ActivationContext);
        }
    }

    public void Pause(bool pause)
    {
        _paused = pause;
    }

    public void HandleDeviceEvent(DeviceStateChangedEvent deviceEvent)
    {
        if (deviceEvent is null)
        {
            throw new ArgumentNullException(nameof(deviceEvent));
        }

        var user = _userManager.GetInputUserForDevice(deviceEvent.DeviceIdentifier.DeviceId);
        if (user is null)
        {
            _notificationPublisher.Notify(deviceEvent);
        }
        else
        {
            var device = user.GetDevice(deviceEvent.DeviceIdentifier.DeviceId);
            var deviceEventType = deviceEvent.Status is DeviceStatus.Disconnected 
                ? DeviceEventType.Disconnected 
                : DeviceEventType.Connected;
            _notificationPublisher.Notify(new UserDeviceEvent(user.Id, deviceEvent.DeviceIdentifier, deviceEventType));
        }
    }

    #endregion

    #region Helpers

    private (InputDefinition, InputScheme) GetInputScheme(InputSystemConfiguration configuration, ActiveInputScheme activeScheme)
    {
        var inputDefinition = configuration.GetDefinition(activeScheme.DefinitionName);
        if (inputDefinition is null)
        {
            inputDefinition = configuration.Definitions.FirstOrDefault(definition => definition.IsDefault) ?? configuration.Definitions.First();
            LogInvalidDefinitionUsageWarning(_logger, activeScheme.DefinitionName, inputDefinition.Name);
        }

        var scheme = inputDefinition.GetScheme(activeScheme.SchemeName);
        if (scheme is null)
        {
            scheme = inputDefinition.Schemes.FirstOrDefault(s => s.IsDefault) ?? inputDefinition.Schemes.First();
            LogInvalidSchemeUsageWarning(_logger, inputDefinition.Name, activeScheme.SchemeName, scheme.Name);
        }

        return (inputDefinition, scheme);
    }

    private IInputUser? TryDevicePairing(InputSystemConfiguration configuration, RuntimeDeviceIdentifier device)
    {
        if (configuration.JoinPolicy.DeviceJoinBehavior is DevicePairingBehavior.Manual)
        {
            return null; 
        }

        var supportedController = configuration.SupportedDeviceCombinations
                                        .Where(deviceCombination => deviceCombination.DeviceIdentities.Contains(device.Identity))
                                        .Cast<InputDeviceCombination?>()
                                        .FirstOrDefault();
        if (supportedController is null)
        {
            return null;
        }

        var targetUser = _userManager.GetUsers()
                                    .FirstOrDefault(user => user.PairedDevices.All(pd => pd.DeviceIdentifier.Identity != device.Identity));
        if (targetUser is null)
        {
            var createUserOutput = _userManager.CreateUser(new UserJoinOptions());
            if (!createUserOutput.IsSuccessful)
            {
                return null;
            }

            targetUser = createUserOutput.Value;
        }

        _userManager.PairDevice(targetUser.Id, device);
        return targetUser;
    }

    private void HandleUserEvent(InputUserEvent userEvent)
    {
        switch (userEvent)
        {
            case InputUserSchemeChangeEvent schemeChangeEvent:
                _userInputTrackerLookup[schemeChangeEvent.UserId] = CreateTracker(_configurationProvider.Configuration, schemeChangeEvent.NewScheme);
                break;
            case InputUserJoinedEvent userJoinedEvent:
                _userInputTrackerLookup[userJoinedEvent.UserId] = CreateTracker(_configurationProvider.Configuration, userJoinedEvent.User.ActiveScheme);
                break;
            case InputUserRemovedEvent userRemovedEvent:
                _userInputTrackerLookup.Remove(userRemovedEvent.UserId);
                break;
        }
    }

    private UserInputTracker CreateTracker(InputSystemConfiguration configuration, ActiveInputScheme activeScheme)
    {
        var (inputDefinition, inputScheme) = GetInputScheme(configuration, activeScheme);
        var deviceSchemeMaps = configuration.GetSchemeMap(inputDefinition.Name, inputScheme.Name);

        return _inputTrackerFactory.Invoke(_serviceProvider, [activeScheme, deviceSchemeMaps]);
    }

    #endregion

    #region Logging

    [LoggerMessage(eventId: 1, LogLevel.Warning, "An input was received for a device named '{deviceIdentity}' but no user was found to be paired to it, thus the input will be ignored.")]
    private static partial void LogNoInputUserForDeviceWarning(ILogger logger, InputDeviceIdentity deviceIdentity);

    [LoggerMessage(eventId: 3, LogLevel.Warning, "An attempt was made to use an active scheme with input definition '{desiredDefinitionName}' but it did not exist, defaulting to '{defaultDefinitionName}'.")]
    private static partial void LogInvalidDefinitionUsageWarning(ILogger logger, string desiredDefinitionName, string defaultDefinitionName);

    [LoggerMessage(eventId: 4, LogLevel.Warning, "An attempt was made to use an active scheme '{desiredSchemeName}' with input definition '{definitionName}' but the scheme did not exist, defaulting to '{defaultSchemeName}'.")]
    private static partial void LogInvalidSchemeUsageWarning(ILogger logger, string definitionName, string desiredSchemeName, string defaultSchemeName);

    #endregion
}
