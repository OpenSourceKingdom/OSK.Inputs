using System;
using OSK.Inputs.Models.Configuration;
using OSK.Inputs.Ports;

namespace OSK.Inputs;
public static class InputSystemBuilderExtensions
{
    #region Device Extensions

    public static IInputSystemBuilder AddXboxController<TInputReader>(this IInputSystemBuilder builder)
        where TInputReader : IInputDeviceReader
    {
        return builder.AddInputDevice<XboxController, TInputReader>();
    }

    public static IInputSystemBuilder AddPlayStationController<TInputReader>(this IInputSystemBuilder builder)
        where TInputReader : IInputDeviceReader
    {
        return builder.AddInputDevice<PlayStationController, TInputReader>();
    }

    public static IInputSystemBuilder AddKeyboard<TInputReader>(this IInputSystemBuilder builder)
        where TInputReader : IInputDeviceReader
    {
        return builder.AddInputDevice<Keyboard, TInputReader>();
    }

    public static IInputSystemBuilder AddMouse<TInputReader>(this IInputSystemBuilder builder)
        where TInputReader : IInputDeviceReader
    {
        return builder.AddInputDevice<Mouse, TInputReader>();
    }

    public static IInputSystemBuilder AddSensorController<TInputReader>(this IInputSystemBuilder builder)
        where TInputReader : IInputDeviceReader
    {
        return builder.AddInputDevice<SensorController, TInputReader>();
    }

    public static IInputSystemBuilder AddCustomDevice<TDevice, TInputReader>(this IInputSystemBuilder builder)
        where TDevice : InputDevice
        where TInputReader : IInputDeviceReader
    {
        return builder.AddInputDevice<TDevice, TInputReader>();
    }

    #endregion

    #region Helpers

    private static IInputSystemBuilder AddInputDevice<TDevice, TInputReader>(this IInputSystemBuilder builder)
        where TInputReader : IInputDeviceReader
        where TDevice : InputDevice
    {
        var device = (TDevice)Activator.CreateInstance(typeof(TDevice), typeof(TInputReader));
        builder.AddInputDevice(device.GetDeviceConfiguration());

        return builder;
    }

    #endregion
}
