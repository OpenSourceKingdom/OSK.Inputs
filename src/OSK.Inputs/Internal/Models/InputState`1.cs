using OSK.Inputs.Abstractions.Inputs;

namespace OSK.Inputs.Internal.Models;

internal abstract class InputState<TInput>(TInput input): InputState(input.Id)
    where TInput: IInput
{
    public TInput Input => input;
}
