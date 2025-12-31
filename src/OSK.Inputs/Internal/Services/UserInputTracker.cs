using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using OSK.Inputs.Abstractions;
using OSK.Inputs.Abstractions.Configuration;
using OSK.Inputs.Internal.Models;

namespace OSK.Inputs.Internal;

internal partial class UserInputTracker(int userId, ActiveInputScheme scheme, InputSchemeActionMap schemeMap, 
    InputProcessorConfiguration processorConfiguration, ILogger<UserInputTracker> logger)
{
    #region Variables

    private const int MaxPointerRecords = 3;

    private readonly Dictionary<InputDeviceIdentity, DeviceInputTracker> _deviceInputTrackerLookup
        = schemeMap.DeviceSchemeMaps.ToDictionary(
            deviceScheme => deviceScheme.DeviceIdentity, 
            deviceScheme => new DeviceInputTracker(deviceScheme));

    private readonly Dictionary<int, VirtualInput> _virtualInputLookup 
        = schemeMap.VirtualInputs.ToDictionary(input => input.Id);

    #endregion

    #region Api

    public ActiveInputScheme ActiveScheme => scheme;

    public IEnumerable<TriggeredActivation> Process(TimeSpan deltaTime)
    {
        var removalDelay = processorConfiguration.TapActivationTime.GetValueOrDefault(defaultValue: TimeSpan.Zero);

        var triggeredActivations = new List<TriggeredActivation>();
        foreach (var deviceTracker in _deviceInputTrackerLookup.Values)
        {
            var inputsToRemove = new List<InputState>();
            foreach (var inputState in deviceTracker.AllStates)
            {
                switch (inputState.Phase)
                {
                    case InputPhase.End:
                        if (inputState.InactiveDuration.GetValueOrDefault(TimeSpan.Zero) >= removalDelay)
                        {
                            inputsToRemove.Add(inputState);
                        }
                        continue;
                    case InputPhase.Start:
                        inputState.Phase = InputPhase.Active;
                        break;
                    case InputPhase.Active:
                        inputState.Duration += deltaTime;
                        break;
                }

                var activation = GetActivationForState(inputState);
                var actionMap = activation is null
                    ? null
                    : deviceTracker.SchemeMap.GetActionMap(activation.Input.Id);
                var triggeredActivation = activation is not null && actionMap is not null
                    ? GetTriggeredActivation(inputState, activation, actionMap)
                    : (TriggeredActivation?)null;
                if (triggeredActivation is not null)
                {
                    triggeredActivations.Add(triggeredActivation.Value);
                }
            }

            foreach (var state in inputsToRemove)
            {
                deviceTracker.RemoveState(state);
            }
        }

        return triggeredActivations;
    }

    public TriggeredActivation? Track(InputActivation inputActivation)
    {
        if (inputActivation is not DeviceInputActivation activation)
        {
            return null;
        }
        if (!_deviceInputTrackerLookup.TryGetValue(activation.DeviceIdentifier.Identity, out var deviceTracker))
        {
            return null;
        }

        var actionMap = deviceTracker.SchemeMap.GetActionMap(activation.Input.Id);
        if (actionMap is null)
        {
            return null;
        }

        var inputState = GetAndUpdateInputState(deviceTracker, activation);
        if (inputState is null)
        {
            return null;
        }

        var virtualInputActivationContext = ProcessVirtualInputActivation(deviceTracker, inputState, actionMap);
        var context = virtualInputActivationContext is null && actionMap.Action.TriggerPhases.Contains(inputState.Phase)
            ? GetTriggeredActivation(inputState, activation, actionMap)
            : virtualInputActivationContext;

        if (inputState.Phase is InputPhase.End && processorConfiguration.TapActivationTime is null)
        {
            deviceTracker.RemoveState(inputState);
        }

        return context;
    }

    #endregion

    #region Helpers

    private DeviceInputState? GetAndUpdateInputState(DeviceInputTracker deviceTracker, InputActivation activation)
    {
        DeviceInputState inputState;
        switch (activation)
        {
            case InputPointerActivation pointerActivation:
                var pointerState = deviceTracker.GetOrCreatePointerState(pointerActivation.PointerId, () =>
                {
                    var virtualInputIds = _virtualInputLookup.Values
                                                  .Where(virtualInput => virtualInput.Contains(activation.Input))
                                                  .Select(virtualInput => virtualInput.Id)
                                                  .ToArray();
                    return new InputPointerState(pointerActivation.PointerId, MaxPointerRecords)
                    {
                        Input = pointerActivation.Input,
                        DeviceIdentifier = pointerActivation.DeviceIdentifier,
                        VirtualInputIds = virtualInputIds,
                        Phase = pointerActivation.Phase,
                        Duration = TimeSpan.Zero
                    };
                });
                pointerState.AddRecord(pointerActivation.Position);

                inputState = pointerState;
                break;
            case InputPowerActivation powerActivation:
                var inputPowerState = deviceTracker.GetOrCreatePowerState(powerActivation.Input.Id, () =>
                {
                    var virtualInputIds = _virtualInputLookup.Values
                                                  .Where(virtualInput => virtualInput.Contains(powerActivation.Input))
                                                  .Select(virtualInput => virtualInput.Id);

                    return new InputPowerState()
                    {
                        Input = powerActivation.Input,
                        DeviceIdentifier = powerActivation.DeviceIdentifier,
                        VirtualInputIds = [.. virtualInputIds],
                        Duration = TimeSpan.Zero,
                        InputPowers = []
                    };
                });

                inputPowerState.InputPowers = [.. powerActivation.GetInputPowers()];
                if (inputPowerState.Phase is InputPhase.End && activation.Phase is InputPhase.Start)
                {
                    inputPowerState.TapCount += 1;
                    inputPowerState.Duration = TimeSpan.Zero;
                }

                inputState = inputPowerState;
                break;
            default:
                LogUnknownActivationWarning(logger, activation.GetType().FullName);
                return null;
        }

        inputState.Phase = activation.Phase;
        inputState.InactiveDuration = inputState.Phase is InputPhase.End
            ? TimeSpan.Zero
            : null;

        return inputState;
    }

    private TriggeredActivation? ProcessVirtualInputActivation(DeviceInputTracker deviceTracker, DeviceInputState inputState,
        InputActionMap actionMap)
    {
        var virtualInputs = inputState.VirtualInputIds.Select(virtualInputId => _virtualInputLookup[virtualInputId]);
        foreach (var virtualInput in virtualInputs)
        {
            switch (virtualInput)
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

                        combinationPhase = CombinePhases(combinationPhase, otherInputState.Phase);
                    }

                    if (completedCombination)
                    {
                        return GetTriggeredActivation(inputState, new VirtualInputActivation(combinationInput, combinationPhase), actionMap);
                    }

                    break;
                default:
                    LogUnknownVirtualInputWarning(logger, virtualInput.Name, virtualInput.GetType().FullName);
                    break;
            }
        }

        return null;
    }

    private InputPhase CombinePhases(InputPhase phaseA, InputPhase phaseB)
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
        if (!actionMap.Action.TrackPointer)
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
    
    private InputActivation? GetActivationForState(InputState state)
    {
        switch (state)
        {
            case InputPowerState powerState:
                return new InputPowerActivation(powerState.DeviceIdentifier, powerState.Input, powerState.Phase, powerState.InputPowers);
            case InputPointerState pointerState:
                var pointerPositionAndMotionData = pointerState.GetCurrentPositionAndMotionData();
                return pointerPositionAndMotionData is null
                    ? null
                    : new InputPointerActivation(pointerState.DeviceIdentifier, pointerState.Input, pointerState.Phase, pointerState.PointerId,
                            pointerPositionAndMotionData.Value.Item1);
            default:
                return null;
        }
    }

    private TriggeredActivation GetTriggeredActivation(InputState state, InputActivation activation, InputActionMap actionMap)
        => new TriggeredActivation(ActiveScheme,
                actionMap,
                new InputActivationContext(userId, activation, GetPointerInformation(actionMap),
                GetActivityInformation(state)));

    private void RemoveState(InputState inputState, VirtualInput virtualInput)
    {
        if (inputState is DeviceInputState deviceInputState)
        {

        }
    }

    #endregion

    #region Logging

    [LoggerMessage(eventId: 1, LogLevel.Warning, "Input Activation was of an unknown type and could not be processed: {activationTypeName}")]
    private static partial void LogUnknownActivationWarning(ILogger logger, string activationTypeName);

    [LoggerMessage(eventId: 2, LogLevel.Warning, "An attempt was made to process a virtual input named '{virtualInputName}', but it was unrecognized type '{virtualInputType}' and could not be processed.")]
    private static partial void LogUnknownVirtualInputWarning(ILogger logger, string virtualInputName, string virtualInputType);

    #endregion
}
