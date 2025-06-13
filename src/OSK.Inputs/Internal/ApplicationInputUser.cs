using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OSK.Inputs.Internal.Services;
using OSK.Inputs.Models.Configuration;
using OSK.Inputs.Models.Runtime;

namespace OSK.Inputs.Internal;
internal class ApplicationInputUser(int userId, InputSystemConfiguration inputSystemConfiguration) : IApplicationInputUser, IDisposable
{
    #region Variables

    internal readonly Dictionary<string, RuntimeInputController> _inputControllers = [];

    private readonly Dictionary<int, RuntimeInputDevice> _userInputDeviceLookup = [];
    private RuntimeInputController? _activeInputController;

    #endregion
     
    #region IApplicationInputUser

    public int Id => userId;

    public InputDefinition ActiveInputDefinition { get; private set; } = inputSystemConfiguration.InputDefinitions.First();

    public IEnumerable<InputDeviceIdentifier> DeviceIdentifiers => _userInputDeviceLookup.Values.Select(device => device.DeviceIdentifier);

    public InputScheme? GetActiveInputScheme(string controllerId)
    {
        if (_inputControllers.TryGetValue(controllerId, out var inputController))
        {
            return inputController.InputScheme;
        }

        return null;
    }

    #endregion

    #region Public

    public event Action<int, RuntimeInputDevice> OnInputDeviceReconnected = (_, _) => { };
    public event Action<int, RuntimeInputDevice> OnInputDeviceDisconnected = (_, _) => { };
    public event Action<int, InputControllerConfiguration> OnActiveInputControllerChanged = (_, _) => { };

    public void Dispose()
    {
        foreach (var controller in _inputControllers.Values)
        {
            RemoveInputController(controller);
        }


        _inputControllers.Clear();
    }

    public bool TryGetDevice(int deviceId, out RuntimeInputDevice device)
    {
        return _userInputDeviceLookup.TryGetValue(deviceId, out device);
    }

    public void AddInputDevices(params RuntimeInputDevice[] inputDevices)
    {
        foreach (var inputDevice in inputDevices)
        {
            if (!_userInputDeviceLookup.TryGetValue(inputDevice.DeviceIdentifier.DeviceId, out _))
            {
                _userInputDeviceLookup.Add(inputDevice.DeviceIdentifier.DeviceId, inputDevice);
                inputDevice.InputReader.OnDeviceDisconnected += NotifyDeviceDisconnected;
                inputDevice.InputReader.OnDeviceConnected += NotifyDeviceReconnected;
            }
        }

        if (ActiveInputDefinition is not null)
        {
            SetActiveInputDefinition(ActiveInputDefinition, _inputControllers.Values.Select(controller => controller.InputScheme).ToArray());
        }
    }

    public void SetActiveInputDefinition(InputDefinition inputDefinition, IEnumerable<InputScheme> activeInputSchemes)
    {
        ActiveInputDefinition = inputDefinition;

        _inputControllers.Clear();
        _activeInputController = null;
        foreach (var scheme in activeInputSchemes)
        {
            var controllerConfiguration = inputSystemConfiguration.InputControllers.FirstOrDefaultByString(
                controller => controller.ControllerName, scheme.ControllerId);
            if (controllerConfiguration is null)
            {
                continue;
            }

            var schemeInputDevices = scheme.DeviceActionMaps.Select(deviceMap =>
            {
                var runtimeInputDevice = _userInputDeviceLookup.Values.FirstOrDefault(device => device.DeviceIdentifier.DeviceName == deviceMap.DeviceName);
                if (runtimeInputDevice is null)
                {
                    return null;
                }

                var inputLookup = runtimeInputDevice.Configuration.Inputs.ToDictionary(input => input.Id);
                var activeInputs = deviceMap.InputActionMaps.Select(actionMap => inputLookup[actionMap.InputId]);
                runtimeInputDevice.SetActiveInputs(deviceMap, activeInputs);
                return runtimeInputDevice;
            })
                .Where(device => device is not null);
            
            // if any of the devices is available for this input scheme, we shouldn't prevent a user from providing input
            // i.e. keyboard + mouse without a mouse shouldn't stop keyboard input
            if (schemeInputDevices is null || !schemeInputDevices.Any())
            {
                continue;
            }

#pragma warning disable CS8620 // Argument cannot be used for parameter due to differences in the nullability of reference types.
            var inputController = new RuntimeInputController(controllerConfiguration, scheme, schemeInputDevices.ToArray());
#pragma warning restore CS8620 // Argument cannot be used for parameter due to differences in the nullability of reference types.
            _inputControllers[scheme.ControllerId] = inputController;
        }
    }

    public async Task<IEnumerable<UserActionCommand>> ReadInputsAsync(int maxConcurrentDevices, CancellationToken cancellationToken = default)
    {
        if (_activeInputController is not null)
        {
            var actions = await ReadInputControllerAsync(_activeInputController, ActiveInputDefinition, maxConcurrentDevices, cancellationToken);
            if (actions.Any())
            {
                return actions;
            }
        }
        if (cancellationToken.IsCancellationRequested)
        {
            return [];
        }

        foreach (var inputController in _inputControllers.Values)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }

            var actions = await ReadInputControllerAsync(inputController, ActiveInputDefinition, maxConcurrentDevices, cancellationToken);
            if (actions.Any())
            { 
                _activeInputController = inputController;
                OnActiveInputControllerChanged(userId, inputController.Configuration);
                return actions;
            }
        }

        return [];
    }

    #endregion

    #region Helpers

    internal void RemoveInputController(RuntimeInputController inputController)
    {
        if (!_inputControllers.TryGetValue(inputController.ControllerId, out var _))
        {
            return;
        }

        foreach (var device in inputController.InputDevices)
        {
            device.Dispose();
            _userInputDeviceLookup.Remove(device.DeviceIdentifier.DeviceId);
        }
        _inputControllers.Remove(inputController.ControllerId);
    }

    private void NotifyDeviceDisconnected(InputDeviceIdentifier deviceIdentifier)
    {
        var changedInputController = _inputControllers.Values.Where(inputController
            => inputController.InputDevices.Any(device => device.DeviceIdentifier == deviceIdentifier))
            .Select(controller => new
            {
                Controller = controller,
                InputDevice = controller.InputDevices.First(device => device.DeviceIdentifier == deviceIdentifier)
            })
            .FirstOrDefault();
            
        if (changedInputController is not null)
        {
            if (_activeInputController is not null && changedInputController.Controller == _activeInputController)
            {
                _activeInputController = null;
            }
            OnInputDeviceDisconnected(userId, changedInputController.InputDevice);
        }
    }

    private void NotifyDeviceReconnected(InputDeviceIdentifier deviceIdentifier)
    {
        var inputDevice = _inputControllers.Values.SelectMany(controller => controller.InputDevices)
            .FirstOrDefault(device => device.DeviceIdentifier == deviceIdentifier);
        if (inputDevice is not null)
        {
            OnInputDeviceReconnected(userId, inputDevice);
        }
    }

    private async Task<IEnumerable<UserActionCommand>> ReadInputControllerAsync(RuntimeInputController controller, InputDefinition definition,
        int maxConcurrentDevices, CancellationToken cancellationToken)
    {
        var actionCommandsByDevice = await controller.InputDevices.Where(device => device.ActionMap is not null).ExecuteConcurrentResultAsync(
            async (device) =>
            {
                var activatedInputs = await device.ReadInputsAsync(cancellationToken);

                var actionMapLookup = device.ActionMap!.InputActionMaps.ToDictionary(actionMap => actionMap.InputId);
                return activatedInputs.Where(activatedInput 
                    => actionMapLookup.TryGetValue(activatedInput.Input.Id, out var inputActionMap) 
                     && inputActionMap.TriggerPhases.Contains(activatedInput.TriggeredPhase))
                    .Select(activatedInput
                        => new UserActionCommand(userId, activatedInput, definition[actionMapLookup[activatedInput.Input.Id].ActionKey]));
            }, maxConcurrentDevices, cancellationToken);
        
        var actionCommands = new Dictionary<string, UserActionCommand>();
        foreach (var actionCommand in actionCommandsByDevice.SelectMany(actionCommands => actionCommands))
        {
            if (!actionCommands.TryGetValue(actionCommand.InputAction.ActionKey, out _))
            {
                actionCommands.Add(actionCommand.InputAction.ActionKey, actionCommand);
            }
        }

        return actionCommands.Values;
    }

    #endregion
}