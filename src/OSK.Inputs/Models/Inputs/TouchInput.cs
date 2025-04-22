namespace OSK.Inputs.Models.Inputs;
public class TouchInput(int touchCount): SensorInput(touchCount, $"{touchCount} {(touchCount == 1 ? "Touch" : "Touches")}")
{
    public int TouchCount => Id;
}
