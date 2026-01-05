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

    // Face Buttons (Cardinal/Positional naming)
    /// <summary> Square (PS), X (Xbox) </summary>
    ButtonWest = 10,
    /// <summary> Circle (PS), B (Xbox) </summary>
    ButtonEast = 11,
    /// <summary> Triangle (PS), Y (Xbox) </summary>
    ButtonNorth = 12,
    /// <summary> Cross (PS), A (Xbox) </summary>
    ButtonSouth = 13,

    // Shoulder / Triggers
    RightTrigger = 30,
    RightBumper = 31,
    LeftTrigger = 32,
    LeftBumper = 33,

    // D-Pad
    DpadLeft = 40,
    DpadRight = 41,
    DpadUp = 42,
    DpadDown = 43,

    // Joysticks
    LeftJoyStickClick = 50,
    LeftJoyStick = 51,
    RightJoyStickClick = 52,
    RightJoyStick = 53,

    // Touch
    TouchPadClick = 60,
    TouchPad = 70
}
