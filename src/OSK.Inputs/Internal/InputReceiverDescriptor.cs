using System;
using OSK.Inputs.Models.Configuration;
using OSK.Inputs.Models.Inputs;

namespace OSK.Inputs.Internal;

internal class InputReceiverDescriptor(string receiverName, Type inputReceiverType, Func<IInput, bool> validator) : IInputReceiverDescriptor
{
    public string ReceiverName => receiverName;

    public Type InputReceiverType => inputReceiverType;

    public bool IsValidInput(IInput input) => validator(input);
}
