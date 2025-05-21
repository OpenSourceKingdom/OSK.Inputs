using System;
using OSK.Inputs.Models.Configuration;
using OSK.Inputs.Ports;

namespace OSK.Inputs;
public static class InputSystemBuilderExtensions
{
    #region Public

    public static IInputSystemBuilder AddXboxController<TInputReader>(this IInputSystemBuilder builder, Action<XboxController> configurator)
        where TInputReader : IInputDeviceReader
    {
        return builder.AddInputDevice<XboxController, TInputReader>(configurator);
    }

    public static IInputSystemBuilder AddPlayStationController<TInputReader>(this IInputSystemBuilder builder, Action<PlayStationController> configurator)
        where TInputReader : IInputDeviceReader
    {
        return builder.AddInputDevice<PlayStationController, TInputReader>(configurator);
    }

    public static IInputSystemBuilder AddKeyboard<TInputReader>(this IInputSystemBuilder builder, Action<Keyboard> configurator)
        where TInputReader : IInputDeviceReader
    {
        return builder.AddInputDevice<Keyboard, TInputReader>(configurator);
    }

    public static IInputSystemBuilder AddMouse<TInputReader>(this IInputSystemBuilder builder, Action<Mouse> configurator)
        where TInputReader : IInputDeviceReader
    {
        return builder.AddInputDevice<Mouse, TInputReader>(configurator);
    }

    public static IInputSystemBuilder AddSensorController<TInputReader>(this IInputSystemBuilder builder, Action<SensorController> configurator)
        where TInputReader : IInputDeviceReader
    {
        return builder.AddInputDevice<SensorController, TInputReader>(configurator);
    }

    #endregion

    #region Helpers

    private static IInputSystemBuilder AddInputDevice<TConfigurator, TInputReader>(this IInputSystemBuilder builder, Action<TConfigurator> configuratorAction)
        where TInputReader : IInputDeviceReader
        where TConfigurator : InputDevice
    {
        if (configuratorAction is null)
        {
            throw new ArgumentNullException(nameof(configuratorAction));
        }

        var configurator = (TConfigurator)Activator.CreateInstance(typeof(TConfigurator), typeof(TInputReader));
        configuratorAction(configurator);

        builder.AddInputController(configurator.BuildDeviceConfiguration());

        return builder;
    }

    #endregion
}
