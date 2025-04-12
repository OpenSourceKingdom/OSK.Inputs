using System;
using System.Collections.Generic;
using System.Text;
using OSK.Inputs.Models.Inputs;

namespace OSK.Inputs.Models.Configuration;

public abstract class GamePadDevice(InputDeviceName inputControllerName, Type inputReaderType): InputDevice(inputControllerName, inputReaderType)
{
    #region Static

    public const string DeviceTypeName = "Gamepad";

    public static readonly GamePadButtonInput Menu = new GamePadButtonInput(1, "Menu");
    public static readonly GamePadButtonInput Options = new GamePadButtonInput(2, "Options");

    public static readonly GamePadButtonInput X = new GamePadButtonInput(10, "X");
    public static readonly GamePadButtonInput Y = new GamePadButtonInput(11, "Y");
    public static readonly GamePadButtonInput A = new GamePadButtonInput(12, "A");
    public static readonly GamePadButtonInput B = new GamePadButtonInput(13, "B");

    public static readonly GamePadButtonInput Square = new GamePadButtonInput(14, "Square");
    public static readonly GamePadButtonInput Triangle = new GamePadButtonInput(15, "Triangle");
    public static readonly GamePadButtonInput Circle = new GamePadButtonInput(16, "Circle");

    public static readonly GamePadButtonInput RightTrigger = new GamePadButtonInput(30, "Right Trigger");
    public static readonly GamePadButtonInput RightBumper = new GamePadButtonInput(31, "Right Bumper");
    public static readonly GamePadButtonInput LeftTrigger = new GamePadButtonInput(32, "Left Trigger");
    public static readonly GamePadButtonInput LeftBumper = new GamePadButtonInput(33, "Left Bumper");

    public static readonly GamePadButtonInput DpadLeft = new GamePadButtonInput(40, "Dpad Left");
    public static readonly GamePadButtonInput DpadRight = new GamePadButtonInput(41, "Dpad Right");
    public static readonly GamePadButtonInput DpadUp = new GamePadButtonInput(42, "Dpad Up");
    public static readonly GamePadButtonInput DpadDown = new GamePadButtonInput(43, "Dpad Down");

    public static readonly GamePadButtonInput LeftJoyStickClick = new GamePadButtonInput(50, "Left JoyStick Click");
    public static readonly GamePadStickInput LeftJoyStick = new GamePadStickInput(51, "Left JoyStick");
    public static readonly GamePadButtonInput RightJoyStickClick = new GamePadButtonInput(52, "Right JoyStick Click");
    public static readonly GamePadStickInput RightJoyStick = new GamePadStickInput(53, "Right JoyStick");

    #endregion
}
