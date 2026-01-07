using System;
using OSK.Extensions.Inputs.Configuration.Ports;
using OSK.Inputs.Abstractions.Devices;

namespace OSK.Extensions.Inputs.Configuration;

public static class InputDeviceMapBuilderExtensions
{
    public static IInputDeviceMapBuilder WithInput<TDeviceSpecification, TInput>(
        this IInputDeviceMapBuilder builder, TInput input, string actionKey)
        where TInput : Enum
        where TDeviceSpecification : InputDeviceSpecification<TInput>
    {
        return builder.WithInputMap(Convert.ToInt32(input), actionKey);
    }
}
