using System;
using Microsoft.Extensions.DependencyInjection;
using OSK.Inputs.Internal.Services;
using OSK.Inputs.Models.Configuration;
using OSK.Inputs.Ports;

namespace OSK.Inputs;
public static class InputSystemBuilderExtensions
{
    #region Public

    public static IInputSystemBuilder AddXboxController<TInputReader>(this IInputSystemBuilder builder, Action<XboxControllerConfigurator> configurator)
        where TInputReader : IInputReader
    {
        return builder.AddInputController<XboxControllerConfigurator, TInputReader>(configurator);
    }

    public static IInputSystemBuilder AddPlayStationController<TInputReader>(this IInputSystemBuilder builder, Action<PlayStationControllerConfigurator> configurator)
        where TInputReader : IInputReader
    {
        return builder.AddInputController<PlayStationControllerConfigurator, TInputReader>(configurator);
    }

    public static IInputSystemBuilder AddKeyboardAndMouseController<TInputReader>(this IInputSystemBuilder builder, Action<KeyboardAndMouseConfigurator> configurator)
        where TInputReader : IInputReader
    {
        return builder.AddInputController<KeyboardAndMouseConfigurator, TInputReader>(configurator);
    }

    public static IInputSystemBuilder AddSensorController<TInputReader>(this IInputSystemBuilder builder, Action<SensorControllerConfigurator> configurator)
        where TInputReader : IInputReader
    {
        return builder.AddInputController<SensorControllerConfigurator, TInputReader>(configurator);
    }

    #endregion

    #region Helpers

    private static IInputSystemBuilder AddInputController<TConfigurator, TInputReader>(this IInputSystemBuilder builder, Action<TConfigurator> configuratorAction)
        where TInputReader : IInputReader
        where TConfigurator : InputControllerConfigurator
    {
        if (configuratorAction is null)
        {
            throw new ArgumentNullException(nameof(configuratorAction));
        }

        var configurator = (TConfigurator)Activator.CreateInstance(typeof(TConfigurator), typeof(TInputReader));
        configuratorAction(configurator);

        builder.AddInputController(configurator.BuildControllerConfiguration());

        return builder;
    }

    #endregion
}
