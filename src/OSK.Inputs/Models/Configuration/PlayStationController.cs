﻿using System;
using System.Collections.Generic;
using OSK.Inputs.Models.Inputs;

namespace OSK.Inputs.Models.Configuration;
public class PlayStationController(Type inputReaderType) : GamePadDevice(PlayStationControllerName, inputReaderType)
{
    #region Static

    public static readonly InputDeviceName PlayStationControllerName = new InputDeviceName("PlayStationController");

    #endregion

    #region InputDevice Overrides

    public override IEnumerable<IInput> AllInputs { get; } = [
        Square, Triangle, Circle, X, Menu,
        DpadLeft, DpadRight, DpadUp, DpadDown, RightTrigger, RightBumper, LeftTrigger, LeftBumper,
        LeftJoyStick, LeftJoyStickClick, RightJoyStick, RightJoyStickClick
    ];

    #endregion
}
