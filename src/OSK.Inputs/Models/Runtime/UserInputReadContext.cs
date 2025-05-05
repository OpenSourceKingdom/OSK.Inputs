using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using OSK.Inputs.Models.Configuration;
using OSK.Inputs.Models.Inputs;

namespace OSK.Inputs.Models.Runtime;
public class UserInputReadContext(int userId, InputDeviceName deviceName)
{
    #region Variables

    private const float DegreeThresholdForVector = 5;

    private readonly Dictionary<int, InputActionMapPair> _actionMapPairLookup = [];
    private readonly Dictionary<int, ActivatedInput> _previousActivatedInputs = [];
    private readonly Dictionary<int, ActivatedInput> _currentActivatedInputs = [];

    #endregion

    #region Api

    public void SetInputState(IInput input, InputPhase currentPhase)
        => SetInputState(_actionMapPairLookup[input.Id], currentPhase, PointerInformation.Default, InputPower.None);
        
    public void SetInputState(IInput input, InputPhase currentPhase,
        Vector2 pointerLocation)
        => SetInputState(_actionMapPairLookup[input.Id], currentPhase, 
            new PointerInformation(PointerInformation.DefaultPointerId, [pointerLocation]), 
            currentPhase is InputPhase.Idle
                ? InputPower.None
                : InputPower.FullPower(1));

    public void SetInputState(IInput input, InputPhase currentPhase,
        Vector2 pointerLocation, params float[] inputAxisPowers)
        => SetInputState(_actionMapPairLookup[input.Id], currentPhase,
            new PointerInformation(PointerInformation.DefaultPointerId, [pointerLocation]), 
            InputPower.FromPowerLevels(inputAxisPowers));

    public void SetInputState(IInput input, InputPhase currentPhase,
        PointerInformation pointerInformation, params float[] inputAxisPowers)
        => SetInputState(_actionMapPairLookup[input.Id], currentPhase, pointerInformation,
            InputPower.FromPowerLevels(inputAxisPowers));

    private void SetInputState(InputActionMapPair inputActionMapPair, InputPhase currentPhase,
        PointerInformation pointerInformation, InputPower inputPower)
    {
        if (_currentActivatedInputs.TryGetValue(inputActionMapPair.InputId, out _))
        {
            return;
        }
        if (_previousActivatedInputs.TryGetValue(inputActionMapPair.InputId, out var previousActivatedInput))
        {
            pointerInformation = MergePointerInformation(pointerInformation, currentPhase, previousActivatedInput,
                DegreeThresholdForVector);
        }

        _currentActivatedInputs.Add(inputActionMapPair.InputId,
            new ActivatedInput(userId, deviceName, inputActionMapPair, currentPhase, inputPower, pointerInformation));
    }

    #endregion

    #region Helpers

    internal IEnumerable<ActivatedInput> ProcessInputs()
    {
        var activeInputs = _currentActivatedInputs.Values;
        PrepareForNextRead();

        return activeInputs;
    }

    internal IEnumerable<InputActionMapPair> InputActionMapPairs
    {
        get => _actionMapPairLookup.Values;
        set
        {
            _actionMapPairLookup.Clear();
            foreach (var inputMapPair in value)
            {
                _actionMapPairLookup[inputMapPair.InputId] = inputMapPair;
            }
        }
    }

    private void PrepareForNextRead()
    {
        _previousActivatedInputs.Clear();
        foreach (var activatedInput in _currentActivatedInputs.Values
            .Where(input => _actionMapPairLookup.TryGetValue(input.Input.Id, out _)))
        {
            var actionMapPair = _actionMapPairLookup[activatedInput.Input.Id];
            if (!actionMapPair.TriggerPhases.Contains(activatedInput.TriggeredPhase))
            {
                _previousActivatedInputs[activatedInput.Input.Id] = activatedInput;
            }
        }

        _currentActivatedInputs.Clear();
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
