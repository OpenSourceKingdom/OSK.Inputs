namespace OSK.Inputs.Abstractions.Devices.GamePads;

/// <summary>
/// Defines all of the gamepad inputs available for the input system
/// </summary>
public static class GamePadInputs
{
    public const string GamePadDeviceType = "GamePad";

    public static readonly GamePadButton Menu = new(1);
    public static readonly GamePadButton Options = new(2);
    public static readonly GamePadButton Home = new(3);
    public static readonly GamePadButton Share = new(4);

    /// <summary>
    /// Represents the button on the left side of the button face; i.e. Square for Playstation, X for Xbox
    /// </summary>
    public static readonly GamePadButton ButtonWest = new(10);
    /// <summary>
    /// Represents the button on the right side of the button face; i.e Circle for Playstation, B for Xbox
    /// </summary>
    public static readonly GamePadButton ButtonEast = new(11);
    /// <summary>
    /// Represents the button on the north side of the button face; i.e. Triangle for Playstation, Y for Xbox
    /// </summary>
    public static readonly GamePadButton ButtonNorth = new(12);
    /// <summary>
    /// Represents the button on the south side of the button face; i.e. X for Playstation, A for Xbox
    /// </summary>
    public static readonly GamePadButton ButtonSouth = new(13);

    public static readonly GamePadTrigger RightTrigger = new(30);
    public static readonly GamePadButton RightBumper = new(31);
    public static readonly GamePadTrigger LeftTrigger = new(32);
    public static readonly GamePadButton LeftBumper = new(33);

    public static readonly GamePadButton DpadLeft = new(40);
    public static readonly GamePadButton DpadRight = new(41);
    public static readonly GamePadButton DpadUp = new(42);
    public static readonly GamePadButton DpadDown = new(43);

    public static readonly GamePadButton LeftJoyStickClick = new(50);
    public static readonly GamePadJoyStick LeftJoyStick = new(51);
    public static readonly GamePadButton RightJoyStickClick = new(52);
    public static readonly GamePadJoyStick RightJoyStick = new(53);

    public static readonly GamePadButton TouchPadClick = new(60);
    public static readonly GamePadTouchPad TouchPad = new(70);
}
