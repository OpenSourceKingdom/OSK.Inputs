using System.Linq;
using OSK.Inputs.Abstractions.Inputs;

namespace OSK.Inputs.Internal.Models;

internal class VirtualInputState(VirtualInput virtualInput): InputState<VirtualInput>(virtualInput)
{
    public int TapCount { get; set; }

    public int[] LinkedInputIds { get; } = virtualInput.GetLinkedInputs().OfType<DeviceInput>().Select(input => input.Id).ToArray();

    public override IInput GetActiveInput() => Input;
}
