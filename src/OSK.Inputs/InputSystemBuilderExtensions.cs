using System;
using OSK.Inputs.Internal.Services;
using OSK.Inputs.Models.Configuration;
using OSK.Inputs.Ports;

namespace OSK.Inputs;
public static class InputSystemBuilderExtensions
{
    public static IInputSystemBuilder AddInputController(this IInputSystemBuilder builder, string controllerName,
        Action<IInputControllerBuilder> controllerConfiguration)
        => builder.AddInputController(new InputControllerName(controllerName), controllerConfiguration);

    private static IInputSystemBuilder AddInputController(this IInputSystemBuilder builder, InputControllerName controllerName, 
        Action<IInputControllerBuilder> controllerConfiguration)
    {
        if (controllerConfiguration is null)
        {
            throw new ArgumentNullException(nameof(controllerConfiguration));
        }

        var controllerBuilder = new InputControllerBuilder(controllerName);
        controllerConfiguration(controllerBuilder);

        builder.AddInputController(controllerBuilder.BuildInputController());

        return builder;
    }
}
