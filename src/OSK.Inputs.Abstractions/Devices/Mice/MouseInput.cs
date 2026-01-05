namespace OSK.Inputs.Abstractions.Devices.Mice;

/// <summary>
/// Defines all of the keyboard inputs available for the input system
/// </summary>
public enum MouseInput
{
    // Primary Buttons
    LeftClick = 1,
    RightClick = 2,
    ScrollWheelClick = 3,

    /// <summary> Represents the vertical/horizontal scroll delta </summary>
    ScrollWheel = 4,

    /// <summary> Represents the X/Y movement of the mouse cursor </summary>
    MouseMovement = 5,
    
    /// <summary> Back button (often found on gaming/office mice) </summary>
    Button4 = 6,
    /// <summary> Forward button (often found on gaming/office mice) </summary>
    Button5 = 7
}
