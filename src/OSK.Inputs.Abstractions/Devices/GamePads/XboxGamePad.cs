using System;
using System.Collections.Generic;
using System.Text;
using OSK.Inputs.Abstractions.Configuration;
using OSK.Inputs.Abstractions.Inputs;
using OSK.Inputs.Abstractions.Devices.GamePads;

namespace OSK.Inputs.Abstractions.Devices.GamePads;

public class XboxGamePad : InputDeviceSpecification<IGamePadInput>
{
    #region Static

    public static InputDeviceIdentity Xbox = new("Xbox", GamePadInputs.GamePadDeviceType);

    #endregion

    #region InputDeviceSpecification Overrides

    public override InputDeviceIdentity DeviceIdentity => Xbox;

    public override IGamePadInput[] Inputs { get; } = [
        GamePadInputs.ButtonSouth, // A
        GamePadInputs.ButtonEast,  // B
        GamePadInputs.ButtonWest,  // X
        GamePadInputs.ButtonNorth, // Y
        GamePadInputs.Menu,        // Menu / Start
        GamePadInputs.Options,     // View / Back
        GamePadInputs.Home,        // Xbox Guide Button
        GamePadInputs.Share,       // Share Button (Series X|S)
        GamePadInputs.DpadUp,
        GamePadInputs.DpadDown,
        GamePadInputs.DpadLeft,
        GamePadInputs.DpadRight,
        GamePadInputs.LeftBumper,
        GamePadInputs.RightBumper,
        GamePadInputs.LeftTrigger,
        GamePadInputs.RightTrigger,
        GamePadInputs.LeftJoyStick,
        GamePadInputs.LeftJoyStickClick,
        GamePadInputs.RightJoyStick,
        GamePadInputs.RightJoyStickClick
    ];

    #endregion
}
