using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OSK.Inputs.Models.Configuration;
using OSK.Inputs.Models.Runtime;

namespace OSK.Inputs.Internal;
internal class ApplicationInputUser(int userId, InputSystemConfiguration inputSystemConfiguration) : IApplicationInputUser, IDisposable
{
    #region Variables

    public event Action<int, InputDevice> OnInputDeviceConnected = (_, _) => { };
    public event Action<int, InputDevice> OnInputDeviceDisconnected = (_, _) => { };
    public event Action<int, InputDevice> OnActiveInputDeviceChanged = (_, _) => { };

    private readonly Dictionary<int, InputDevice> _inputDevices = [];

    private readonly Dictionary<string, InputScheme> _activeInputSchemeLookup = inputSystemConfiguration.InputDefinitions.First()
        .InputSchemes.GroupBy(scheme => scheme.DeviceName).ToDictionary(schemeGroup => schemeGroup.Key.Name,
        schemeGroup => schemeGroup.FirstOrDefault(scheme => scheme.IsDefault) ?? schemeGroup.First());
    private readonly Dictionary<string, UserInputReadContext> _inputReadContextLookup = [];

    private InputDevice? _activeInputDevice;

    #endregion

    #region IApplicationInputUser

    public int Id => userId;

    public InputDefinition ActiveInputDefinition { get; private set; } = inputSystemConfiguration.InputDefinitions.First();

    public IEnumerable<InputDeviceIdentifier> DeviceIdentifiers => _inputDevices.Values.Select(device => device.DeviceIdentifier);

    public InputScheme? GetActiveInputScheme(InputDeviceName deviceName)
    {
        if (_activeInputSchemeLookup.TryGetValue(deviceName.Name, out InputScheme inputScheme))
        {
            return inputScheme;
        }

        return null;
    }

    #endregion

    #region IDisposable

    public void Dispose()
    {
        foreach (var device in _inputDevices.Values)
        {
            RemoveInputDevice(device.DeviceIdentifier);
        }

        _inputDevices.Clear();
    }

    #endregion
    
    #region Publc

    public bool TryGetDevice(int deviceId, out InputDevice device)
        => _inputDevices.TryGetValue(deviceId, out device);

    public void RemoveInputDevice(InputDeviceIdentifier deviceIdentifier)
    {
        if (!_inputDevices.TryGetValue(deviceIdentifier.DeviceId, out var device))
        {
            return;
        }

        device.Dispose();
        _inputDevices.Remove(deviceIdentifier.DeviceId);
    }

    public void AddInputDevices(params InputDevice[] inputDevices)
    {
        foreach (var inputDevice in inputDevices)
        {
            if (!_inputDevices.TryGetValue(inputDevice.DeviceIdentifier.DeviceId, out _))
            {
                _inputDevices[inputDevice.DeviceIdentifier.DeviceId] = inputDevice;

                inputDevice.InputReader.OnControllerDisconnected += NotifyDeviceDisconnected;
                inputDevice.InputReader.OnControllerReconnected += NotifyDeviceReconnected;
            }
        }
    }

    public void SetActiveInputDefinition(InputDefinition inputDefinition, IEnumerable<InputScheme> activeInputSchemes)
    {
        ActiveInputDefinition = inputDefinition;

        _activeInputSchemeLookup.Clear();
        foreach (var activeScheme in activeInputSchemes)
        {
            var inputScheme = inputDefinition.InputSchemes.FirstOrDefaultByString(scheme => scheme.SchemeName,
                activeScheme.SchemeName);

            if (inputScheme != null)
            {
                _activeInputSchemeLookup[inputScheme.DeviceName.Name] = inputScheme;
            }
        }

        _inputReadContextLookup.Clear();
        foreach (var inputSchemeActionCommand in GetReadContexts(inputSystemConfiguration, inputDefinition,
            userId, activeInputSchemes))
        {
            _inputReadContextLookup[inputSchemeActionCommand.Key.SchemeName] = inputSchemeActionCommand.Value;
        }
    }

    public async Task<IEnumerable<UserActionCommand>> ReadInputsAsync(CancellationToken cancellationToken = default)
    {
        foreach (var context in _inputReadContextLookup.Values)
        {
            context.PrepareForNextRead();
        }

        if (_activeInputDevice is not null)
        {
            var readContext = _inputReadContextLookup[GetInputSchemeLookupKey(_activeInputDevice.DeviceIdentifier)];
            await _activeInputDevice.InputReader.ReadInputsAsync(readContext, cancellationToken);
            if (readContext.ReceivedInput)
            {
                return GetActionCommands(readContext);
            }
        }

        if (cancellationToken.IsCancellationRequested)
        {
            return [];
        }

        foreach (var device in _inputDevices.Values)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }
            if (_inputReadContextLookup.TryGetValue(GetInputSchemeLookupKey(device.DeviceIdentifier), out var readContext))
            {
                await device.InputReader.ReadInputsAsync(readContext, cancellationToken);
                if (readContext.ReceivedInput)
                {
                    _activeInputDevice = device;
                    OnActiveInputDeviceChanged(userId, device);
                    return GetActionCommands(readContext);
                }
            }
        }

        return [];
    }

    #endregion

    #region Helpers

    private static IEnumerable<KeyValuePair<InputScheme, UserInputReadContext>> GetReadContexts(InputSystemConfiguration configuration, 
        InputDefinition inputDefinition, int userId, IEnumerable<InputScheme> activeSchemes)
    {
        var inputControllerInputLookup = configuration.SupportedInputDevices.ToDictionary(device => device.DeviceName,
            device => device.Inputs.ToDictionary(input => input.Id));
        var actionConfigurationLookup = inputDefinition.InputActions.ToDictionary(action => action.ActionKey);
        var activeInputSchemeLookup = activeSchemes.ToDictionary(scheme => scheme.DeviceName);

        return inputDefinition.InputSchemes.Select(inputScheme =>
        {
            var deviceInputLookup = inputControllerInputLookup[inputScheme.DeviceName];
            var activeScheme = activeInputSchemeLookup.TryGetValue(inputScheme.DeviceName, out var activeDeviceScheme)
                ? activeDeviceScheme
                : inputScheme;

            return new KeyValuePair<InputScheme, UserInputReadContext>(activeScheme, new UserInputReadContext(userId, 
                activeScheme.InputActionMaps.Select(actionMap => new InputActionMapPair(deviceInputLookup[actionMap.InputId], actionMap))));
        });
    }

    private void NotifyDeviceDisconnected(InputDeviceIdentifier deviceIdentifier)
    {
        if (_inputDevices.TryGetValue(deviceIdentifier.DeviceId, out var device))
        {
            if (_activeInputDevice is not null && _activeInputDevice == device)
            {
                _activeInputDevice = null;
            }
            OnInputDeviceDisconnected(userId, device);
        }
    }

    private void NotifyDeviceReconnected(InputDeviceIdentifier deviceIdentifier)
    {
        if (_inputDevices.TryGetValue(deviceIdentifier.DeviceId, out var device))
        {
            OnInputDeviceConnected(userId, device);
        }
    }

    private IEnumerable<UserActionCommand> GetActionCommands(UserInputReadContext context)
    {
        var actionCommands = new List<UserActionCommand>();
        foreach (var activatedInput in context.GetActivatedInputs())
        {
            actionCommands.Add(new UserActionCommand(userId, activatedInput, ActiveInputDefinition[activatedInput.ActionKey]));
        }

        return actionCommands;
    }

    private string GetInputSchemeLookupKey(InputDeviceIdentifier identifier) => $"{identifier.DeviceName}";

    #endregion
}