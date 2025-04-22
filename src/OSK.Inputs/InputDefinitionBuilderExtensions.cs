using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using OSK.Inputs.Models.Configuration;
using OSK.Inputs.Models.Events;
using OSK.Inputs.Ports;

namespace OSK.Inputs;

public static class InputDefinitionBuilderExtensions
{
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
