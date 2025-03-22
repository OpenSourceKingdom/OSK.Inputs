using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using OSK.Inputs.Internal;
using OSK.Inputs.Models.Inputs;
using OSK.Inputs.Options;

namespace OSK.Inputs.Models.Configuration;
public abstract class InputControllerConfigurator(InputControllerName controllerName, Type inputReaderType)
{
    #region Variables

    private readonly Dictionary<string, InputConfiguration> _inputConfigurations = [];

    #endregion

    #region Helpers

    protected void AddInputConfiguration<TOptions>(IInput input, Action<TOptions>? optionConfigurator)
        where TOptions : InputOptions, new()
    {
        if (_inputConfigurations.TryGetValue(input.Name, out _))
        {
            throw new DuplicateNameException($"Unable to add input {input.Name} to controller configuration because it was already added.");
        }

        TOptions options = new();
        if (optionConfigurator is not null)
        {
            optionConfigurator(options);
        }

        _inputConfigurations[input.Name] = new InputConfiguration<TOptions>(input, options);
    }

    internal IInputControllerConfiguration BuildControllerConfiguration()
    {
        return new DefaultInputControllerConfiguration(controllerName, inputReaderType, _inputConfigurations.Values, null);
    }

    #endregion
}
