using System;
using OSK.Inputs.Models.Inputs;

namespace OSK.Inputs.Ports;
public interface IInputControllerBuilder
{
    IInputControllerBuilder AddInput(IInput input);

    IInputControllerBuilder AddCombinationInput(string name, params string[] inputNames);

    IInputControllerBuilder UseInputReader<TReader>()
        where TReader: IInputReader;

    IInputControllerBuilder UseValidation(Func<IInput, bool> inputValidator);
}
