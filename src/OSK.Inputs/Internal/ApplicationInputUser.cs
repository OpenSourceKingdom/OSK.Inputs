using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OSK.Inputs.Models.Configuration;
using OSK.Inputs.Models.Runtime;

namespace OSK.Inputs.Internal;
internal class ApplicationInputUser(int userId, InputDefinition inputDefinition, 
    IEnumerable<InputScheme> inputSchemes) : IApplicationInputUser, IDisposable
{
    #region Variables

    public event Action<int, InputController> OnInputControllerConnected = (_, _) => { };
    public event Action<int, InputController> OnInputControllerDisconnected = (_, _) => { };
    public event Action<int, InputController> OnActiveInputControllerChanged = (_, _) => { };

    private readonly Dictionary<int, InputController> _inputControllers = [];
    private readonly Dictionary<string, InputScheme> _activeInputSchemeLookup = inputSchemes.ToDictionary(scheme => scheme.ControllerName.Name);
    private readonly Dictionary<string, IEnumerable<InputActionSchemeMap>> _actionSchemeMaps = 
        inputSchemes.ToDictionary(scheme => scheme.SchemeName, 
            scheme => (IEnumerable<InputActionSchemeMap>) inputDefinition.GetInputActionSchemeMaps(scheme.ControllerName, scheme.SchemeName));

    private InputController? _activeInputController;

    #endregion

    #region IApplicationInputUser

    public int Id => userId;

    public InputDefinition ActiveInputDefinition { get; private set; } = inputDefinition;

    public IEnumerable<InputControllerIdentifier> ControllerIdentifiers => _inputControllers.Values.Select(controller => controller.ControllerIdentifier);

    public InputScheme? GetActiveInputScheme(InputControllerName controllerName)
    {
        if (_activeInputSchemeLookup.TryGetValue(controllerName.Name, out InputScheme inputScheme))
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
            RemoveInputController(controller.ControllerIdentifier);
        }

        _inputControllers.Clear();
    }

    #endregion
    
    #region Publc

    public bool TryGetController(int controllerId, out InputController controller)
        => _inputControllers.TryGetValue(controllerId, out controller);

    public void RemoveInputController(InputControllerIdentifier controllerIdentifier)
    {
        if (!_inputControllers.TryGetValue(controllerIdentifier.ControllerId, out var controller))
        {
            return;
        }

        controller.Dispose();
        _inputControllers.Remove(controllerIdentifier.ControllerId);
    }

    public void AddInputControllers(params InputController[] inputControllers)
    {
        foreach (var inputController in inputControllers)
        {
            if (!_inputControllers.TryGetValue(inputController.ControllerIdentifier.ControllerId, out _))
            {
                _inputControllers[inputController.ControllerIdentifier.ControllerId] = inputController;

                inputController.InputReader.OnControllerDisconnected += NotifyControllerDisconnected;
                inputController.InputReader.OnControllerReconnected += NotifyControllerReconnected;
            }
        }
    }

    public void SetActiveInputSchemes(InputDefinition inputDefinition, IEnumerable<InputScheme> activeInputSchemes)
    {
        ActiveInputDefinition = inputDefinition;

        _activeInputSchemeLookup.Clear();
        _actionSchemeMaps.Clear();
        foreach (var scheme in activeInputSchemes)
        {
            _activeInputSchemeLookup[GetInputSchemeLookupKey(scheme)] = scheme;
            _actionSchemeMaps[scheme.SchemeName] = inputDefinition.GetInputActionSchemeMaps(scheme.ControllerName, scheme.SchemeName);
        }
    }

    public async Task<IEnumerable<UserActivatedInput>> ReadInputsAsync(CancellationToken cancellationToken = default)
    {
        if (_activeInputController is not null)
        {
            var schemeMaps = _actionSchemeMaps[GetInputSchemeLookupKey(_activeInputController.ControllerIdentifier)];
            var activatedInputs = await _activeInputController.InputReader.ReadInputsAsync(new InputReadContext(schemeMaps), cancellationToken);
            if (activatedInputs.Any())
            {
                return activatedInputs.Select(input => new UserActivatedInput(userId, input));
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
            if (_actionSchemeMaps.TryGetValue(GetInputSchemeLookupKey(controller.ControllerIdentifier), out var schemeMaps))
            {
                var activatedInputs = await controller.InputReader.ReadInputsAsync(new InputReadContext(schemeMaps), cancellationToken);
                if (activatedInputs.Any())
                {
                    _activeInputController = controller;
                    OnActiveInputControllerChanged(userId, controller);
                    return activatedInputs.Select(input => new UserActivatedInput(userId, input));
                }
            }
        }

        return [];
    }

    #endregion

    #region Helpers

    private void NotifyControllerDisconnected(InputControllerIdentifier controllerIdentifier)
    {
        if (_inputControllers.TryGetValue(controllerIdentifier.ControllerId, out var controller))
        {
            if (_activeInputController is not null && _activeInputController == controller)
            {
                _activeInputController = null;
            }
            OnInputControllerDisconnected(userId, controller);
        }
    }

    private void NotifyControllerReconnected(InputControllerIdentifier controllerIdentifier)
    {
        if (_inputControllers.TryGetValue(controllerIdentifier.ControllerId, out var controller))
        {
            OnInputControllerConnected(userId, controller);
        }
    }

    private string GetInputSchemeLookupKey(InputControllerIdentifier identifier) => $"{identifier.ControllerName}";
    private string GetInputSchemeLookupKey(InputScheme inputScheme) => $"{inputScheme.ControllerName}";

    #endregion
}