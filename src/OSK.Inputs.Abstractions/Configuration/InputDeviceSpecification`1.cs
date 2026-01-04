using System.Collections.Generic;
using System.Linq;
using OSK.Inputs.Abstractions.Inputs;

namespace OSK.Inputs.Abstractions.Configuration;

public abstract class InputDeviceSpecification<TInput>: InputDeviceSpecification
    where TInput: IInput
{
    /// <summary>
    /// The collection of <see cref="Input"/>s the device uses
    /// </summary>
    public abstract TInput[] Inputs { get; }

    public override IReadOnlyCollection<IInput> GetInputs()
        => [.. Inputs.Cast<IInput>()];
}
