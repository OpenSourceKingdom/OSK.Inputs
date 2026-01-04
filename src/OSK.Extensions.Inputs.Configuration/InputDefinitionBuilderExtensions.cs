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
    /// <summary>
    /// Creates a list of actions given a service of type <typeparamref name="TService"/>.
    /// 
    /// The extension searches for methods based on their return type being void and taking
    /// a single parameter for <see cref="InputEventContext"/>. Names must be unique, so
    /// overloads are not guaranteed to work. If changing the name of the method or enabling extra
    /// features, like pointer details, is needed, you will need to use the <see cref="InputActionAttribute"/>
    /// to define these options.
    /// </summary>
    /// <typeparam name="TService">
    /// The service object that will be used to get the methods for. This service needs to be registered on the DI chain that the input system uses.
    /// </typeparam>
    /// <param name="builder">The builder to configure</param>
    /// <returns>The builder for chaining</returns>
    public static IInputDefinitionBuilder WithActions<TService>(this IInputDefinitionBuilder builder)
        where TService : class
    {
        var methodsToRegister = typeof(TService).GetMethods().Where(method =>
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
            var invoker = InvokerFactory.CreateInvoker<TService>(method);

            builder.WithAction(inputActionName,
                inputEventContext => invoker.FastInvoke(inputEventContext.Services.GetRequiredService<TService>(), [inputEventContext]),
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
