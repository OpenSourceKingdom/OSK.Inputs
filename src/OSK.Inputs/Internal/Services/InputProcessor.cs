using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OSK.Inputs.Abstractions;
using OSK.Inputs.Abstractions.Configuration;
using OSK.Inputs.Abstractions.Devices;
using OSK.Inputs.Abstractions.Notifications;
using OSK.Inputs.Abstractions.Runtime;
using OSK.Inputs.Options;
using OSK.Inputs.Ports;
using OSK.Operations.Outputs;
using OSK.Operations.Outputs.Models;

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

    internal readonly ObjectFactory<InputUserInputTracker> _userInputTrackerFactory
        = ActivatorUtilities.CreateFactory<InputUserInputTracker>([typeof(int),
            typeof(InputSchemeActionMap), typeof(InputSystemConfiguration)]);

    private readonly Func<int, InputSchemeActionMap, InputSystemConfiguration, IInputUserTracker> _newInputTrackerFactory;

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
        notificationPublisher.OnDeviceNotification += deviceNotification =>
        {
            if (deviceNotification is DeviceUnpairedNotification deviceUnpairedNotification)
            {
                _registeredUserDevices.Remove(deviceUnpairedNotification.DeviceIdentifier);
            }
        };

        _newInputTrackerFactory = (userId, schemeActionMap, processorConfiguration)
            => _userInputTrackerFactory(_serviceProvider, [userId, schemeActionMap, processorConfiguration]);
    }

    internal InputProcessor(IInputUserManager userManager, IInputNotificationPublisher notificationPublisher,
        IInputConfigurationProvider configurationProvider, IServiceProvider serviceProvider, ILogger<InputProcessor> logger,
        Func<int, InputSchemeActionMap, InputSystemConfiguration, IInputUserTracker> customTrackerFactory,
        Dictionary<int, IInputUserTracker> trackerDictionary)
        : this(userManager, notificationPublisher, configurationProvider, serviceProvider, logger)
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

    public Output<ProcessedInputResult> ProcessEvent(TimeSpan deltaTime, InputEvent inputEvent, InputEventProcessOptions options)
    {
        if (inputEvent is null)
        {
            throw new ArgumentNullException(nameof(inputEvent));
        }
        if (options is null)
        {
            throw new ArgumentNullException(nameof(options));
        }
        if (_pauseInputProcessing)
        {
            return Out.InvalidRequest<ProcessedInputResult>("Input processing paused");
        }
        if (inputEvent is not DeviceInputEvent deviceInputEvent)
        {
            LogUnsupportedInputTypeInformation(_logger, inputEvent.GetType().FullName);
            return Out.InvalidRequest<ProcessedInputResult>($"Input type '{inputEvent.GetType().FullName}' is not supported.");
        }

        var inputTracker = GetInputTrackerForDevice(deviceInputEvent.DeviceIdentifier);
        if (inputTracker is null)
        {
            return Out.InvalidRequest<ProcessedInputResult>($"Unrecognized error, unable to get an input tracker for the device or user.");
        }

        var processedInputEvent = inputTracker.Track(deltaTime, inputEvent);
        if (!processedInputEvent.IsSuccessful)
        {
            return processedInputEvent.As<ProcessedInputResult>();
        }

        var actionSuppressed = processedInputEvent.Data.ActionMap?.Action.ActionGroup is not null &&
            options.SuppressedActionGroups.Contains(processedInputEvent.Data.ActionMap.Action.ActionGroup.Value);
        if (!actionSuppressed && processedInputEvent.Data.Triggered)
        {
            LogInputActionTriggeredDebug(_logger, inputTracker.UserId, deviceInputEvent.DeviceIdentifier, inputTracker.ActiveScheme, 
                processedInputEvent.Data.ActionMap.Action?.Name ?? "{Passive Action}");
            processedInputEvent.Data.Execute();
        }

        return Out.Success(new ProcessedInputResult()
        {
            MatchedAction = processedInputEvent.Data.ActionMap,
            ConsumedInput = !actionSuppressed && (processedInputEvent.Data.ActivationContext?.ConsumedInput ?? false)
        });
    }

    public void ToggleInputProcessing(bool pause)
    {
        LogTogglePauseDebug(_logger, pause);
        _pauseInputProcessing = pause;
    }

    public void ProcessMessage(IInputProcessorMessage processorMessage)
    {
        if (processorMessage is null)
        {
            throw new ArgumentNullException(nameof(processorMessage));
        }

        switch (processorMessage)
        {
            case DeviceStateChangedNotification deviceNotification:
                var user = _userManager.GetInputUserForDevice(deviceNotification.DeviceIdentifier.DeviceId);
                if (user is null)
                {
                    _notificationPublisher.Notify(deviceNotification);
                    return;
                }

                var device = user.GetDevice(deviceNotification.DeviceIdentifier.DeviceId);
                UserDeviceNotification userDeviceNotification = deviceNotification.Status is DeviceStatus.Disconnected
                    ? new UserDeviceDisconnectedNotification(user, deviceNotification.DeviceIdentifier)
                    : new UserDeviceConnectedNotification(user, deviceNotification.DeviceIdentifier);

                if (_userInputTrackerLookup.TryGetValue(user.Id, out var deviceTracker))
                {
                    deviceTracker.ResetInput(deviceNotification.DeviceIdentifier.DeviceFamily);
                }

                _notificationPublisher.Notify(userDeviceNotification);

                break;
            case InputSystemFocusLostEvent:
                foreach (var tracker in _userInputTrackerLookup.Values)
                {
                    foreach (var deviceFamily in tracker.ActiveScheme.DeviceFamilies)
                    {
                        tracker.ResetInput(deviceFamily);
                    }
                }
                break;
        }
    }

    #endregion

    #region Helpers
    
    private void HandleUserEvent(InputUserNotification userEvent)
    {
        switch (userEvent)
        {
            case InputUserActiveDefinitionChangeNotification definitionChangeNotification:
                if (_userInputTrackerLookup.TryGetValue(definitionChangeNotification.User.Id, out var tracker))
                {
                    CreateOrUpdateTracker(_configurationProvider.Configuration, definitionChangeNotification.User, tracker.ActiveScheme.DeviceFamilies);
                }
                LogDefinitionChangeDebug(_logger, definitionChangeNotification.User.Id, definitionChangeNotification.ActiveDefinitionName);
                break;
            case InputUserJoinedNotification userJoinedEvent:
                LogUserJoinedDebug(_logger, userJoinedEvent.User.Id);
                break;
            case InputUserRemovedNotification userRemovedEvent:
                _userInputTrackerLookup.Remove(userRemovedEvent.User.Id);
                LogUserRemovedDebug(_logger, userRemovedEvent.User.Id);
                break;
            default:
                LogUnknownUserNotificationInformation(_logger, userEvent.GetType().FullName);
                break;
        }
    }

    private ActiveInputScheme? GetActiveInputScheme(InputSystemConfiguration configuration, IInputUser user, InputDeviceFamily[] deviceFamilies)
    {
        var inputDefinition = configuration.GetDefinition(user.ActiveInputDefinitionName);
        if (inputDefinition is null)
        {
            inputDefinition = configuration.Definitions.FirstOrDefault(definition => definition.IsDefault) ?? configuration.Definitions.First();
            LogInvalidDefinitionUsageWarning(_logger, user.ActiveInputDefinitionName, inputDefinition.Name);
        }

        var deviceCombinationId = InputDeviceCombination.GetCombinationId(deviceFamilies);
        var potentialSchemes = inputDefinition.GetSchemesByDevicecCombination(deviceCombinationId);
        if (!potentialSchemes.Any())
        {
            // Say search fails for keyboard but one exists for keyboard and mouse. Or, say we fail to find an Xbox scheme but there is a game pad scheme that could
            // somewhat work (most maps will function for all game pads, except for special functions). i.e. need to widen search
            var closestSupportedCombination = configuration.SupportedDeviceCombinations.Select(combination => new
                {
                    Combination = combination,
                    SupportConfidence = deviceFamilies.Sum(combination.GetDeviceSupportConfidence)
                })
                .Where(combinationSupportConfidence => combinationSupportConfidence.SupportConfidence > 0)
                .OrderByDescending(combinationSupportConfidence => combinationSupportConfidence.SupportConfidence)
                .FirstOrDefault();

            if (closestSupportedCombination is null)
            {
                LogUnsupportedDeviceFamiliesWarning(_logger, inputDefinition.Name, string.Join(", ", deviceFamilies.Select(family => family.Name)));
                return null;
            }

            deviceCombinationId = InputDeviceCombination.GetCombinationId(closestSupportedCombination.Combination.DeviceFamilies);
            potentialSchemes = inputDefinition.GetSchemesByDevicecCombination(deviceCombinationId);
        }

        var preferredScheme = user.GetPreferredInputScheme(user.ActiveInputDefinitionName, deviceCombinationId);
        var scheme = preferredScheme.HasValue 
            ? potentialSchemes.FirstOrDefault(s => s.Name.Equals(preferredScheme.Value.SchemeName, StringComparison.OrdinalIgnoreCase)) ?? potentialSchemes.First()
            : potentialSchemes.FirstOrDefault(s => s.IsDefault) ?? potentialSchemes.First();

        return new ActiveInputScheme(user.ActiveInputDefinitionName, scheme.Name, [.. scheme.GetDeviceFamilies() ]);
    }

    private IInputUser? TryDevicePairing(InputSystemConfiguration configuration, RuntimeDeviceIdentifier deviceIdentifier)
    {
        if (configuration.JoinPolicy.DeviceJoinBehavior is DevicePairingBehavior.Manual)
        {
            LogUnpairdDeviceDueToPolicyInformation(_logger, deviceIdentifier);
            return null; 
        }

        var supportedDeviceCombination = configuration.SupportedDeviceCombinations
                                        .Where(deviceCombination => deviceCombination.DeviceFamilies.Contains(deviceIdentifier.DeviceFamily))
                                        .Cast<InputDeviceCombination?>()
                                        .FirstOrDefault();
        if (supportedDeviceCombination is null)
        {
            LogUnpairedDeviceDueToUnsupportedCombinationInformation(_logger, deviceIdentifier);
            return null;
        }

        var targetUser = GetUserForDevicePairing(configuration.SupportedDeviceCombinations, _userManager.GetUsers(), 
            configuration.JoinPolicy.DeviceJoinBehavior, deviceIdentifier.DeviceFamily);
        if (targetUser is null)
        {
            LogNewUserCreateDebug(_logger, deviceIdentifier);
            var createUserOutput = _userManager.CreateUser(new UserJoinOptions());
            if (!createUserOutput.IsSuccessful)
            {
                return null;
            }

            targetUser = createUserOutput.Data;
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
            return CreateOrUpdateTracker(_configurationProvider.Configuration, _userManager.GetUser(userId)!, [deviceIdentifier.DeviceFamily]);
        }

        var deviceUser = _userManager.GetInputUserForDevice(deviceIdentifier.DeviceId) ?? TryDevicePairing(_configurationProvider.Configuration, deviceIdentifier);
        if (deviceUser is null)
        {
            LogNoInputUserForDeviceWarning(_logger, deviceIdentifier.DeviceFamily);
            _notificationPublisher.Notify(new UnrecognizedDeviceNotification(deviceIdentifier));
            return null;
        }

        _registeredUserDevices[deviceIdentifier] = deviceUser.Id;
        if (_userInputTrackerLookup.TryGetValue(deviceUser.Id, out var inputTracker))
        {
            return inputTracker;
        }

        LogNewInputTrackerForUnregisteredUserDebug(_logger, deviceUser.Id, deviceIdentifier);
        return CreateOrUpdateTracker(_configurationProvider.Configuration, deviceUser, [deviceIdentifier.DeviceFamily]);
    }

    private IInputUserTracker? CreateOrUpdateTracker(InputSystemConfiguration configuration, IInputUser user, InputDeviceFamily[] deviceFamilies)
    {
        if (_userInputTrackerLookup.TryGetValue(user.Id, out var inputTracker)
             && user.ActiveInputDefinitionName.Equals(inputTracker.ActiveScheme.DefinitionName, StringComparison.OrdinalIgnoreCase)
                 && deviceFamilies.Any(inputTracker.ActiveScheme.DeviceFamilies.Contains))
        {
            return inputTracker;
        }
        
        var activeScheme = GetActiveInputScheme(configuration, user, deviceFamilies);
        if (activeScheme is null)
        {
            return null;
        }

        var schemeActionMap = configuration.GetSchemeMap(activeScheme.Value.DefinitionName, activeScheme.Value.DeviceCombinationId, activeScheme.Value.SchemeName);
        if (schemeActionMap is null)
        {
            throw new InvalidOperationException($"Scheme action map for user input tracker was null, but this should not have been possible; definition: {activeScheme.Value.DefinitionName}, scheme: {activeScheme.Value.SchemeName}.");
        }

        LogNewActiveSchemeInformation(_logger, user.Id, string.Join(", ", deviceFamilies.Select(family => family.Name)), schemeActionMap.DefinitionName, schemeActionMap.SchemeName);
        _userInputTrackerLookup[user.Id] = _newInputTrackerFactory(user.Id, schemeActionMap, configuration);
        return _userInputTrackerLookup[user.Id];
    }

    private IInputUser? GetUserForDevicePairing(IEnumerable<InputDeviceCombination> supportedDeviceCombinations,
        IEnumerable<IInputUser> users, DevicePairingBehavior pairingBehavior, InputDeviceFamily newdeviceFamily)
    {
        if (!users.Any())
        {
            return null;
        }

        var deviceCombinationLookup = supportedDeviceCombinations.SelectMany(combination
            => combination.DeviceFamilies.Select(identity => new { DeviceFamily = identity, Combination = combination }))
            .GroupBy(deviceFamilyCombinations => deviceFamilyCombinations.DeviceFamily)
            .ToDictionary(deviceFamilyCombinationGroup => deviceFamilyCombinationGroup.Key,
                deviceFamilyCombinationGroup 
                    => deviceFamilyCombinationGroup.Select(identityCombinationPair => identityCombinationPair.Combination)
                                                     .Distinct()
                                                     .ToArray());

        var userDevicePairingData = users.Select(user =>
        {
            var pairedDeviceSet = user.PairedDevices.Select(pairedDevice => pairedDevice.DeviceIdentifier.DeviceFamily).ToHashSet();
            var completedCombinations = supportedDeviceCombinations.Count(combination => combination.DeviceFamilies.All(identity => pairedDeviceSet.Contains(identity)));
            var missingNewDevice = !pairedDeviceSet.Contains(newdeviceFamily);
            var fewestDevicesToCompleteClosestCombinationWithDevice = missingNewDevice
                ? supportedDeviceCombinations
                    .Where(combination => combination.Contains(newdeviceFamily))
                    .Select(combination => combination.DeviceFamilies.Count(identity => !pairedDeviceSet.Contains(identity)))
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
