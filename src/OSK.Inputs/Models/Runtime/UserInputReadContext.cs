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
    private static readonly InputPhase[] AllFlags = Enum.GetValues(typeof(InputPhase))
        .Cast<InputPhase>()
        .OrderByDescending(phase => (int)phase)
        .ToArray();

    private readonly Dictionary<int, InputActionMapPair> _actionMapPairLookup = [];
    private readonly Dictionary<int, ActivatedInput> _previousActivatedInputs = [];
    private readonly Dictionary<int, ActivatedInput> _currentActivatedInputs = [];

    #endregion

    #region Api

    public IEnumerable<InputActionMapPair> InputActionPairs 
    { 
        get => _actionMapPairLookup.Values; 
        internal set
        {
            _actionMapPairLookup.Clear();
            foreach (var pair in value)
            {
                _actionMapPairLookup[pair.InputId] = pair;
            }
        } 
    }

    public void ActivateInput(InputActionMapPair inputActionMapPair, InputPhase triggeredPhase,
        Vector2 pointerLocation)
        => ActivatePointerInput(inputActionMapPair, triggeredPhase, 
            new PointerInformation(PointerInformation.DefaultPointerId, [pointerLocation]), 
            InputPower.FullPower(1));

    public void ActivateInput(InputActionMapPair inputActionMapPair, InputPhase triggeredPhase,
        Vector2 pointerLocation, params float[] inputAxisPowers)
        => ActivatePointerInput(inputActionMapPair, triggeredPhase,
            new PointerInformation(PointerInformation.DefaultPointerId, [pointerLocation]), 
            InputPower.FromPowerLevels(inputAxisPowers));

    public void ActivatePointerInput(InputActionMapPair inputActionMapPair, InputPhase triggeredPhase,
        PointerInformation pointerInformation, params float[] inputAxisPowers)
        => ActivatePointerInput(inputActionMapPair, triggeredPhase, pointerInformation,
            InputPower.FromPowerLevels(inputAxisPowers));

    public void ActivatePointerInput(InputActionMapPair inputActionMapPair, InputPhase triggeredPhase,
        PointerInformation pointerInformation, InputPower inputPower)
    {
        if (_currentActivatedInputs.TryGetValue(inputActionMapPair.InputId, out _))
        {
            return;
        }
        if (_previousActivatedInputs.TryGetValue(inputActionMapPair.InputId, out var previousActivatedInput))
        {
            pointerInformation = MergePointerInformation(pointerInformation, triggeredPhase, previousActivatedInput,
                DegreeThresholdForVector);
        }

        _currentActivatedInputs.Add(inputActionMapPair.InputId,
            new ActivatedInput(userId, deviceName, inputActionMapPair, triggeredPhase, inputPower, pointerInformation));
    }

    #endregion

    #region Helpers

    internal IEnumerable<ActivatedInput> GetActivatedInputs() => _currentActivatedInputs.Values;

    internal bool ReceivedInput => _currentActivatedInputs.Any();

    internal void PrepareForNextRead()
    {
        _previousActivatedInputs.Clear();
        foreach (var activatedInput in _currentActivatedInputs.Values
            .Where(input => _actionMapPairLookup.TryGetValue(input.Input.Id, out _)))
        {
            var actionTriggerPhase = _actionMapPairLookup[activatedInput.Input.Id];
            var lastTriggerFlag = AllFlags.Where(flag => actionTriggerPhase.TriggerPhase.HasFlag(flag))
                .OrderByDescending(flag => (int)flag)
                .First();

            if (!activatedInput.TriggeredPhase.HasFlag(lastTriggerFlag))
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
