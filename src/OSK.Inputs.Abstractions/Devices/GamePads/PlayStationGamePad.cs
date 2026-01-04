using OSK.Inputs.Abstractions.Configuration;

namespace OSK.Inputs.Abstractions.Devices.GamePads;

public class PlayStationGamePad : InputDeviceSpecification<IGamePadInput>
{
    #region Static

    public static InputDeviceIdentity PlayStation = new("PLayStation", GamePadInputs.GamePadDeviceType);

    #endregion

    #region InputDeviceSpecification Overrides

    public override InputDeviceIdentity DeviceIdentity => PlayStation;

    public override IGamePadInput[] Inputs { get; } = [
        GamePadInputs.ButtonSouth, // Cross
        GamePadInputs.ButtonEast,  // Circle
        GamePadInputs.ButtonWest,  // Square
        GamePadInputs.ButtonNorth, // Triangle
        GamePadInputs.Menu,        // Options
        GamePadInputs.Options,     // Create / Share
        GamePadInputs.Home,        // PS Button
        GamePadInputs.Share,       // Touchpad Click (Often mapped to a unique ID)
        GamePadInputs.DpadUp,
        GamePadInputs.DpadDown,
        GamePadInputs.DpadLeft,
        GamePadInputs.DpadRight,
        GamePadInputs.LeftBumper,  // L1
        GamePadInputs.RightBumper, // R1
        GamePadInputs.LeftTrigger, // L2
        GamePadInputs.RightTrigger, // R2
        GamePadInputs.LeftJoyStick,
        GamePadInputs.LeftJoyStickClick, // L3
        GamePadInputs.RightJoyStick,
        GamePadInputs.RightJoyStickClick, // R3
        GamePadInputs.TouchPad,
        GamePadInputs.TouchPadClick
    ];

    #endregion
}
