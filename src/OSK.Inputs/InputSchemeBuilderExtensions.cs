using System;
using OSK.Inputs.Models.Configuration;
using OSK.Inputs.Models.Inputs;
using OSK.Inputs.Ports;

namespace OSK.Inputs;
public static class InputSchemeBuilderExtensions
{
    public static IInputSchemeBuilder UseKeyboard(this IInputSchemeBuilder builder, 
        Action<IInputDeviceActionBuilder<IKeyboardInput>> actionConfigurator)
    {
        builder.AddDevice(Keyboard.KeyboardName, actionConfigurator);
        return builder;
    }

    public static IInputSchemeBuilder UseMouse(this IInputSchemeBuilder builder,
        Action<IInputDeviceActionBuilder<IMouseInput>> actionConfigurator)
    {
        builder.AddDevice(Mouse.MouseName, actionConfigurator);
        return builder;
    }

    public static IInputSchemeBuilder UsePlayStationGamePad(this IInputSchemeBuilder builder,
        Action<IInputDeviceActionBuilder<IGamePadInput>> actionConfigurator)
    {
        builder.AddDevice(PlayStationController.PlayStationControllerName, actionConfigurator);
        return builder;
    }

    public static IInputSchemeBuilder UseXboxGamePad(this IInputSchemeBuilder builder,
        Action<IInputDeviceActionBuilder<IGamePadInput>> actionConfigurator)
    {
        builder.AddDevice(XboxController.XboxControllerName, actionConfigurator);
        return builder;
    }

    public static IInputSchemeBuilder UseSensors(this IInputSchemeBuilder builder,
        Action<IInputDeviceActionBuilder<ISensorInput>> actionConfigurator)
    {
        builder.AddDevice(SensorController.SensorControllerName, actionConfigurator);
        return builder;
    }
}
