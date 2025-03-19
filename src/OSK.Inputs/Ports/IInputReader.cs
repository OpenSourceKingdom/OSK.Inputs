using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OSK.Hexagonal.MetaData;
using OSK.Inputs.Models.Configuration;
using OSK.Inputs.Models.Runtime;

namespace OSK.Inputs.Ports;

[HexagonalIntegration(HexagonalIntegrationType.IntegrationRequired)]
public interface IInputReader: IDisposable
{
    event Action<InputControllerIdentifier> OnControllerDisconnected;
    event Action<InputControllerIdentifier> OnControllerReconnected;

    Task<IEnumerable<ActivatedInput>> ReadInputsAsync(InputScheme inputScheme, CancellationToken cancellationToken = default);
}
