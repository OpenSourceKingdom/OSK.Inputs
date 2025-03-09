using System;
using OSK.Inputs.Models.Inputs;

namespace OSK.Inputs.Models.Configuration;
public interface IInputReceiverDescriptor
{
    string ReceiverName { get; }

    Type InputSystemType { get; }

    bool IsValidInput(IInput input);
}
