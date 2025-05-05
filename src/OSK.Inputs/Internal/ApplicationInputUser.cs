using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using OSK.Inputs.Internal.Services;
using OSK.Inputs.Models.Configuration;
using OSK.Inputs.Models.Inputs;
using OSK.Inputs.Models.Runtime;

namespace OSK.Inputs.Internal;
internal class ApplicationInputUser(int userId, InputSystemConfiguration inputSystemConfiguration) : IApplicationInputUser, IDisposable
{
    #region Variables

    private readonly Dictionary<int, RuntimeInputDevice> _userInputDeviceLookup = [];
    internal readonly Dictionary<string, RuntimeInputController> _inputControllers = [];
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
            SetActiveInputDefinition(ActiveInputDefinition, _inputControllers.Values.Select(controller => controller.InputScheme));
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
                controller => scheme.ControllerId, scheme.ControllerId);
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
                var actionMapPairs = deviceMap.InputActionMaps.SelectMany(actionMap => 
                {
                    var input = inputLookup[actionMap.InputId];
                    return input switch
                    {
                        CombinationInput combinationInput => combinationInput.Inputs.Select(comboInput => new InputActionMapPair(new MaskedInput(comboInput, combinationInput), actionMap)),
                        _ => [ new InputActionMapPair(input, actionMap) ]
                    };
                });
                runtimeInputDevice.SetInputScheme(actionMapPairs);
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
            var activatedInputs = await _activeInputController.InputDevices.ExecuteConcurrentResultAsync(
                device => device.ReadInputsAsync(cancellationToken), maxConcurrentDevices, cancellationToken);
            var allInputs = activatedInputs.SelectMany(inputs => inputs);
            if (allInputs.Any())
            {
                return GetActionCommands(ActiveInputDefinition, allInputs);
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

            var activatedInputs = await inputController.InputDevices.ExecuteConcurrentResultAsync(
                device => device.ReadInputsAsync(cancellationToken), 1, cancellationToken);
            var allInputs = activatedInputs.SelectMany(inputs => inputs);
            if (allInputs.Any())
            { 
                _activeInputController = inputController;
                OnActiveInputControllerChanged(userId, inputController.Configuration);
                return GetActionCommands(ActiveInputDefinition, allInputs);
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

    private IEnumerable<UserActionCommand> GetActionCommands(InputDefinition inputDefinition, IEnumerable<ActivatedInput> activatedInputs)
    {
        var actionCommandLookup = new Dictionary<int, UserActionCommand>();
        var virtualInputLookup = new Dictionary<int, List<ActivatedInput>>();

        foreach (var activatedInput in activatedInputs) 
        {
            if (activatedInput.Input is MaskedInput maskedInput && maskedInput.ParentInput is not null)
            {
                if (!virtualInputLookup.TryGetValue(maskedInput.ParentInput.Id, out var combinationInputs))
                {
                    combinationInputs = [];
                    virtualInputLookup[maskedInput.ParentInput.Id] = combinationInputs;
                }

                combinationInputs.Add(activatedInput);
            }
            else
            {
                actionCommandLookup[activatedInput.Input.Id] = new UserActionCommand(userId, activatedInput, inputDefinition[activatedInput.ActionKey]);
            }
        }

        foreach (var virtualInputActivations in virtualInputLookup.Values)
        {
            var maskedInput = (MaskedInput)virtualInputActivations.First().Input;
            var virtualInputActionMapPair = virtualInputActivations.First().InputActionMapPair;
            InputPhase? virtualActivationPhase = null;

            switch (maskedInput.ParentInput)
            {
                case CombinationInput combinationInput:
                    foreach (var activatedInput in virtualInputActivations)
                    {
                        if (virtualInputActionMapPair.TriggerPhases.Contains(activatedInput.TriggeredPhase))
                        {
                            if (virtualActivationPhase is null || (int)activatedInput.TriggeredPhase < (int)virtualActivationPhase)
                            {
                                virtualActivationPhase = activatedInput.TriggeredPhase;
                            }
                            else
                            {
                                virtualActivationPhase = null;
                                break;
                            }
                        }
                    }

                    break;
            }

            if (virtualActivationPhase is not null)
            {
                foreach (var input in virtualInputActivations)
                {
                    actionCommandLookup.Remove(input.Input.Id);
                }

                var firstActivatedInput = virtualInputActivations.First();
                actionCommandLookup[maskedInput.ParentInput!.Id] = new UserActionCommand(userId,
                    new ActivatedInput(userId, firstActivatedInput.DeviceName, firstActivatedInput.InputActionMapPair,
                       virtualActivationPhase.Value, firstActivatedInput.InputPower, firstActivatedInput.PointerInformation),
                    inputDefinition[virtualInputActionMapPair.ActionKey]);
            }
        }

        return actionCommandLookup.Values;
    }

    #endregion
}