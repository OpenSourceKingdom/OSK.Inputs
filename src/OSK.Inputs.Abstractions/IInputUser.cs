using System.Collections.Generic;
using OSK.Hexagonal.MetaData;
using OSK.Inputs.Abstractions.Runtime;

namespace OSK.Inputs.Abstractions;

/// <summary>
/// A user that interacts with the system using some form of <see cref="RuntimeDeviceIdentifier"/>
/// </summary>
///
[HexagonalIntegration(HexagonalIntegrationType.LibraryProvided)]
public interface IInputUser
{
    int Id { get; }

    ActiveInputScheme ActiveScheme { get; }

    IReadOnlyCollection<PairedDevice> PairedDevices { get; }

    PairedDevice? GetDevice(int deviceId);
}
