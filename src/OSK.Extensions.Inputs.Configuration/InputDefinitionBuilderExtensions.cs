using System;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using OSK.Expressions.Invoker;
using OSK.Extensions.Inputs.Configuration.Attributes;
using OSK.Extensions.Inputs.Configuration.Options;
using OSK.Extensions.Inputs.Configuration.Ports;
using OSK.Inputs.Abstractions.Inputs;
using OSK.Inputs.Abstractions.Runtime;

namespace OSK.Extensions.Inputs.Configuration;

public static class InputDefinitionBuilderExtensions
{
    /// Creates a list of actions given a service of type <typeparamref name="TService"/>.
    /// <inheritdoc cref="WithActions(IInputDefinitionBuilder, Type)"/>
    public static IInputDefinitionBuilder WithActions<TService>(this IInputDefinitionBuilder builder)
        where TService : class
        => builder.WithActions(typeof(TService));

    /// <summary>
    /// The extension searches for methods based on their return type being void and taking
    /// a single parameter for <see cref="InputEventContext"/>. Names must be unique, so
    /// overloads are not guaranteed to work. If changing the name of the method or enabling extra
    /// features, like pointer details, is needed, you will need to use the <see cref="InputActionAttribute"/>
    /// to define these options.
    /// <param name="builder">The builder to configure</param>
    /// </summary>
    /// <param name="serviceType">
    /// The service object that will be used to get the methods for. This service needs to be registered on the DI chain that the input system uses.
    /// </param>
    /// <returns>The builder for chaining</returns>
    public static IInputDefinitionBuilder WithActions(this IInputDefinitionBuilder builder, Type serviceType)
    {
        if (serviceType is null)
        {
            throw new ArgumentNullException(nameof(serviceType));
        }

        var methodsToRegister = serviceType.GetMethods().Where(method =>
        {
            var methodParameters = method.GetParameters();
            return methodParameters.Length is 1 && methodParameters[0].ParameterType == typeof(InputEventContext)
             && method.ReturnParameter.ParameterType == typeof(void);
        });

        foreach (var method in methodsToRegister)
        {
            var inputActionAttribute = method.GetCustomAttribute<InputActionAttribute>();
            var inputActionName = string.IsNullOrWhiteSpace(inputActionAttribute?.ActionName)
                ? method.Name
                : inputActionAttribute.ActionName;
            var invoker = InvokerFactory.CreateInvoker(serviceType, method);

            builder.WithAction(inputActionName,
                inputEventContext => invoker.FastInvoke(inputEventContext.Services.GetRequiredService(serviceType), [inputEventContext]),
                inputActionAttribute?.TriggerPhases ?? [InputPhase.Start],
                new InputActionOptions()
                {
                    Description = inputActionAttribute?.Description,
                    IncludePointerInformation = inputActionAttribute?.IncludePointerDetails ?? false
                });
        }

        return builder;
    }
}
