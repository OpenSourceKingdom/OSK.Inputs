using System;
using OSK.Inputs.Models.Configuration;
using OSK.Inputs.Models.Inputs;
using OSK.Inputs.Ports;

namespace OSK.Inputs;
public static class InputSchemeBuilderExtensions
{
    public static IInputSchemeBuilder AddKeyboardMaps(this IInputSchemeBuilder builder, 
        Action<IInputDeviceActionBuilder<IKeyboardInput>> actionConfigurator)
    {
        builder.AddDevice(Keyboard.KeyboardName, actionConfigurator);
        return builder;
    }

    public static IInputSchemeBuilder AddMouseMaps(this IInputSchemeBuilder builder,
        Action<IInputDeviceActionBuilder<IMouseInput>> actionConfigurator)
    {
        builder.AddDevice(Mouse.MouseName, actionConfigurator);
        return builder;
    }

    public static IInputSchemeBuilder AddPlayStationMaps(this IInputSchemeBuilder builder,
        Action<IInputDeviceActionBuilder<IGamePadInput>> actionConfigurator)
    {
        builder.AddDevice(PlayStationController.PlayStationControllerName, actionConfigurator);
        return builder;
    }

    public static IInputSchemeBuilder AddXboxMaps(this IInputSchemeBuilder builder,
        Action<IInputDeviceActionBuilder<IGamePadInput>> actionConfigurator)
    {
        builder.AddDevice(XboxController.XboxControllerName, actionConfigurator);
        return builder;
    }

    public static IInputSchemeBuilder AddSensorMaps(this IInputSchemeBuilder builder,
        Action<IInputDeviceActionBuilder<ISensorInput>> actionConfigurator)
    {
        builder.AddDevice(SensorController.SensorControllerName, actionConfigurator);
        return builder;
    }
}
