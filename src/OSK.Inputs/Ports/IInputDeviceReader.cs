using System;
using System.Threading;
using System.Threading.Tasks;
using OSK.Hexagonal.MetaData;
using OSK.Inputs.Models.Runtime;

namespace OSK.Inputs.Ports;

[HexagonalIntegration(HexagonalIntegrationType.IntegrationRequired)]
public interface IInputDeviceReader: IDisposable
{
    event Action<InputDeviceIdentifier> OnDeviceDisconnected;
    event Action<InputDeviceIdentifier> OnControllerReconnected;

    Task ReadInputsAsync(UserInputReadContext context, CancellationToken cancellationToken = default);
}
