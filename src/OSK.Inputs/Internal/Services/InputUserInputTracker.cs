using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using OSK.Functions.Outputs.Abstractions;
using OSK.Functions.Outputs.Logging.Abstractions;
using OSK.Inputs.Abstractions;
using OSK.Inputs.Abstractions.Configuration;
using OSK.Inputs.Abstractions.Devices;
using OSK.Inputs.Abstractions.Inputs;
using OSK.Inputs.Abstractions.Runtime;
using OSK.Inputs.Internal.Models;

namespace OSK.Inputs.Internal.Services;

internal partial class InputUserInputTracker(int userId, InputSchemeActionMap schemeMap, 
    InputSystemConfiguration configuration, ILogger<InputUserInputTracker> logger,
    IOutputFactory<InputUserInputTracker> outputFactory, IServiceProvider serviceProvider): IInputUserTracker
{
    #region Variables

    /// <summary>
    /// The deadzone minimum prevents calculating smoothness for intensities if the tolerance is effectively 0.
    /// </summary>
    private const float DeadzoneMinimumThreshold = 0.01f;
    private const int MaxPointerRecords = 3;
    private float _pointerSquareThreshold = MathF.Pow(configuration.ProcessorConfiguration.PointerMovementThreshold.GetValueOrDefault(.01f), 2);

    private float _deadZoneTolerance = configuration.ProcessorConfiguration.DeadzoneTolerance.HasValue
        ? configuration.ProcessorConfiguration.DeadzoneTolerance < DeadzoneMinimumThreshold 
            ? DeadzoneMinimumThreshold 
            : configuration.ProcessorConfiguration.DeadzoneTolerance.Value
        : DeadzoneMinimumThreshold;

    private readonly Dictionary<InputDeviceFamily, DeviceInputTracker> _deviceInputTrackerLookup
        = schemeMap.DeviceSchemeMaps.ToDictionary(
            deviceScheme => deviceScheme.DeviceFamily, 
            deviceScheme => new DeviceInputTracker(deviceScheme));

    private readonly Dictionary<InputDeviceFamily, InputDeviceSpecification> _deviceInputLookup = schemeMap.DeviceSchemeMaps
            .Select(device => configuration.GetDeviceSpecification(device.DeviceFamily))
            .Where(specification => specification is not null)
            .ToDictionary(specification => specification!.DeviceFamily, specification => specification!);

    #endregion

    #region IUserInputTracker

    public ActiveInputScheme ActiveScheme { get; } = new ActiveInputScheme(schemeMap.DefinitionName, schemeMap.SchemeName, 
                                                                            [.. schemeMap.DeviceSchemeMaps.Select(m => m.DeviceFamily)]);

    public int UserId => userId;

    public IEnumerable<ProcessedInputEvent> Update(TimeSpan deltaTime)
    {
        var removalDelay = configuration.ProcessorConfiguration.TapReactivationTime.GetValueOrDefault(defaultValue: TimeSpan.Zero);

        var triggeredActions = new List<ProcessedInputEvent>();
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

                inputState.Duration += deltaTime;

                var reprocess = false;
                switch (inputState.Phase)
                {
                    case InputPhase.Start:
                        if (inputState.Duration >= configuration.ProcessorConfiguration.ActiveTimeThreshold.GetValueOrDefault(TimeSpan.Zero))
                        {
                            inputState.Phase = InputPhase.Active;
                            reprocess = true;
                        }
                        break;
                    case InputPhase.Active:
                        break;
                }

                ProcessedInputEvent? processedInputEvent = null;
                var inputEvent = GetEventForState(inputState);

                if (inputEvent is not null)
                {
                    if (reprocess)
                    {
                        var reprocessedOutput = Track(deltaTime, inputEvent);
                        processedInputEvent = reprocessedOutput.IsSuccessful
                            ? reprocessedOutput.Value
                            : null;
                    }
                    else if (inputState.MappedAction is not null)
                    {
                        processedInputEvent = GetTriggeredProcessedEvent(deltaTime, inputState, inputEvent, inputState.MappedAction);
                    }
                }

                if (processedInputEvent is not null)
                {
                    triggeredActions.Add(processedInputEvent.Value);
                }
            }

            foreach (var state in inputsToRemove)
            {
                deviceTracker.RemoveState(state);
            }
        }

        return triggeredActions;
    }

    public IOutput<ProcessedInputEvent> Track(TimeSpan deltaTime, InputEvent inputEvent)
    {
        if (inputEvent is not DeviceInputEvent deviceInputEvent)
        {
            return outputFactory.Fail<ProcessedInputEvent>("The input event was not a physical input event");
        }
        if (!_deviceInputTrackerLookup.TryGetValue(deviceInputEvent.DeviceIdentifier.DeviceFamily, out var deviceTracker))
        {
            return outputFactory.NotFound<ProcessedInputEvent>("No device tracker was found for the device triggering the input.");
        }

        var actionMaps = deviceTracker.SchemeMap.GetActionMaps(deviceInputEvent.InputId);
        if (!actionMaps.Any())
        {
            return outputFactory.Fail<ProcessedInputEvent>("No action map found for the input");
        }

        var inputState = GetAndUpdateInputState(deviceTracker, deviceInputEvent);
        if (inputState is null)
        {
            return outputFactory.Fail<ProcessedInputEvent>("Unable to acquire input state");
        }

        var virtualActionMaps = actionMaps.Where(map => map.Input is VirtualInput);
        var inputActionMap = actionMaps.FirstOrDefault(map => map.Input is IDeviceInput);

        // Null action maps refer to passive inputs (i.e. pointers) that are merely meant to provide passive data collection rather than
        // active input execution
        if (inputActionMap.Action is null)
        {
            return outputFactory.Succeed(ProcessedInputEvent.NotTriggered);
        }

        var virtualInputActivationContext = ProcessVirtualInputEvent(deltaTime, deviceTracker, inputState, virtualActionMaps);
        var processInputEvent = virtualInputActivationContext is null && inputActionMap is not null 
                && inputActionMap.Action.TriggerPhases.Contains(inputState.Phase)
            ? GetTriggeredProcessedEvent(deltaTime, inputState, deviceInputEvent, inputActionMap)
            : virtualInputActivationContext;

        inputState.MappedAction = processInputEvent?.ActionMap;
        if (inputState.Phase is InputPhase.End && configuration.ProcessorConfiguration.TapReactivationTime is null)
        {
            deviceTracker.RemoveState(inputState);
        }

        return outputFactory.Succeed(processInputEvent ?? ProcessedInputEvent.NotTriggered);
    }

    #endregion

    #region Helpers

    private DeviceInputState? GetAndUpdateInputState(DeviceInputTracker deviceTracker, DeviceInputEvent inputEvent)
    {
        IInput? input = null;
        if (!_deviceInputLookup.TryGetValue(inputEvent.DeviceIdentifier.DeviceFamily, out var specification)
             || !specification.TryGetInput(inputEvent.InputId, out input))
        {
            return null;
        }

        DeviceInputState inputState;
        switch (inputEvent)
        {
            case InputPointerEvent pointerEvent:
                var pointerState = deviceTracker.GetOrCreatePointerState(pointerEvent.PointerId, () =>
                {
                    return new InputPointerState(pointerEvent.PointerId, (IDeviceInput)input!, MaxPointerRecords, _pointerSquareThreshold)
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
                var inputPowerState = deviceTracker.GetOrCreatePowerState(powerEvent.InputId, () =>
                {
                    return new InputPowerState((IDeviceInput)input!)
                    {
                        DeviceIdentifier = powerEvent.DeviceIdentifier,
                        Duration = TimeSpan.Zero,
                        InputPowers = []
                    };
                });

                inputPowerState.InputPowers = input switch  
                {
                    AnalogInput _ => ApplyDeadzoneSmoothScaling([.. powerEvent.InputIntensities], _deadZoneTolerance),
                    _ => [.. powerEvent.InputIntensities]
                };
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

    private ProcessedInputEvent? ProcessVirtualInputEvent(TimeSpan deltaTime, DeviceInputTracker deviceTracker, DeviceInputState inputState,
        IEnumerable<InputActionMap> virtualInputActionMaps)
    {
        foreach (var virtualInputActionMap in virtualInputActionMaps)
        {
            switch (virtualInputActionMap.Input)
            {
                case DeviceCombinationInput combinationInput:
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
                        return GetTriggeredProcessedEvent(deltaTime, inputState, new VirtualInputEvent(combinationInput, combinationPhase), 
                            virtualInputActionMap);
                    }

                    break;
                default:
                    LogUnknownVirtualInputWarning(logger, deviceTracker.SchemeMap.DeviceFamily, virtualInputActionMap.Input.Id, virtualInputActionMap.Input.GetType().FullName);
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
        if (actionMap.Action is null || !actionMap.Action.IncludePointerDetails)
        {
            return PointerDetails.Empty;
        }

        var pointerData = _deviceInputTrackerLookup.Values.SelectMany(deviceState
            => deviceState.PointerStates.Select(pointerState 
                =>
            {
                var pointerPositionMotionData = pointerState.GetCurrentPositionAndMotionData();
                if (pointerPositionMotionData is null)
                {
                    return null;
                }

                return (PointerData?) new PointerData(pointerState.PointerId, deviceState.SchemeMap.DeviceFamily,
                    pointerPositionMotionData.Value.Item1, pointerPositionMotionData.Value.Item2);
            }))
            .Where(pointerData => pointerData is not null)
            .Select(p => p!.Value)
            .ToArray();

        return new PointerDetails(pointerData);
    }
    
    private InputEvent? GetEventForState(InputState state)
    {
        switch (state)
        {
            case InputPowerState powerState:
                return new InputPowerEvent(powerState.DeviceIdentifier, powerState.Input.Id, powerState.Phase, powerState.InputPowers);
            case InputPointerState pointerState:
                var pointerPositionAndMotionData = pointerState.GetCurrentPositionAndMotionData();
                return pointerPositionAndMotionData is null
                    ? null
                    : new InputPointerEvent(pointerState.DeviceIdentifier, pointerState.Input.Id, pointerState.Phase, pointerState.PointerId,
                            pointerPositionAndMotionData.Value.Item1);
            default:
                return null;
        }
    }

    private ProcessedInputEvent GetTriggeredProcessedEvent(TimeSpan deltaTime, InputState state, InputEvent activation, InputActionMap actionMap)
        => new(actionMap, new InputEventContext(userId, deltaTime, activation, GetPointerInformation(actionMap), GetActivityInformation(state), serviceProvider));

    private InputIntensity[] ApplyDeadzoneSmoothScaling(InputIntensity[] inputIntensities, float deadzone)
    {
        if (deadzone <= DeadzoneMinimumThreshold)
        {
            return [.. inputIntensities.Select(intensity => InputIntensity.Zero(intensity.Axis))];
        }

        var rawMagnitude = inputIntensities.CalculateMagnitude();
        if (rawMagnitude <= deadzone)
        {
            return [.. inputIntensities.Select(intensity => InputIntensity.Zero(intensity.Axis))];
        }

        // Calculate the scaled magnitude (0.0 to 1.0)
        // This ensures that at the deadzone edge, the value is 0, not the deadzone value itself.
        var scaledMagnitude = (rawMagnitude - deadzone) / (1f - deadzone);

        // Clamp to 1.0 to handle slight hardware variances
        scaledMagnitude = MathF.Min(scaledMagnitude, 1f);

        // Calculate the scaling ratio
        // We multiply the original component by (scaledMagnitude / rawMagnitude)
        // This effectively "shortens" the vector while keeping its direction.
        var factor = MathF.Abs(scaledMagnitude / rawMagnitude);

        return [.. inputIntensities.Select(intensity => new InputIntensity(intensity.Axis, intensity.Power * factor))];
    }

    #endregion

    #region Logging

    [LoggerMessage(eventId: 1, LogLevel.Warning, "Input Activation was of an unknown type and could not be processed: {activationTypeName}")]
    private static partial void LogUnknownActivationWarning(ILogger logger, string activationTypeName);

    [LoggerMessage(eventId: 2, LogLevel.Warning, "An attempt was made to process a virtual input with id '{virtualInputId}' on device '{deviceFamily}', but it was unrecognized type '{virtualInputType}' and could not be processed.")]
    private static partial void LogUnknownVirtualInputWarning(ILogger logger, InputDeviceFamily deviceFamily, int virtualInputId, string virtualInputType);

    #endregion
}
