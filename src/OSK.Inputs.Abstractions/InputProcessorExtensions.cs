using System;
using OSK.Inputs.Abstractions.Runtime;
using OSK.Operations.Outputs.Models;

namespace OSK.Inputs.Abstractions;

public static class InputProcessorExtensions
{
    public static Output<ProcessedInputResult> ProcessEvent(this IInputProcessor inputProcessor, TimeSpan deltaTime, InputEvent inputEvent)
        => inputProcessor.ProcessEvent(deltaTime, inputEvent, new InputEventProcessOptions());
}
