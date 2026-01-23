using System;
using System.Collections.Generic;
using System.Linq;
using OSK.Inputs.Abstractions.Configuration;
using OSK.Inputs.Abstractions.Inputs;

namespace OSK.Inputs.Internal.Models;

internal class DeviceInputTracker(DeviceSchemeActionMap schemeMap)
{
    #region Variables

    private readonly Dictionary<int, InputPowerState> _powerStates = [];
    private readonly Dictionary<int, InputPointerState> _pointerStates = [];
    private readonly Dictionary<VirtualInput, VirtualInputState> _virtualInputStates = [];

    #endregion

    #region Api

    public DeviceSchemeActionMap SchemeMap => schemeMap;

    public IEnumerable<InputState> AllInputStates => ((IEnumerable<InputState>)_pointerStates.Values).Concat(_powerStates.Values).Concat(_virtualInputStates.Values);

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

    public VirtualInputState? GetVirtualInputState(VirtualInput virtualInput)
        => _virtualInputStates.TryGetValue(virtualInput, out var state)
            ? state
            : null;

    public void SetVirtualInputState(VirtualInputState virtualInputState)
    {
        _virtualInputStates[virtualInputState.Input] = virtualInputState;
        foreach (var linkedInputId in virtualInputState.LinkedInputIds)
        {
            if (_powerStates.TryGetValue(linkedInputId, out var powerState))
            {
                powerState.LinkedVirtualInput = virtualInputState.Input;
            }
            else if (_pointerStates.TryGetValue(linkedInputId, out var pointerState))
            {
                pointerState.LinkedVirtualInput = virtualInputState.Input;
            }
        }
    }

    public void RemoveState(InputState state)
    {
        switch (state)
        {
            case DeviceInputState deviceInputState:
                _powerStates.Remove(deviceInputState.Input.Id);
                _pointerStates.Remove(deviceInputState.Input.Id);
                if (deviceInputState.LinkedVirtualInput is not null)
                {
                    foreach (var linkedInputId in _virtualInputStates[deviceInputState.LinkedVirtualInput].LinkedInputIds)
                    {
                        _powerStates.Remove(linkedInputId);
                        _pointerStates.Remove(linkedInputId);
                    }
                    _virtualInputStates.Remove(deviceInputState.LinkedVirtualInput);
                }
                break;
            case VirtualInputState virtualInputState:
                if (_virtualInputStates.Remove(virtualInputState.Input))
                {
                    foreach (var linkedInputId in virtualInputState.LinkedInputIds)
                    {
                        _powerStates.Remove(linkedInputId);
                        _pointerStates.Remove(linkedInputId);
                    }
                }
                break;
        }
    }

    #endregion
}
