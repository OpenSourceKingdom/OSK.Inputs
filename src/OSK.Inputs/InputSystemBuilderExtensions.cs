using System;
using OSK.Inputs.Internal.Services;
using OSK.Inputs.Models.Configuration;
using OSK.Inputs.Ports;

namespace OSK.Inputs;
public static class InputSystemBuilderExtensions
{
    public static IInputSystemBuilder AddXboxController<TInputReader>(this IInputSystemBuilder builder, Action<XboxControllerConfigurator> configurator)
        where TInputReader : IInputReader
    {
        if (configurator is null)
        {
            throw new ArgumentNullException(nameof(configurator));
        }

        var xboxConfigurator = new XboxControllerConfigurator(typeof(TInputReader));
        configurator(xboxConfigurator);

        builder.AddInputController(xboxConfigurator.BuildControllerConfiguration());

        return builder;
    }

    public static IInputSystemBuilder AddPlayStationController<TInputReader>(this IInputSystemBuilder builder, Action<PlayStationControllerConfigurator> configurator)
        where TInputReader : IInputReader
    {
        if (configurator is null)
        {
            throw new ArgumentNullException(nameof(configurator));
        }

        var playStationConfigurator = new PlayStationControllerConfigurator(typeof(TInputReader));
        configurator(playStationConfigurator);

        builder.AddInputController(playStationConfigurator.BuildControllerConfiguration());

        return builder;
    }

    public static IInputSystemBuilder AddKeyboardAndMouseController<TInputReader>(this IInputSystemBuilder builder, Action<KeyboardAndMouseConfigurator> configurator)
        where TInputReader : IInputReader
    {
        if (configurator is null)
        {
            throw new ArgumentNullException(nameof(configurator));
        }

        var keyboardAndMouseConfigurator = new KeyboardAndMouseConfigurator(typeof(TInputReader));
        configurator(keyboardAndMouseConfigurator);

        builder.AddInputController(keyboardAndMouseConfigurator.BuildControllerConfiguration());

        return builder;
    }
}
