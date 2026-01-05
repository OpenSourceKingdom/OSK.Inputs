using System.Numerics;
using OSK.Inputs.Abstractions.Inputs;

namespace OSK.Inputs.Abstractions.Runtime;

/// <summary>
/// Represents a pointer event on a device. This could be a touch, mouse, or similar event.
/// </summary>
/// <param name="device">The device the input belongs to</param>
/// <param name="input">The input that triggered the event</param>
/// <param name="phase">The phase of the input</param>
/// <param name="pointerId">The pointer that this event is associated with</param>
/// <param name="position">The position of the pointer</param>
public class InputPointerEvent(RuntimeDeviceIdentifier device, IDeviceInput input, InputPhase phase,
    int pointerId, Vector2 position)
    : DeviceInputEvent(device, input, phase)
{
    #region Variables

    public int PointerId => pointerId;

    public Vector2 Position => position;

    #endregion
}
