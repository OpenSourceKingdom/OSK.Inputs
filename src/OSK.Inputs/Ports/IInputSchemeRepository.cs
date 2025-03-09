using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OSK.Functions.Outputs.Abstractions;
using OSK.Inputs.Models.Configuration;
using OSK.Inputs.Models.Runtime;

namespace OSK.Inputs.Ports;

public interface IInputSchemeRepository
{
    Task<IOutput<IEnumerable<ActiveInputScheme>>> GetActiveInputSchemesAsync(string inputDefinitionName, int playerId, CancellationToken cancellationToken = default);

    Task<IOutput<InputScheme>> GetInputSchemeAsync(string inputDefinitionName, string controllerName, string inputSchemeName,
        CancellationToken cancellationToken = default);

    Task<IOutput<IEnumerable<InputScheme>>> GetInputSchemesAsync(string inputDefinitionName, CancellationToken cancellationToken = default);

    Task<IOutput<ActiveInputScheme>> SaveActiveInputSchemeAsync(ActiveInputScheme inputScheme, CancellationToken cancellationToken = default);

    Task<IOutput<InputScheme>> SaveInputSchemeAsync(string inputDefinitionName, InputScheme inputScheme,
        CancellationToken cancellationToken = default);

    Task<IOutput> DeleteInputSchemeAsync(string inputDefinitionName, string controllerName, string inputSchemeName,
        CancellationToken cancellationToken = default);
}
