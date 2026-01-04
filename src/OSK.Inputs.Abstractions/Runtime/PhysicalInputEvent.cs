using OSK.Inputs.Abstractions.Inputs;

namespace OSK.Inputs.Abstractions.Runtime;

/// <summary>
/// Represents a physical input event from the input system. This differs from the <see cref="VirtualInputEvent"/> which is 
/// determined by the software; this input is a physical entity on a device that is intereacted with by the user.
/// </summary>
/// <param name="deviceIdentifier">The device that the input belongs to</param>
/// <param name="input">The input that triggered this event</param>
/// <param name="phase">The specific phase for the input</param>
public abstract class PhysicalInputEvent(RuntimeDeviceIdentifier deviceIdentifier, PhysicalInput input, InputPhase phase)
    : InputEvent(input, phase)
{
    /// <summary>
    /// The raw physical input that was used to trigger this event
    /// </summary>
    public new PhysicalInput Input => input;

    /// <summary>
    /// The specific device that initiated the input
    /// </summary>
    public RuntimeDeviceIdentifier DeviceIdentifier => deviceIdentifier;
}
