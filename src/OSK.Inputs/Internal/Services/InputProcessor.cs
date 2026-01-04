using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OSK.Functions.Outputs.Abstractions;
using OSK.Functions.Outputs.Logging.Abstractions;
using OSK.Inputs.Abstractions;
using OSK.Inputs.Abstractions.Configuration;
using OSK.Inputs.Abstractions.Notifications;
using OSK.Inputs.Abstractions.Runtime;
using OSK.Inputs.Options;
using OSK.Inputs.Ports;
using OSK.Inputs.Internal.Services;

namespace OSK.Inputs.Internal.Services;

internal partial class InputProcessor: IInputProcessor
{
    #region Variables

    internal bool _pauseInputProcessing;
    private readonly Dictionary<int, IInputUserTracker> _userInputTrackerLookup = [];
    private readonly Dictionary<RuntimeDeviceIdentifier, int> _registeredUserDevices = [];

    private readonly IInputUserManager _userManager;
    private readonly IInputNotificationPublisher _notificationPublisher;
    private readonly IInputConfigurationProvider _configurationProvider;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<InputProcessor> _logger;
    private readonly IOutputFactory<InputProcessor> _outputFactory;

    internal readonly ObjectFactory<InputUserInputTracker> _userInputTrackerFactory
        = ActivatorUtilities.CreateFactory<InputUserInputTracker>([typeof(int), typeof(ActiveInputScheme),
            typeof(InputSchemeActionMap), typeof(InputProcessorConfiguration)]);

    private readonly Func<int, ActiveInputScheme, InputSchemeActionMap, InputProcessorConfiguration, IInputUserTracker> _newInputTrackerFactory;

    #endregion

    #region Constructors

    public InputProcessor(IInputUserManager userManager, IInputNotificationPublisher notificationPublisher, 
        IInputConfigurationProvider configurationProvider, IServiceProvider serviceProvider, ILogger<InputProcessor> logger,
        IOutputFactory<InputProcessor> outputFactory)
    {
        _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        _configurationProvider = configurationProvider ?? throw new ArgumentNullException(nameof(configurationProvider));
        _notificationPublisher = notificationPublisher ?? throw new ArgumentNullException(nameof(notificationPublisher));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _outputFactory = outputFactory ?? throw new ArgumentNullException(nameof(outputFactory));

        notificationPublisher.OnUserNotification += HandleUserEvent;
        _newInputTrackerFactory = (userId, activeScheme, schemeActionMap, processorConfiguration)
            => _userInputTrackerFactory(_serviceProvider, [userId, activeScheme, schemeActionMap, processorConfiguration]);
    }

    internal InputProcessor(IInputUserManager userManager, IInputNotificationPublisher notificationPublisher,
        IInputConfigurationProvider configurationProvider, IServiceProvider serviceProvider, ILogger<InputProcessor> logger,
        IOutputFactory<InputProcessor> outputFactory,
        Func<int, ActiveInputScheme, InputSchemeActionMap, InputProcessorConfiguration, IInputUserTracker> customTrackerFactory,
        Dictionary<int, IInputUserTracker> trackerDictionary)
        : this(userManager, notificationPublisher, configurationProvider, serviceProvider, logger, outputFactory)
    {
        _newInputTrackerFactory = customTrackerFactory ?? throw new ArgumentNullException(nameof(customTrackerFactory));
        _userInputTrackerLookup = trackerDictionary ?? throw new ArgumentNullException(nameof(trackerDictionary));
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

    public IOutput ProcessEvent(InputEvent inputEvent)
    {
        if (inputEvent is null)
        {
            throw new ArgumentNullException(nameof(inputEvent));
        }
        if (_pauseInputProcessing)
        {
            return _outputFactory.Fail("Input processing paused");
        }
        if (inputEvent is not PhysicalInputEvent physicalInputEvent)
        {
            LogUnsupportedInputTypeInformation(_logger, inputEvent.GetType().FullName);
            return _outputFactory.Fail($"Input type '{inputEvent.Input.GetType().FullName}' is not supported.");
        }

        var inputTracker = GetInputTrackerForDevice(physicalInputEvent.DeviceIdentifier);
        if (inputTracker is null)
        {
            return _outputFactory.Fail($"Unrecognized error, unable to get an input tracker for the device or user.");
        }

        var triggeredActionOutput = inputTracker.Track(inputEvent);
        if (triggeredActionOutput.IsSuccessful && triggeredActionOutput.Value is not null)
        {
            LogInputActionTriggeredDebug(_logger, inputTracker.UserId, physicalInputEvent.DeviceIdentifier, inputTracker.ActiveScheme, 
                triggeredActionOutput.Value.Value.ActionMap.Action.Name);
            triggeredActionOutput.Value.Value.Execute();
        }

        return triggeredActionOutput;
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

    private (InputDefinition, InputScheme) GetViableInputScheme(InputSystemConfiguration configuration, ActiveInputScheme activeScheme)
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

        var targetUser = GetUserForDevicePairing(configuration.SupportedDeviceCombinations, _userManager.GetUsers(), 
            configuration.JoinPolicy.DeviceJoinBehavior, deviceIdentifier.Identity);
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

        var pairedOutput = _userManager.PairDevice(targetUser.Id, deviceIdentifier);
        if (!pairedOutput.IsSuccessful)
        {
            LogDevicePairingFailedWarning(_logger, targetUser.Id, deviceIdentifier);
            _notificationPublisher.Notify(new DevicePairingFailedNotification(targetUser.Id, deviceIdentifier));
            return null;
        }

        return targetUser;
    }

    private IInputUserTracker? GetInputTrackerForDevice(RuntimeDeviceIdentifier deviceIdentifier)
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

    private IInputUserTracker CreateTracker(int userId, InputSystemConfiguration configuration, ActiveInputScheme activeScheme)
    {
        var (inputDefinition, inputScheme) = GetViableInputScheme(configuration, activeScheme);
        var schemeActionMap = configuration.GetSchemeMap(inputDefinition.Name, inputScheme.Name);
        if (schemeActionMap is null)
        {
            throw new InvalidOperationException($"Scheme action map for user input tracker was null, but this should not have been possible; definition: {inputDefinition.Name}, scheme: {inputScheme.Name}.");
        }

        return _newInputTrackerFactory(userId, activeScheme, schemeActionMap, configuration.ProcessorConfiguration);
    }

    private IInputUser? GetUserForDevicePairing(IEnumerable<InputDeviceCombination> supportedDeviceCombinations,
        IEnumerable<IInputUser> users, DevicePairingBehavior pairingBehavior, InputDeviceIdentity newDeviceIdentity)
    {
        if (!users.Any())
        {
            return null;
        }

        var deviceCombinationLookup = supportedDeviceCombinations.SelectMany(combination
            => combination.DeviceIdentities.Select(identity => new { DeviceIdentity = identity, Combination = combination }))
            .GroupBy(deviceIdentityCombinations => deviceIdentityCombinations.DeviceIdentity)
            .ToDictionary(deviceIdentityCombinationGroup => deviceIdentityCombinationGroup.Key,
                deviceIdentityCombinationGroup 
                    => deviceIdentityCombinationGroup.Select(identityCombinationPair => identityCombinationPair.Combination)
                                                     .Distinct()
                                                     .ToArray());

        var userDevicePairingData = users.Select(user =>
        {
            var pairedDeviceSet = user.PairedDevices.Select(pairedDevice => pairedDevice.DeviceIdentifier.Identity).ToHashSet();
            var completedCombinations = supportedDeviceCombinations.Count(combination => combination.DeviceIdentities.All(identity => pairedDeviceSet.Contains(identity)));
            var missingNewDevice = !pairedDeviceSet.Contains(newDeviceIdentity);
            var fewestDevicesToCompleteClosestCombinationWithDevice = missingNewDevice
                ? supportedDeviceCombinations
                    .Where(combination => combination.Contains(newDeviceIdentity))
                    .Select(combination => combination.DeviceIdentities.Count(identity => !pairedDeviceSet.Contains(identity)))
                    .Min()
                : 100;

            return new
            {
                User = user,
                IsMissingNewDevice = missingNewDevice,
                TotalPairedDevices = user.PairedDevices.Count,
                TotalCombinationsCompleted = completedCombinations,
                FewestDevicesToCompleteClosestCombinationWithDevice = fewestDevicesToCompleteClosestCombinationWithDevice
            };
        });

        IInputUser? pairedUser = null;
        switch (pairingBehavior)
        {
            case DevicePairingBehavior.Balanced:
                pairedUser = userDevicePairingData.OrderByDescending(pairingData => pairingData.IsMissingNewDevice)
                                                  .ThenBy(pairingData => pairingData.FewestDevicesToCompleteClosestCombinationWithDevice)
                                                  .ThenBy(pairingData => pairingData.TotalPairedDevices)
                                                  .ThenBy(pairingData => pairingData.TotalCombinationsCompleted)
                                                  .FirstOrDefault()?.User;
                break;
        }

        return pairedUser;
    }

    #endregion
}
