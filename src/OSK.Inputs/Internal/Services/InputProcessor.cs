using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OSK.Inputs.Abstractions;
using OSK.Inputs.Abstractions.Configuration;
using OSK.Inputs.Abstractions.Notifications;
using OSK.Inputs.Abstractions.Runtime;
using OSK.Inputs.Internal.Models;
using OSK.Inputs.Options;
using OSK.Inputs.Ports;

namespace OSK.Inputs.Internal.Services;

internal partial class InputProcessor: IInputProcessor
{
    #region Variables

    internal bool _pauseInputProcessing;
    private readonly Dictionary<int, IUserInputTracker> _userInputTrackerLookup = [];
    private readonly Dictionary<RuntimeDeviceIdentifier, int> _registeredUserDevices = [];

    private readonly IInputUserManager _userManager;
    private readonly IInputNotificationPublisher _notificationPublisher;
    private readonly IInputConfigurationProvider _configurationProvider;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<InputProcessor> _logger;

    private static readonly ObjectFactory<UserInputTracker> _inputTrackerFactory
        = ActivatorUtilities.CreateFactory<UserInputTracker>([typeof(int), typeof(ActiveInputScheme),
            typeof(InputSchemeActionMap), typeof(InputProcessorConfiguration)]);

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

        notificationPublisher.OnUserNotification += HandleUserEvent;
    }

    #endregion

    #region IInputProcessor

    public void Update(TimeSpan deltaTime)
    {
        if (_pauseInputProcessing)
        {
            return;
        }

        foreach (var inputTracker in _userInputTrackerLookup.Values)
        {
            var triggeredActions = inputTracker.Update(deltaTime);
            foreach (var triggeredAction in triggeredActions)
            {
                triggeredAction.Execute();
            }
        }
    }

    public void ProcessEvent(InputEvent inputEvent)
    {
        if (inputEvent is null)
        {
            throw new ArgumentNullException(nameof(inputEvent));
        }
        if (_pauseInputProcessing)
        {
            return;
        }
        if (inputEvent is not PhysicalInputEvent physicalInputEvent)
        {
            LogUnsupportedInputTypeInformation(_logger, inputEvent.GetType().FullName);
            return;
        }

        var inputTracker = GetInputTrackerForDevice(physicalInputEvent.DeviceIdentifier);
        if (inputTracker is null)
        {
            return;
        }

        var triggeredAction = inputTracker.Track(inputEvent);
        if (triggeredAction is not null)
        {
            LogInputActionTriggeredDebug(_logger, inputTracker.UserId, physicalInputEvent.DeviceIdentifier, inputTracker.ActiveScheme, triggeredAction.Value.ActionMap.Action.Name);
            triggeredAction.Value.Execute();
        }
    }

    public void ToggleInputProcessing(bool pause)
    {
        LogTogglePauseDebug(_logger, pause);
        _pauseInputProcessing = pause;
    }

    public void HandleDeviceNotification(DeviceStateChangedNotification deviceNotification)
    {
        if (deviceNotification is null)
        {
            throw new ArgumentNullException(nameof(deviceNotification));
        }

        var user = _userManager.GetInputUserForDevice(deviceNotification.DeviceIdentifier.DeviceId);
        if (user is null)
        {
            _notificationPublisher.Notify(deviceNotification);
            return;
        }

        var device = user.GetDevice(deviceNotification.DeviceIdentifier.DeviceId);
        UserDeviceNotification userDeviceNotification = deviceNotification.Status is DeviceStatus.Disconnected
            ? new UserDeviceDisconnectedNotification(user.Id, deviceNotification.DeviceIdentifier)
            : new UserDeviceConnectedNotification(user.Id, deviceNotification.DeviceIdentifier);
        _notificationPublisher.Notify(userDeviceNotification);
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

    private IInputUser? TryDevicePairing(InputSystemConfiguration configuration, RuntimeDeviceIdentifier deviceIdentifier)
    {
        if (configuration.JoinPolicy.DeviceJoinBehavior is DevicePairingBehavior.Manual)
        {
            LogUnpairdDeviceDueToPolicyInformation(_logger, deviceIdentifier);
            return null; 
        }

        var supportedDeviceCombination = configuration.SupportedDeviceCombinations
                                        .Where(deviceCombination => deviceCombination.DeviceIdentities.Contains(deviceIdentifier.Identity))
                                        .Cast<InputDeviceCombination?>()
                                        .FirstOrDefault();
        if (supportedDeviceCombination is null)
        {
            LogUnpairedDeviceDueToUnsupportedCombinationInformation(_logger, deviceIdentifier);
            return null;
        }

        var targetUser = _userManager.GetUsers()
                                    .FirstOrDefault(user => user.PairedDevices.All(pd => pd.DeviceIdentifier.Identity != deviceIdentifier.Identity));
        if (targetUser is null)
        {
            LogNewUserCreateDebug(_logger, deviceIdentifier);
            var createUserOutput = _userManager.CreateUser(new UserJoinOptions());
            if (!createUserOutput.IsSuccessful)
            {
                return null;
            }

            targetUser = createUserOutput.Value;
        }

        _userManager.PairDevice(targetUser.Id, deviceIdentifier);
        return targetUser;
    }

    private void HandleUserEvent(InputUserNotification userEvent)
    {
        switch (userEvent)
        {
            case InputUserSchemeChangeNotification schemeChangeEvent:
                _userInputTrackerLookup[schemeChangeEvent.UserId] = CreateTracker(schemeChangeEvent.UserId, _configurationProvider.Configuration, schemeChangeEvent.NewScheme);
                LogSchemeChangeDebug(_logger, schemeChangeEvent.UserId, schemeChangeEvent.NewScheme.DefinitionName, schemeChangeEvent.NewScheme.SchemeName);
                break;
            case InputUserJoinedNotification userJoinedEvent:
                _userInputTrackerLookup[userJoinedEvent.UserId] = CreateTracker(userJoinedEvent.UserId, _configurationProvider.Configuration, userJoinedEvent.User.ActiveScheme);
                LogUserJoinedDebug(_logger, userJoinedEvent.UserId);
                break;
            case InputUserRemovedNotification userRemovedEvent:
                _userInputTrackerLookup.Remove(userRemovedEvent.UserId);
                LogUserRemovedDebug(_logger, userRemovedEvent.UserId);
                break;
            default:
                LogUnknownUserNotificationInformation(_logger, userEvent.GetType().FullName);
                break;
        }
    }

    private IUserInputTracker? GetInputTrackerForDevice(RuntimeDeviceIdentifier deviceIdentifier)
    {
        if (_registeredUserDevices.TryGetValue(deviceIdentifier, out var userId))
        {
            return _userInputTrackerLookup[userId];
        }

        var deviceUser = _userManager.GetInputUserForDevice(deviceIdentifier.DeviceId) ?? TryDevicePairing(_configurationProvider.Configuration, deviceIdentifier);
        if (deviceUser is null)
        {
            LogNoInputUserForDeviceWarning(_logger, deviceIdentifier.Identity);
            _notificationPublisher.Notify(new UnrecognizedDeviceNotification(deviceIdentifier));
            return null;
        }

        if (!_userInputTrackerLookup.TryGetValue(deviceUser.Id, out var inputTracker))
        {
            LogNewInputTrackerForUnregisteredUserDebug(_logger, deviceUser.Id, deviceIdentifier);
            inputTracker = CreateTracker(deviceUser.Id, _configurationProvider.Configuration, deviceUser.ActiveScheme);
        }

        return inputTracker;
    }

    private IUserInputTracker CreateTracker(int userId, InputSystemConfiguration configuration, ActiveInputScheme activeScheme)
    {
        var (inputDefinition, inputScheme) = GetInputScheme(configuration, activeScheme);
        var schemeActionMap = configuration.GetSchemeMap(inputDefinition.Name, inputScheme.Name);

        return _inputTrackerFactory.Invoke(_serviceProvider, [userId, activeScheme, schemeActionMap, configuration.ProcessorConfiguration]);
    }

    #endregion

    #region Logging

    [LoggerMessage(eventId: 1, LogLevel.Warning, "An input was received for a device named '{deviceIdentity}' but no user was found to be paired to it, thus the input will be ignored.")]
    private static partial void LogNoInputUserForDeviceWarning(ILogger logger, InputDeviceIdentity deviceIdentity);

    [LoggerMessage(eventId: 3, LogLevel.Warning, "An attempt was made to use an active scheme with input definition '{desiredDefinitionName}' but it did not exist, defaulting to '{defaultDefinitionName}'.")]
    private static partial void LogInvalidDefinitionUsageWarning(ILogger logger, string desiredDefinitionName, string defaultDefinitionName);

    [LoggerMessage(eventId: 4, LogLevel.Warning, "An attempt was made to use an active scheme '{desiredSchemeName}' with input definition '{definitionName}' but the scheme did not exist, defaulting to '{defaultSchemeName}'.")]
    private static partial void LogInvalidSchemeUsageWarning(ILogger logger, string definitionName, string desiredSchemeName, string defaultSchemeName);

    [LoggerMessage(eventId: 5, LogLevel.Debug, "User {userId} has changed their scheme to use a scheme '{schemeName}' for input definition '{definitionName}'.")]
    private static partial void LogSchemeChangeDebug(ILogger logger, int userId, string definitionName, string schemeName);

    [LoggerMessage(eventId: 6, LogLevel.Debug, "User {userId} has joined the session, creating new input tracker.")]
    private static partial void LogUserJoinedDebug(ILogger logger, int userId);

    [LoggerMessage(eventId: 7, LogLevel.Debug, "User {userId} has been removed from the session, removing input tracker.")]
    private static partial void LogUserRemovedDebug(ILogger logger, int userId);

    [LoggerMessage(eventId: 8, LogLevel.Information, "A user notification, of type '{userEventTypeName}', was sent that was not supported.")]
    private static partial void LogUnknownUserNotificationInformation(ILogger logger, string userEventTypeName);

    [LoggerMessage(eventId: 9, LogLevel.Information, "An input device, '{deviceIdentifier}', sent input but was not paird due to the policy's manual device handling")]
    private static partial void LogUnpairdDeviceDueToPolicyInformation(ILogger logger, RuntimeDeviceIdentifier deviceIdentifier);

    [LoggerMessage(eventId: 10, LogLevel.Information, "An input device, '{deviceIdentifier}, sent input but was not paired because it was not part of a support device combination.")]
    private static partial void LogUnpairedDeviceDueToUnsupportedCombinationInformation(ILogger logger, RuntimeDeviceIdentifier deviceIdentifier);

    [LoggerMessage(eventId: 11, LogLevel.Debug, "A new input device, '{deviceIdentifier}', sent input and no user was found to possess it, new user being created due to policy's settings.")]
    private static partial void LogNewUserCreateDebug(ILogger logger, RuntimeDeviceIdentifier deviceIdentifier);

    [LoggerMessage(eventId: 12, LogLevel.Debug, "An input device, '{deviceIdentifier}', sent input for user {userId} but user input has not been received yet, creating new input tracker.")]
    private static partial void LogNewInputTrackerForUnregisteredUserDebug(ILogger logger, int userId, RuntimeDeviceIdentifier deviceIdentifier);

    [LoggerMessage(eventId: 13, LogLevel.Debug, "Input processing pause state changed, tracking input: {pause}")]
    private static partial void LogTogglePauseDebug(ILogger logger, bool pause);

    [LoggerMessage(eventId: 14, LogLevel.Information, "Input was received for input type '{inputTypeName}' but it is not a supported input type, ignoring int processing.")]
    private static partial void LogUnsupportedInputTypeInformation(ILogger logger, string inputTypeName);

    [LoggerMessage(eventId: 15, LogLevel.Debug, "Input received, from device {deviceIdentifier}, for user {userId} has triggered an action '{actionName}' for input scheme {activeScheme}.")]
    private static partial void LogInputActionTriggeredDebug(ILogger logger, int userId, RuntimeDeviceIdentifier deviceIdentifier, ActiveInputScheme activeScheme, string actionName);
    
    #endregion
}
