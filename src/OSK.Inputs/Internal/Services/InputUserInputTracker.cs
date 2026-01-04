using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using OSK.Functions.Outputs.Abstractions;
using OSK.Functions.Outputs.Logging.Abstractions;
using OSK.Inputs.Abstractions.Configuration;
using OSK.Inputs.Abstractions.Inputs;
using OSK.Inputs.Abstractions.Runtime;
using OSK.Inputs.Internal.Models;
using OSK.Inputs.Internal.Services;
using OSK.Inputs.Internal;

namespace OSK.Inputs.Internal.Services;

internal partial class InputUserInputTracker(int userId, ActiveInputScheme scheme, InputSchemeActionMap schemeMap, 
    InputProcessorConfiguration processorConfiguration, ILogger<InputUserInputTracker> logger, IOutputFactory<InputUserInputTracker> outputFactory,
    IServiceProvider serviceProvider): IInputUserTracker
{
    #region Variables

    private const int MaxPointerRecords = 3;

    private readonly Dictionary<InputDeviceIdentity, DeviceInputTracker> _deviceInputTrackerLookup
        = schemeMap.DeviceSchemeMaps.ToDictionary(
            deviceScheme => deviceScheme.DeviceIdentity, 
            deviceScheme => new DeviceInputTracker(deviceScheme));

    #endregion

    #region IUserInputTracker

    public ActiveInputScheme ActiveScheme => scheme;

    public int UserId => userId;

    public IEnumerable<TriggeredActionEvent> Update(TimeSpan deltaTime)
    {
        var removalDelay = processorConfiguration.TapDelayTime.GetValueOrDefault(defaultValue: TimeSpan.Zero);

        var triggeredActions = new List<TriggeredActionEvent>();
        foreach (var deviceTracker in _deviceInputTrackerLookup.Values)
        {
            var inputsToRemove = new List<InputState>();
            foreach (var inputState in deviceTracker.AllStates)
            {
                if (inputState.Phase is InputPhase.End)
                {
                    if (inputState.InactiveDuration.GetValueOrDefault(TimeSpan.Zero) >= removalDelay)
                    {
                        inputsToRemove.Add(inputState);
                    }
                    continue;
                }
                
                TriggeredActionEvent? triggeredAction;
                var reprocess = false;
                switch (inputState.Phase)
                {
                    case InputPhase.Start:
                        inputState.Phase = InputPhase.Active;
                        reprocess = true;
                        break;
                    case InputPhase.Active:
                        inputState.Duration += deltaTime;
                        break;
                }

                var inputEvent = GetEventForState(inputState);
                triggeredAction = inputEvent is not null && inputState.MappedAction is not null
                    ? reprocess 
                        ? Track(inputEvent).Value 
                        : GetTriggeredActivation(inputState, inputEvent, inputState.MappedAction)
                    : null;

                if (triggeredAction is not null)
                {
                    triggeredActions.Add(triggeredAction.Value);
                }
            }

            foreach (var state in inputsToRemove)
            {
                deviceTracker.RemoveState(state);
            }
        }

        return triggeredActions;
    }

    public IOutput<TriggeredActionEvent?> Track(InputEvent inputEvent)
    {
        if (inputEvent is not PhysicalInputEvent physicalInputEvent)
        {
            return outputFactory.Fail<TriggeredActionEvent?>("The input event was not a physical input event");
        }
        if (!_deviceInputTrackerLookup.TryGetValue(physicalInputEvent.DeviceIdentifier.Identity, out var deviceTracker))
        {
            return outputFactory.Fail<TriggeredActionEvent?>("No device tracker was found for the device triggering the input.");
        }

        var actionMaps = deviceTracker.SchemeMap.GetActionMaps(physicalInputEvent.Input.Id);
        if (!actionMaps.Any())
        {
            return outputFactory.Fail<TriggeredActionEvent?>("No action map found for the input");
        }

        var inputState = GetAndUpdateInputState(deviceTracker, physicalInputEvent);
        if (inputState is null)
        {
            return outputFactory.Fail<TriggeredActionEvent?>("Unable to acquire input state");
        }

        var virtualActionMaps = actionMaps.Where(map => map.Input is VirtualInput);
        var inputActionMap = actionMaps.FirstOrDefault(map => map.Input is PhysicalInput);


        var virtualInputActivationContext = ProcessVirtualInputEvent(deviceTracker, inputState, virtualActionMaps);
        var triggeredActivation = virtualInputActivationContext is null && inputActionMap is not null 
                && inputActionMap.Action.TriggerPhases.Contains(inputState.Phase)
            ? GetTriggeredActivation(inputState, physicalInputEvent, inputActionMap)
            : virtualInputActivationContext;

        inputState.MappedAction = triggeredActivation?.ActionMap;

        if (triggeredActivation is not null)
        {
            triggeredActivation.Value.Execute();
        }

        if (inputState.Phase is InputPhase.End && processorConfiguration.TapDelayTime is null)
        {
            deviceTracker.RemoveState(inputState);
        }

        return outputFactory.Succeed(triggeredActivation);
    }

    #endregion

    #region Helpers

    private PhysicalInputState? GetAndUpdateInputState(DeviceInputTracker deviceTracker, InputEvent inputEvent)
    {
        PhysicalInputState inputState;
        switch (inputEvent)
        {
            case InputPointerEvent pointerEvent:
                var pointerState = deviceTracker.GetOrCreatePointerState(pointerEvent.PointerId, () =>
                {
                    return new InputPointerState(pointerEvent.PointerId, pointerEvent.Input, MaxPointerRecords)
                    {
                        DeviceIdentifier = pointerEvent.DeviceIdentifier,
                        Phase = pointerEvent.Phase,
                        Duration = TimeSpan.Zero
                    };
                });
                pointerState.AddRecord(pointerEvent.Position);

                inputState = pointerState;
                break;
            case InputPowerEvent powerEvent:
                var inputPowerState = deviceTracker.GetOrCreatePowerState(powerEvent.Input.Id, () =>
                {
                    return new InputPowerState(powerEvent.Input)
                    {
                        DeviceIdentifier = powerEvent.DeviceIdentifier,
                        Duration = TimeSpan.Zero,
                        InputPowers = []
                    };
                });

                inputPowerState.InputPowers = [.. powerEvent.InputIntensities];
                if (inputPowerState.Phase is InputPhase.End && inputEvent.Phase is InputPhase.Start)
                {
                    inputPowerState.TapCount += 1;
                    inputPowerState.Duration = TimeSpan.Zero;
                }

                inputState = inputPowerState;
                break;
            default:
                LogUnknownActivationWarning(logger, inputEvent.GetType().FullName);
                return null;
        }

        inputState.Phase = inputEvent.Phase;
        inputState.InactiveDuration = inputState.Phase is InputPhase.End
            ? TimeSpan.Zero
            : null;

        return inputState;
    }

    private TriggeredActionEvent? ProcessVirtualInputEvent(DeviceInputTracker deviceTracker, PhysicalInputState inputState,
        IEnumerable<InputActionMap> virtualInputActionMaps)
    {
        foreach (var virtualInputActionMap in virtualInputActionMaps)
        {
            switch (virtualInputActionMap.Input)
            {
                case CombinationInput combinationInput:
                    var combinationPhase = inputState.Phase;
                    var completedCombination = true;

                    foreach (var input in combinationInput.DeviceInputs.Where(input => input.Id != inputState.InputId))
                    {
                        var otherInputState = deviceTracker.GetInputPowerState(input.Id);
                        if (otherInputState is null)
                        {
                            completedCombination = false;
                            break;
                        }

                        combinationPhase = DetermineCombinationPhase(combinationPhase, otherInputState.Phase);
                    }

                    if (completedCombination)
                    {
                        return GetTriggeredActivation(inputState, new VirtualInputEvent(combinationInput, combinationPhase), 
                            virtualInputActionMap);
                    }

                    break;
                default:
                    LogUnknownVirtualInputWarning(logger, deviceTracker.SchemeMap.DeviceIdentity, virtualInputActionMap.Input.Id, virtualInputActionMap.Input.GetType().FullName);
                    break;
            }
        }

        return null;
    }

    private InputPhase DetermineCombinationPhase(InputPhase phaseA, InputPhase phaseB)
    {
        // End phase always wins
        if (phaseA is InputPhase.End || phaseB is InputPhase.End)
        {
            return InputPhase.End;
        }
        // Start phase always wins, second
        if (phaseA is InputPhase.Start || phaseB is InputPhase.Start)
        {
            return InputPhase.Start;
        }

        return InputPhase.Active;
    }

    private InputActivityInformation GetActivityInformation(InputState state)
    {
        return new InputActivityInformation()
        {
            TapCount = state is InputPowerState powerState
                ? powerState.TapCount
                : null,
            Duration = state.Duration
        };
    }

    private PointerDetails GetPointerInformation(InputActionMap actionMap)
    {
        if (!actionMap.Action.IncludePointerDetails)
        {
            return PointerDetails.Empty;
        }

        var pointerData = _deviceInputTrackerLookup.Values.Select(deviceState
            => deviceState.PointerStates.Select(pointerState 
                =>
            {
                var pointerPositionMotionData = pointerState.GetCurrentPositionAndMotionData();
                if (pointerPositionMotionData is null)
                {
                    return null;
                }

                return (PointerData?) new PointerData(pointerState.PointerId, deviceState.SchemeMap.DeviceIdentity,
                    pointerPositionMotionData.Value.Item1, pointerPositionMotionData.Value.Item2);
            }))
            .Where(pointerData => pointerData is not null)
            .Cast<PointerData>()
            .ToArray();

        return new PointerDetails(pointerData);
    }
    
    private InputEvent? GetEventForState(InputState state)
    {
        switch (state)
        {
            case InputPowerState powerState:
                return new InputPowerEvent(powerState.DeviceIdentifier, powerState.Input, powerState.Phase, powerState.InputPowers);
            case InputPointerState pointerState:
                var pointerPositionAndMotionData = pointerState.GetCurrentPositionAndMotionData();
                return pointerPositionAndMotionData is null
                    ? null
                    : new InputPointerEvent(pointerState.DeviceIdentifier, pointerState.Input, pointerState.Phase, pointerState.PointerId,
                            pointerPositionAndMotionData.Value.Item1);
            default:
                return null;
        }
    }

    private TriggeredActionEvent GetTriggeredActivation(InputState state, InputEvent activation, InputActionMap actionMap)
        => new(ActiveScheme, actionMap,
                new InputEventContext(userId, activation, GetPointerInformation(actionMap),
                GetActivityInformation(state), serviceProvider));

    #endregion

    #region Logging

    [LoggerMessage(eventId: 1, LogLevel.Warning, "Input Activation was of an unknown type and could not be processed: {activationTypeName}")]
    private static partial void LogUnknownActivationWarning(ILogger logger, string activationTypeName);

    [LoggerMessage(eventId: 2, LogLevel.Warning, "An attempt was made to process a virtual input with id '{virtualInputId}' on device '{deviceIdentity}', but it was unrecognized type '{virtualInputType}' and could not be processed.")]
    private static partial void LogUnknownVirtualInputWarning(ILogger logger, InputDeviceIdentity deviceIdentity, int virtualInputId, string virtualInputType);

    #endregion
}
