using System;
using System.Collections.Generic;
using System.Data;
using OSK.Inputs.Models.Configuration;
using OSK.Inputs.Ports;

namespace OSK.Inputs.Internal.Services;

internal class InputDefinitionBuilder(string definitionName, IInputValidationService validationService) : IInputDefinitionBuilder
{
    #region Variables

    private bool _allowCustomInputSchemes;
    private readonly Dictionary<string, InputAction> _actions = [];
    private readonly Dictionary<string, InputControllerConfiguration> _controllers = [];

    #endregion

    #region IInputDefinitionBuilder

    public IInputDefinitionBuilder AddAction(InputAction action)
    {
        if (string.IsNullOrWhiteSpace(action.ActionKey))
        {
            throw new ArgumentNullException(nameof(action));
        }
        if (_actions.TryGetValue(action.ActionKey, out _))
        {
            throw new DuplicateNameException($"An action has already been added to the input definition with the key {action.ActionKey}.");
        }

        _actions.Add(action.ActionKey, action);
        return this;
    }

    public IInputDefinitionBuilder AddInputController(InputControllerConfiguration controller)
    {
        if (controller is null)
        {
            throw new ArgumentNullException(nameof(controller));
        }
        if (string.IsNullOrWhiteSpace(controller.ControllerName))
        {
            throw new ArgumentNullException("Controller name can not be empty.");
        }
        if (_controllers.TryGetValue(controller.ControllerName, out _))
        {
            throw new DuplicateNameException($"A controller with the name {controller.ControllerName} has already been added to the definition.");
        }

        _controllers.Add(controller.ControllerName, controller);
        return this;
    }

    public IInputDefinitionBuilder AllowCustomInputSchemes()
    {
        _allowCustomInputSchemes = true;
        return this;
    }

    public InputDefinition Build()
    {
        var definition = new InputDefinition(definitionName, _allowCustomInputSchemes, _controllers.Values, _actions.Values);

        var validationContext = validationService.ValidateInputDefinition(definition);
        validationContext.EnsureValid();

        return definition;
    }

    #endregion
}
