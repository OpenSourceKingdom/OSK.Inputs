using System;
using OSK.Inputs.Models.Configuration;
using OSK.Inputs.Options;
using OSK.Inputs.Ports;

namespace OSK.Inputs;

public static class InputDefinitionBuilderExtensions
{
    public static IInputDefinitionBuilder AddAction(this IInputDefinitionBuilder builder, string actionName)
        => builder.AddAction(actionName, null, null);

    public static IInputDefinitionBuilder AddAction(this IInputDefinitionBuilder builder, string actionName, Action<InputActionOptions>? configureOptions)
        => builder.AddAction(actionName, null, configureOptions);

    public static IInputDefinitionBuilder AddAction(this IInputDefinitionBuilder builder, string actionName, string? description)
        => builder.AddAction(actionName, description, null);

    public static IInputDefinitionBuilder AddAction(this IInputDefinitionBuilder builder, string actionName, string? description, Action<InputActionOptions>? configureOptions)
    {
        InputActionOptions? options = null;
        if (configureOptions is not null)
        {
            options = new();
            configureOptions(options);
        }

        builder.AddAction(new InputAction(actionName, description, options));
        return builder;
    }
}
