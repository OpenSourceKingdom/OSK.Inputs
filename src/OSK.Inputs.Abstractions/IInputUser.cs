using System.Collections.Generic;
using OSK.Inputs.Abstractions.Runtime;

namespace OSK.Inputs.Abstractions;

public interface IInputUser
{
    int Id { get; }

    ActiveInputScheme ActiveScheme { get; }

    IReadOnlyCollection<PairedDevice> PairedDevices { get; }

    PairedDevice? GetDevice(int deviceId);
}
