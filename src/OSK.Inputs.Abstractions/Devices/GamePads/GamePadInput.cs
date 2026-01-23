namespace OSK.Inputs.Abstractions.Devices.GamePads;

/// <summary>
/// Defines all of the gamepad inputs available for the input system
/// </summary>
public enum GamePadInput
{
    // System / Utility
    Menu = 1,
    Options = 2,
    Home = 3,
    Share = 4,
    Plus = 5,
    Minus = 6,

    // Face Buttons (Cardinal/Positional naming)
    /// <summary> Square (PS), X (Xbox) </summary>
    ButtonWest = 20,
    /// <summary> Circle (PS), B (Xbox) </summary>
    ButtonEast = 21,
    /// <summary> Triangle (PS), Y (Xbox) </summary>
    ButtonNorth = 22,
    /// <summary> Cross (PS), A (Xbox) </summary>
    ButtonSouth = 23,

    // Shoulder / Triggers
    RightTrigger = 40,
    RightBumper = 41,
    LeftTrigger = 42,
    LeftBumper = 43,

    // D-Pad
    DpadLeft = 60,
    DpadRight = 61,
    DpadUp = 62,
    DpadDown = 63,

    // Joysticks
    LeftJoyStickClick = 80,
    LeftJoyStick = 81,
    RightJoyStickClick = 82,
    RightJoyStick = 83,

    // Touch
    TouchPadClick = 100,
    TouchPad = 101
}
