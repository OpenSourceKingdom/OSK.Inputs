using System;
using System.Collections.Generic;
using System.Text;
using OSK.Inputs.Models.Inputs;
using OSK.Inputs.Options;

namespace OSK.Inputs.Models.Configuration;
public class XboxController(Type inputReaderType) : GamePadDevice(XboxControllerName, inputReaderType)
{
    #region Static

    public static readonly InputDeviceName XboxControllerName = new InputDeviceName("XboxController");

    #endregion

    #region InputDevice Overrides

    public override IEnumerable<IInput> AllInputs { get; } = [
        X, Y, A, B, Menu,
        DpadDown, DpadLeft, DpadRight, DpadUp,
        RightTrigger, RightBumper, LeftTrigger, LeftBumper,
        LeftJoyStick, LeftJoyStickClick, RightJoyStick, RightJoyStickClick        
    ];

    #endregion
}
