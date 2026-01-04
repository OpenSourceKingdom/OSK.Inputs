using OSK.Inputs.Abstractions.Inputs;

namespace OSK.Inputs.Abstractions.Runtime;

/// <summary>
/// Represents a virtual input that was triggered by the input system. This differs from a <see cref="PhysicalInputEvent"/>
/// which comes from a <see cref="PhysicalInput"/>. The virtual inputs are managed by the software and are triggered on behalf
/// of the user.
/// </summary>
/// <param name="virtualInput">The virtual input that was triggered</param>
/// <param name="phase">The phase of the input triggered</param>
public class VirtualInputEvent(VirtualInput virtualInput, InputPhase phase)
    : InputEvent(virtualInput, phase)
{
}
