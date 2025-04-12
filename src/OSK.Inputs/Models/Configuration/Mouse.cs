using System;
using System.Collections.Generic;
using System.Text;
using OSK.Inputs.Models.Inputs;

namespace OSK.Inputs.Models.Configuration;
public class Mouse(Type inputReaderType): InputDevice(MouseName, inputReaderType)
{
    #region Variables

    public static readonly InputDeviceName MouseName = new InputDeviceName("Mouse");

    public static readonly MouseButtonInput LeftClick = new MouseButtonInput(1, "Left Click");
    public static readonly MouseButtonInput RightClick = new MouseButtonInput(2, "Right Click");
    public static readonly MouseButtonInput ScrollWheelClick = new MouseButtonInput(3, "Middle Click");

    public static readonly MouseScrollInput ScrollWheel = new MouseScrollInput(4);

    //public static readonly AnalogInput Movement = new MouseButtonInput("Mouse_Move");

    #endregion

    #region InputDevice Overrides

    public override IEnumerable<IInput> AllInputs { get; } = [
        LeftClick, RightClick, ScrollWheelClick, ScrollWheel
    ];

    #endregion
}
