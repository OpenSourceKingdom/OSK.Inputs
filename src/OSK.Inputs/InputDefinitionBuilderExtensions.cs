using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using OSK.Expressions.Invoker;
using OSK.Inputs.Attributes;
using OSK.Inputs.Models.Configuration;
using OSK.Inputs.Models.Events;
using OSK.Inputs.Ports;

namespace OSK.Inputs;

public static class InputDefinitionBuilderExtensions
{
    #region Registrations

    public static IInputDefinitionBuilder RegisterActions<T>(this IInputDefinitionBuilder builder)
        where T : notnull
    {
        var methodsToRegister = typeof(T).GetMethods().Where(method =>
        {
            var methodParameters = method.GetParameters();
            return methodParameters.Length is 1 && methodParameters[0].ParameterType == typeof(InputActivationEvent)
             && method.ReturnParameter.ParameterType == typeof(ValueTask);
        });

        foreach (var method in methodsToRegister)
        {
            var inputActionAttribute = method.GetCustomAttribute<InputActionAttribute>();
            var inputActionName = string.IsNullOrWhiteSpace(inputActionAttribute?.ActionName) 
                ? method.Name 
                : inputActionAttribute.ActionName;
            var invoker = InvokerFactory.CreateInvoker<T>(method);

            builder.AddAction<T>(inputActionName, inputActionAttribute?.Description, 
                (service, activationEvent) => invoker.FastInvoke<ValueTask>(service, [activationEvent]));
        }

        return builder;
    }

    #endregion

    #region Actions

    public static IInputDefinitionBuilder AddAction<TService>(this IInputDefinitionBuilder builder, string actionName,
    Func<TService, InputActivationEvent, ValueTask> actionExecutor)
    where TService : notnull
        => builder.AddAction(actionName, null, @event => actionExecutor(@event.Services.GetRequiredService<TService>(), @event));

    public static IInputDefinitionBuilder AddAction<TService>(this IInputDefinitionBuilder builder, string actionName,
        string? description, Func<TService, InputActivationEvent, ValueTask> actionExecutor)
        where TService : notnull
        => builder.AddAction(actionName, description, @event => actionExecutor(@event.Services.GetRequiredService<TService>(), @event));

    public static IInputDefinitionBuilder AddAction(this IInputDefinitionBuilder builder, string actionName,
        Func<InputActivationEvent, ValueTask> actionExecutor)
        => builder.AddAction(actionName, null, actionExecutor);

    public static IInputDefinitionBuilder AddAction(this IInputDefinitionBuilder builder, string actionName, string? description,
        Func<InputActivationEvent, ValueTask> actionExecutor)
    {
        if (actionExecutor is null)
        {
            throw new ArgumentNullException(nameof(actionExecutor));
        }

        builder.AddAction(new InputAction(actionName, actionExecutor, description));
        return builder;
    }

    #endregion
}
