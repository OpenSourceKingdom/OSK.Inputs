namespace OSK.Inputs.Models.Inputs;
public static class GamePadInputs
{
    public static readonly DigitalInput Menu = new DigitalInput("Menu");

    public static readonly DigitalInput X = new DigitalInput("X");
    public static readonly DigitalInput Y = new DigitalInput("Y");
    public static readonly DigitalInput A = new DigitalInput("A");
    public static readonly DigitalInput B = new DigitalInput("B");

    public static readonly DigitalInput Square = new DigitalInput("Square");
    public static readonly DigitalInput Triangle = new DigitalInput("Triangle");
    public static readonly DigitalInput Circle = new DigitalInput("Circle");

    public static readonly DigitalInput RightTrigger = new DigitalInput("Right_Trigger");
    public static readonly DigitalInput RightBumper = new DigitalInput("Right_Bumper");
    public static readonly DigitalInput LeftTrigger = new DigitalInput("Left_Trigger");
    public static readonly DigitalInput LeftBumper = new DigitalInput("Left_Bumper");

    public static readonly DigitalInput DpadLeft = new DigitalInput("Dpad_Left");
    public static readonly DigitalInput DpadRight = new DigitalInput("Dpad_Right");
    public static readonly DigitalInput DpadUp = new DigitalInput("Dpad_Up");
    public static readonly DigitalInput DpadDown = new DigitalInput("Dpad_Down");

    public static readonly DigitalInput LeftJoyStickClick = new DigitalInput("Left_JoyStick_Click");
    public static readonly AnalogInput LeftJoyStick = new AnalogInput("Left_JoyStick");
    public static readonly DigitalInput RightJoyStickClick = new DigitalInput("Right_JoyStick_Click");
    public static readonly AnalogInput RightJoyStick = new AnalogInput("Right_JoyStick");
}
