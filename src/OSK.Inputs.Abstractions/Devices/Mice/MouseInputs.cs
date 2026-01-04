using OSK.Inputs.Abstractions.Devices.Mice;

namespace OSK.Inputs.Abstractions.Devices.Mice;

/// <summary>
/// Defines all of the keyboard inputs available for the input system
/// </summary>
public static class MouseInputs
{
    public const string MouseDeviceType = "Mouse";

    public static readonly MouseButton LeftClick = new(1);
    public static readonly MouseButton RightClick = new(2);
    public static readonly MouseButton ScrollWheelClick = new(3);

    public static readonly MouseScrollWheel ScrollWheel = new(4);

    public static readonly MouseMovement MouseMovement = new(5);
}
