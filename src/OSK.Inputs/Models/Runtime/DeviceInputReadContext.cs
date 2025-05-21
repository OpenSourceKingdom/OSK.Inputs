using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using OSK.Inputs.Models.Configuration;
using OSK.Inputs.Models.Inputs;

namespace OSK.Inputs.Models.Runtime;
public class DeviceInputReadContext(InputDeviceName deviceName)
{
    #region Variables

    const float ANGLE_THRESHOLD_FOR_NEW_VECTOR = 0.5f;

    private readonly Dictionary<int, IInput> _inputLookup = [];

    private readonly Dictionary<int, ActivatedInput> _previousActivations = [];
    private readonly Dictionary<int, ActivatedInput> _currentActivations = [];

    #endregion

    #region Api

    public void SetInputState(IInput input, InputPhase currentPhase)
        => SetInputState(input, currentPhase, PointerInformation.Default, InputPower.None);
        
    public void SetInputState(IInput input, InputPhase currentPhase,
        Vector2 pointerLocation)
        => SetInputState(input, currentPhase, 
            new PointerInformation(PointerInformation.DefaultPointerId, [pointerLocation]), 
            currentPhase is InputPhase.Idle
                ? InputPower.None
                : InputPower.FullPower(1));

    public void SetInputState(IInput input, InputPhase currentPhase,
        Vector2 pointerLocation, params float[] inputAxisPowers)
        => SetInputState(input, currentPhase,
            new PointerInformation(PointerInformation.DefaultPointerId, [pointerLocation]), 
            InputPower.FromPowerLevels(inputAxisPowers));

    public void SetInputState(IInput input, InputPhase currentPhase,
        PointerInformation pointerInformation, params float[] inputAxisPowers)
        => SetInputState(input, currentPhase, pointerInformation,
            InputPower.FromPowerLevels(inputAxisPowers));

    #endregion

    #region Helpers

    private void SetInputState(IInput input, InputPhase currentPhase, PointerInformation pointerInformation, InputPower inputPower)
    {
        if (!_inputLookup.TryGetValue(input.Id, out _))
        {
            return;
        }
        if (_currentActivations.TryGetValue(input.Id, out _))
        {
            return;
        }
        if (_previousActivations.TryGetValue(input.Id, out var previousInput))
        {
            pointerInformation = MergePointerInformation(pointerInformation, currentPhase, previousInput,
                ANGLE_THRESHOLD_FOR_NEW_VECTOR);
        }

        _currentActivations.Add(input.Id,
            new ActivatedInput(deviceName, input, currentPhase, inputPower, pointerInformation));
    }

    internal IEnumerable<ActivatedInput> GetInputActivations()
    {
        return _currentActivations.Values;
    }

    internal void Reset()
    {
        _previousActivations.Clear();
        foreach (var kvp in _currentActivations)
        {
            if (kvp.Value.TriggeredPhase is InputPhase.Idle)
            {
                continue;
            }

            _previousActivations[kvp.Key] = kvp.Value;
        }

        _currentActivations.Clear();
    }

    internal IEnumerable<IInput> ActiveInputs
    {
        get => _inputLookup.Values;
        set
        {
            _inputLookup.Clear();
            foreach (var input in value)
            {
                _inputLookup[input.Id] = input;
            }
        }
    }

    private PointerInformation MergePointerInformation(PointerInformation pointerInformation, InputPhase triggeredPhase,
        ActivatedInput previousInput, float angleThresholdForNewVector)
    {
        // Pointer hasn't moved, use the previous input data as it is more complete
        if (pointerInformation.CurrentPosition == previousInput.PointerInformation.CurrentPosition)
        {
            return previousInput.PointerInformation;
        }

        // The input phase transition that occurred would not require multiple pointer information states (i.e. start to end)
        // because the pointer would 
        var shouldMergeInformation = triggeredPhase.HasFlag(InputPhase.Active)
            || (previousInput.TriggeredPhase.HasFlag(InputPhase.Active) && triggeredPhase.HasFlag(InputPhase.End));
        if (!shouldMergeInformation)
        {
            return pointerInformation;
        }

        var previousMoveVector = previousInput.PointerInformation.CurrentPosition - previousInput.PointerInformation.PreviousPosition;
        var newMoveVector = pointerInformation.CurrentPosition - previousInput.PointerInformation.CurrentPosition;

        // Validate that the new mouse position is actually a different vector movement    
        if (previousMoveVector.GetAngleBetween(newMoveVector) >= angleThresholdForNewVector)
        {
            return new PointerInformation(pointerInformation.PointerId,
                previousInput.PointerInformation.PointerPositions.Append(pointerInformation.CurrentPosition).ToArray());
        }

        return previousInput.PointerInformation;
    }

    #endregion
}
