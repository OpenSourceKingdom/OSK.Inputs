using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using OSK.Inputs.Models.Configuration;
using OSK.Inputs.Models.Events;
using OSK.Inputs.Models.Runtime;
using OSK.Inputs.Options;
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

    #region Input Schemes

    public static IInputDefinitionBuilder AddInputScheme(this IInputDefinitionBuilder builder, InputDeviceName deviceName,
        Action<IInputSchemeBuilder> schemeConfigurator)
    {
        return builder.AddInputScheme(deviceName, InputScheme.DefaultSchemeName, schemeConfigurator);
    }

    public static IInputDefinitionBuilder AddKeyboardScheme(this IInputDefinitionBuilder builder, 
        Action<IInputSchemeBuilder> schemeConfigurator)
    {
        return builder.AddInputScheme(Keyboard.KeyboardName, InputScheme.DefaultSchemeName, schemeConfigurator);
    }
    public static IInputDefinitionBuilder AddKeyboardScheme(this IInputDefinitionBuilder builder, string schemeName,
        Action<IInputSchemeBuilder> schemeConfigurator)
    {
        return builder.AddInputScheme(Keyboard.KeyboardName, schemeName, schemeConfigurator);
    }

    public static IInputDefinitionBuilder AddMouseScheme(this IInputDefinitionBuilder builder, Action<IInputSchemeBuilder> schemeConfigurator)
    {
        return builder.AddInputScheme(Mouse.MouseName, InputScheme.DefaultSchemeName, schemeConfigurator);
    }

    public static IInputDefinitionBuilder AddMouseScheme(this IInputDefinitionBuilder builder, string schemeName,
        Action<IInputSchemeBuilder> schemeConfigurator)
    {
        return builder.AddInputScheme(Mouse.MouseName, schemeName, schemeConfigurator);
    }

    public static IInputDefinitionBuilder AddXboxScheme(this IInputDefinitionBuilder builder, 
        Action<IInputSchemeBuilder> schemeConfigurator)
    {
        return builder.AddInputScheme(XboxController.XboxControllerName, InputScheme.DefaultSchemeName, schemeConfigurator);
    }

    public static IInputDefinitionBuilder AddXboxScheme(this IInputDefinitionBuilder builder, string schemeName,
        Action<IInputSchemeBuilder> schemeConfigurator)
    {
        return builder.AddInputScheme(XboxController.XboxControllerName, schemeName, schemeConfigurator);
    }

    public static IInputDefinitionBuilder AddPlayStationScheme(this IInputDefinitionBuilder builder, Action<IInputSchemeBuilder> schemeConfigurator)
    {
        return builder.AddInputScheme(PlayStationController.PlayStationControllerName, InputScheme.DefaultSchemeName, schemeConfigurator);
    }

    public static IInputDefinitionBuilder AddPlayStationScheme(this IInputDefinitionBuilder builder, string schemeName,
        Action<IInputSchemeBuilder> schemeConfigurator)
    {
        return builder.AddInputScheme(PlayStationController.PlayStationControllerName, schemeName, schemeConfigurator);
    }

    public static IInputDefinitionBuilder AddSensorScheme(this IInputDefinitionBuilder builder, Action<IInputSchemeBuilder> schemeConfigurator)
    {
        return builder.AddInputScheme(SensorController.SensorControllerName, InputScheme.DefaultSchemeName, schemeConfigurator);
    }
    public static IInputDefinitionBuilder AddSensorScheme(this IInputDefinitionBuilder builder, string schemeName,
        Action<IInputSchemeBuilder> schemeConfigurator)
    {
        return builder.AddInputScheme(SensorController.SensorControllerName, schemeName, schemeConfigurator);
    }

    #endregion
}
