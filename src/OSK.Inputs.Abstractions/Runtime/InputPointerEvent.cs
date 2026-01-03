using System.Numerics;
using OSK.Inputs.Abstractions.Inputs;

namespace OSK.Inputs.Abstractions.Runtime;

public class InputPointerEvent(RuntimeDeviceIdentifier device, PhysicalInput input, InputPhase phase, int pointerId, Vector2 position)
    : PhysicalInputEvent(device, input, phase)
{
    public int PointerId => pointerId;

    public Vector2 Position => position;
}
