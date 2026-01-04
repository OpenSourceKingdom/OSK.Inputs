using System;
using System.Collections.Generic;
using System.Text;

namespace OSK.Inputs.Abstractions.Inputs;

public interface IInput
{
    public int Id { get; }

    string DeviceType { get; }

    InputType InputType { get; }
}
