using System;
using System.Collections.Generic;
using System.Text;
using OSK.Inputs.Abstractions.Inputs;

namespace OSK.Inputs.Internal.Models;

internal abstract class InputState<TInput>(TInput input): InputState(input.Id)
    where TInput: Input
{
    public TInput Input => input;
}
