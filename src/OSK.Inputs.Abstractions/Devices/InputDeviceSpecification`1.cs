using System;

namespace OSK.Inputs.Abstractions.Devices;

public abstract class InputDeviceSpecification<TInput>: InputDeviceSpecification
    where TInput : Enum
{
}
