using System;
using System.Threading;
using System.Threading.Tasks;
using OSK.Hexagonal.MetaData;
using OSK.Inputs.Models.Runtime;

namespace OSK.Inputs.Ports;

[HexagonalIntegration(HexagonalIntegrationType.IntegrationRequired)]
public interface IInputReader: IDisposable
{
    event Action<InputDeviceIdentifier> OnControllerDisconnected;
    event Action<InputDeviceIdentifier> OnControllerReconnected;

    Task ReadInputsAsync(UserInputReadContext context, CancellationToken cancellationToken = default);
}
