using System.Collections.Generic;
using OSK.Inputs.Abstractions.Inputs;

namespace OSK.Inputs.Abstractions.Configuration;

public abstract class InputDeviceSpecification
{
    public abstract InputDeviceIdentity DeviceIdentity { get; }   

    public abstract IReadOnlyCollection<Input> Inputs { get; }
}
