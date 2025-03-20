using System;
using OSK.Inputs.Models.Inputs;
using OSK.Inputs.Ports;

namespace OSK.Inputs;
public static class InputControllerBuilderExtensions
{
    public static IInputControllerBuilder UseInputValidator<TInput>(this IInputControllerBuilder builder)
        where TInput : IInput
    {
        builder.UseValidation(input => input is TInput);
        return builder;
    }

    public static IInputControllerBuilder AddHardwareInput<TValue, TOptions>(this IInputControllerBuilder builder, string name, TValue inputValue,
        Action<TOptions> configureOptions)
        where TOptions : class, new()
    {
        TOptions options = new();
        configureOptions(options);

        builder.AddInput(new HardwareInput<TValue, TOptions>(name, inputValue, options));
        return builder;
    }
}
