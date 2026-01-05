namespace OSK.Inputs.Abstractions.Devices.GamePads;

public class PlayStationGamePad : GamePadDeviceSpecification
{
    #region Static

    public static InputDeviceFamily PlayStation = new("PlayStation", InputDeviceType.GamePad);

    #endregion

    #region InputDeviceSpecification Overrides

    public override InputDeviceFamily DeviceFamily => PlayStation;

    protected override GamePadInput[] Inputs { get; } = [
        GamePadInput.ButtonSouth, // Cross
        GamePadInput.ButtonEast,  // Circle
        GamePadInput.ButtonWest,  // Square
        GamePadInput.ButtonNorth, // Triangle
        GamePadInput.Menu,        // Options
        GamePadInput.Options,     // Create / Share
        GamePadInput.Home,        // PS Button
        GamePadInput.Share,       // Touchpad Click (Often mapped to a unique ID)
        GamePadInput.DpadUp,
        GamePadInput.DpadDown,
        GamePadInput.DpadLeft,
        GamePadInput.DpadRight,
        GamePadInput.LeftBumper,  // L1
        GamePadInput.RightBumper, // R1
        GamePadInput.LeftTrigger, // L2
        GamePadInput.RightTrigger, // R2
        GamePadInput.LeftJoyStick,
        GamePadInput.LeftJoyStickClick, // L3
        GamePadInput.RightJoyStick,
        GamePadInput.RightJoyStickClick, // R3
        GamePadInput.TouchPad,
        GamePadInput.TouchPadClick
    ];

    #endregion
}
