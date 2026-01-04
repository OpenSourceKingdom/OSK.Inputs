using System;
using System.Collections.Generic;
using System.Linq;
using OSK.Inputs.Abstractions.Configuration;

namespace OSK.Inputs.Internal.Models;

internal class DeviceInputTracker(DeviceSchemeActionMap schemeMap)
{
    #region Variables

    private readonly Dictionary<int, InputPowerState> _powerStates = [];
    private readonly Dictionary<int, InputPointerState> _pointerStates = [];

    #endregion

    #region Api

    public DeviceSchemeActionMap SchemeMap => schemeMap;

    public IEnumerable<InputState> AllStates => ((IEnumerable<InputState>)_pointerStates.Values).Concat(_powerStates.Values);

    public IEnumerable<InputPointerState> PointerStates => _pointerStates.Values;

    public InputPowerState? GetInputPowerState(int inputId)
        => _powerStates.TryGetValue(inputId, out var state)
            ? state
            : null;

    public InputPowerState GetOrCreatePowerState(int inputId, Func<InputPowerState> factory)
    {
        if (!_powerStates.TryGetValue(inputId, out var state))
        {
            state = factory();
            _powerStates[inputId] = state;
        }

        return state;
    }

    public InputPointerState GetOrCreatePointerState(int pointerId, Func<InputPointerState> factory)
    {
        if (!_pointerStates.TryGetValue(pointerId, out var state))
        {
            state = factory();
            _pointerStates[pointerId] = state;
        }

        return state;
    }

    public void RemoveState(InputState state)
    {
        if (state is InputPowerState)
        {
            _powerStates.Remove(state.InputId);
            return;
        }

        _pointerStates.Remove(state.InputId);
    }

    #endregion
}
