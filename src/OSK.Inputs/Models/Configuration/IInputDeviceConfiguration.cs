using System;
using System.Collections.Generic;
using OSK.Inputs.Models.Inputs;

namespace OSK.Inputs.Models.Configuration;
public interface IInputDeviceConfiguration
{
    #region Variables

    public InputDeviceName DeviceName { get; }

    public Type InputReaderType { get; }

    public IReadOnlyCollection<IInput> Inputs { get; }

    bool IsValidInput(IInput input);

    #endregion
}
