using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OSK.Hexagonal.MetaData;

namespace OSK.Inputs.Abstractions;

[HexagonalIntegration(HexagonalIntegrationType.IntegrationRequired)]
public interface IInputListener
{
    ValueTask<IEnumerable<InputActivation>> ListenAsync(CancellationToken cancellationToken = default);
}
