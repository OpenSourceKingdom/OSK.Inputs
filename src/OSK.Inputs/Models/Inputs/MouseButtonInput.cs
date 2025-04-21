using OSK.Inputs.Models.Configuration;

namespace OSK.Inputs.Models.Inputs;
public class MouseButtonInput(int id, string name): DigitalInput(id, name, Mouse.MouseName.Name), IMouseInput
{
}
