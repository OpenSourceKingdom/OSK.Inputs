using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OSK.Hexagonal.MetaData;
using OSK.Inputs.Models.Runtime;

namespace OSK.Inputs.Ports;

[HexagonalIntegration(HexagonalIntegrationType.IntegrationRequired)]
public interface IInputSystem: IDisposable
{
    Task<IEnumerable<ActivatedInput>> ReadInputsAsync(CancellationToken cancellationToken = default);
}
