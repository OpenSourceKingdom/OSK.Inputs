using System;
using System.Collections.Generic;
using OSK.Inputs.Models.Inputs;

namespace OSK.Inputs.Models.Configuration;
public interface IInputControllerConfiguration
{
    #region Variables

    public InputControllerName ControllerName { get; }

    public Type InputReaderType { get; }

    public IReadOnlyCollection<InputConfiguration> InputConfigurations { get; }

    bool IsValidInput(IInput input);

    #endregion
}
