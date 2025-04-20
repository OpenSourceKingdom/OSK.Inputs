using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OSK.Functions.Outputs.Abstractions;
using OSK.Inputs.Models.Configuration;
using OSK.Inputs.Models.Runtime;

namespace OSK.Inputs.Ports;

public interface IInputSchemeRepository
{
    Task<IOutput<IEnumerable<ActiveInputScheme>>> GetActiveInputSchemesAsync(int userId, string inputDefinitionName, CancellationToken cancellationToken = default);

    Task<IOutput<InputScheme>> GetCustomInputSchemeAsync(string inputDefinitionName, string controllerId, string inputSchemeName,
        CancellationToken cancellationToken = default);

    Task<IOutput<IEnumerable<InputScheme>>> GetCustomInputSchemesAsync(string inputDefinitionName, CancellationToken cancellationToken = default);

    Task<IOutput<InputScheme>> SaveCustomInputSchemeAsync(string inputDefinitionName, InputScheme inputScheme,
        CancellationToken cancellationToken = default);

    Task<IOutput> DeleteCustomInputSchemeAsync(string inputDefinitionName, string controllerId, string inputSchemeName,
        CancellationToken cancellationToken = default);

    Task<IOutput> DeleteActiveInputSchemeAsync(int userId, string inputDefinitionName, string controllerId, CancellationToken cancellationToken = default);

    Task<IOutput<ActiveInputScheme>> SaveActiveInputSchemeAsync(ActiveInputScheme inputScheme, CancellationToken cancellationToken = default);
}
