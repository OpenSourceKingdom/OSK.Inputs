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

    public event Action<int, InputDevice> OnInputControllerConnected = (_, _) => { };
    public event Action<int, InputDevice> OnInputControllerDisconnected = (_, _) => { };
    public event Action<int, InputDevice> OnActiveInputControllerChanged = (_, _) => { };

    private readonly Dictionary<int, InputDevice> _inputControllers = [];

    private readonly Dictionary<string, InputScheme> _activeInputSchemeLookup = inputSystemConfiguration.InputDefinitions.First()
        .InputSchemes.GroupBy(scheme => scheme.ControllerName).ToDictionary(schemeGroup => schemeGroup.Key.Name,
        schemeGroup => schemeGroup.FirstOrDefault(scheme => scheme.IsDefault) ?? schemeGroup.First());
    private readonly Dictionary<string, UserInputReadContext> _inputReadContextLookup = [];

    private InputDevice? _activeInputController;

    #endregion

    #region IApplicationInputUser

    public int Id => userId;

    public InputDefinition ActiveInputDefinition { get; private set; } = inputSystemConfiguration.InputDefinitions.First();

    public IEnumerable<InputDeviceIdentifier> ControllerIdentifiers => _inputControllers.Values.Select(controller => controller.DeviceIdentifier);

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
        foreach (var controller in _inputControllers.Values)
        {
            RemoveInputController(controller.DeviceIdentifier);
        }

        _inputControllers.Clear();
    }

    #endregion
    
    #region Publc

    public bool TryGetController(int controllerId, out InputDevice controller)
        => _inputControllers.TryGetValue(controllerId, out controller);

    public void RemoveInputController(InputDeviceIdentifier controllerIdentifier)
    {
        if (!_inputControllers.TryGetValue(controllerIdentifier.DeviceId, out var controller))
        {
            return;
        }

        controller.Dispose();
        _inputControllers.Remove(controllerIdentifier.DeviceId);
    }

    public void AddInputControllers(params InputDevice[] inputControllers)
    {
        foreach (var inputController in inputControllers)
        {
            if (!_inputControllers.TryGetValue(inputController.DeviceIdentifier.DeviceId, out _))
            {
                _inputControllers[inputController.DeviceIdentifier.DeviceId] = inputController;

                inputController.InputReader.OnControllerDisconnected += NotifyControllerDisconnected;
                inputController.InputReader.OnControllerReconnected += NotifyControllerReconnected;
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
                _activeInputSchemeLookup[inputScheme.ControllerName.Name] = inputScheme;
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

        if (_activeInputController is not null)
        {
            var readContext = _inputReadContextLookup[GetInputSchemeLookupKey(_activeInputController.DeviceIdentifier)];
            await _activeInputController.InputReader.ReadInputsAsync(readContext, cancellationToken);
            if (readContext.ReceivedInput)
            {
                return GetActionCommands(readContext);
            }
        }

        if (cancellationToken.IsCancellationRequested)
        {
            return [];
        }

        foreach (var controller in _inputControllers.Values)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }
            if (_inputReadContextLookup.TryGetValue(GetInputSchemeLookupKey(controller.DeviceIdentifier), out var readContext))
            {
                await controller.InputReader.ReadInputsAsync(readContext, cancellationToken);
                if (readContext.ReceivedInput)
                {
                    _activeInputController = controller;
                    OnActiveInputControllerChanged(userId, controller);
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
        var inputControllerInputLookup = configuration.SupportedInputControllers.ToDictionary(controller => controller.ControllerName,
            controller => controller.Inputs.ToDictionary(input => input.Id));
        var actionConfigurationLookup = inputDefinition.InputActions.ToDictionary(action => action.ActionKey);
        var activeInputSchemeLookup = activeSchemes.ToDictionary(scheme => scheme.ControllerName);

        return inputDefinition.InputSchemes.Select(inputScheme =>
        {
            var controllerInputLookup = inputControllerInputLookup[inputScheme.ControllerName];
            var activeScheme = activeInputSchemeLookup.TryGetValue(inputScheme.ControllerName, out var activeControllerScheme)
                ? activeControllerScheme
                : inputScheme;

            return new KeyValuePair<InputScheme, UserInputReadContext>(activeScheme, new UserInputReadContext(userId, 
                activeScheme.InputActionMaps.Select(actionMap => new InputActionMapPair(controllerInputLookup[actionMap.InputId], actionMap))));
        });
    }

    private void NotifyControllerDisconnected(InputDeviceIdentifier controllerIdentifier)
    {
        if (_inputControllers.TryGetValue(controllerIdentifier.DeviceId, out var controller))
        {
            if (_activeInputController is not null && _activeInputController == controller)
            {
                _activeInputController = null;
            }
            OnInputControllerDisconnected(userId, controller);
        }
    }

    private void NotifyControllerReconnected(InputDeviceIdentifier controllerIdentifier)
    {
        if (_inputControllers.TryGetValue(controllerIdentifier.DeviceId, out var controller))
        {
            OnInputControllerConnected(userId, controller);
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
    private string GetInputSchemeLookupKey(InputScheme inputScheme) => $"{inputScheme.ControllerName}";

    #endregion
}