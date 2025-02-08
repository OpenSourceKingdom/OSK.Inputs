using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OSK.Functions.Outputs.Abstractions;
using OSK.Hexagonal.MetaData;
using OSK.Inputs.Models.Configuration;

namespace OSK.Inputs.Ports;

[HexagonalIntegration(HexagonalIntegrationType.LibraryProvided, HexagonalIntegrationType.ConsumerPointOfEntry)]
public interface IInputManager
{
    ValueTask<IOutput<IEnumerable<InputDefinition>>> GetInputDefinitionsAsync(CancellationToken cancellationToken = default);

    Task<IOutput<IInputHandler>> GetInputHandlerAsync(string inputDefinitionName, CancellationToken cancellationToken = default);

    Task<IOutput<InputScheme>> SaveInputSchemeAsync(InputScheme inputScheme, CancellationToken cancellationToken = default);

    Task<IOutput> DeleteInputSchemeAsync(string inputDefinitionName, string controllerName, string schemeName,
        CancellationToken cancellationToken = default);
}
