using System.Numerics;

namespace OSK.Inputs.Abstractions;

public class InputPointerActivation(RuntimeDeviceIdentifier device, Input input, InputPhase phase, int pointerId, Vector2 position)
    : DeviceInputActivation(device, input, phase)
{
    public int PointerId => pointerId;

    public Vector2 Position => position;
}
