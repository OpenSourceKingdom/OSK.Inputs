namespace OSK.Inputs.Abstractions.Devices.GamePads;

public class XboxGamePad : GamePadDeviceSpecification
{
    #region Static

    public static InputDeviceFamily Xbox = new("Xbox", InputDeviceType.GamePad);

    #endregion

    #region InputDeviceSpecification Overrides

    public override InputDeviceFamily DeviceFamily => Xbox;

    protected override GamePadInput[] Inputs { get; } = [
        GamePadInput.ButtonSouth, // A
        GamePadInput.ButtonEast,  // B
        GamePadInput.ButtonWest,  // X
        GamePadInput.ButtonNorth, // Y
        GamePadInput.Menu,        // Menu / Start
        GamePadInput.Options,     // View / Back
        GamePadInput.Home,        // Xbox Guide Button
        GamePadInput.Share,       // Share Button (Series X|S)
        GamePadInput.DpadUp,
        GamePadInput.DpadDown,
        GamePadInput.DpadLeft,
        GamePadInput.DpadRight,
        GamePadInput.LeftBumper,
        GamePadInput.RightBumper,
        GamePadInput.LeftTrigger,
        GamePadInput.RightTrigger,
        GamePadInput.LeftJoyStick,
        GamePadInput.LeftJoyStickClick,
        GamePadInput.RightJoyStick,
        GamePadInput.RightJoyStickClick
    ];

    #endregion
}
