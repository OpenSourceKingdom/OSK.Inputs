namespace OSK.Inputs.Models.Inputs;
public static class MouseInputs
{
    public static readonly DigitalInput LeftClick = new DigitalInput("Left_Click");
    public static readonly DigitalInput RightClick = new DigitalInput("Right_Click");

    public static readonly DigitalInput ScrollWheelClick = new DigitalInput("Middle_Click");
    public static readonly AnalogInput ScrollWheel = new AnalogInput("Scroll_Wheel");

    public static readonly AnalogInput Movement = new AnalogInput("Mouse_Move");
}
