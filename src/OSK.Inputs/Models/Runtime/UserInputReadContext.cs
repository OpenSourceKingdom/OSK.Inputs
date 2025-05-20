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
    private readonly Dictionary<int, ActivatedInput> _inputActivationStates = [];

    #endregion

    #region Api

    public void SetInputState(int inputId, InputPhase currentPhase)
        => SetInputState(inputId, currentPhase, PointerInformation.Default, InputPower.None);
        
    public void SetInputState(int inputId, InputPhase currentPhase,
        Vector2 pointerLocation)
        => SetInputState(inputId, currentPhase, 
            new PointerInformation(PointerInformation.DefaultPointerId, [pointerLocation]), 
            currentPhase is InputPhase.Idle
                ? InputPower.None
                : InputPower.FullPower(1));

    public void SetInputState(int inputId, InputPhase currentPhase,
        Vector2 pointerLocation, params float[] inputAxisPowers)
        => SetInputState(inputId, currentPhase,
            new PointerInformation(PointerInformation.DefaultPointerId, [pointerLocation]), 
            InputPower.FromPowerLevels(inputAxisPowers));

    public void SetInputState(int inputId, InputPhase currentPhase,
        PointerInformation pointerInformation, params float[] inputAxisPowers)
        => SetInputState(inputId, currentPhase, pointerInformation,
            InputPower.FromPowerLevels(inputAxisPowers));

    private void SetInputState(int inputId, InputPhase currentPhase,
        PointerInformation pointerInformation, InputPower inputPower)
    {
        if (!_actionMapPairLookup.TryGetValue(inputId, out var inputActionMapPair))
        {
            return;
        }
        if (_inputActivationStates.TryGetValue(inputActionMapPair.InputId, out _))
        {
            return;
        }

        _inputActivationStates.Add(inputActionMapPair.InputId,
            new ActivatedInput(userId, deviceName, inputActionMapPair, currentPhase, inputPower, pointerInformation));
    }

    #endregion

    #region Helpers

    internal IEnumerable<ActivatedInput> GetInputStates()
    {
        return _inputActivationStates.Values;
    }

    internal void Reset()
    {
        _inputActivationStates.Clear();
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

    #endregion
}
