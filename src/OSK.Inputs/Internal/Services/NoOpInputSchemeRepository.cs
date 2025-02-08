using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OSK.Functions.Outputs.Abstractions;
using OSK.Inputs.Models.Configuration;
using OSK.Inputs.Models.Runtime;
using OSK.Inputs.Ports;

namespace OSK.Inputs.Internal.Services;
internal class NoOpInputSchemeRepository(IOutputFactory outputFactory) : IInputSchemeRepository
{
    public Task<IOutput> DeleteInputSchemeAsync(string inputDefinitionName, string controllerName,
        string inputSchemeName, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(outputFactory.Succeed());
    }

    public Task<IOutput<IEnumerable<ActiveInputScheme>>> GetActiveInputSchemesAsync(string inputDefinitionName, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(outputFactory.Succeed(Enumerable.Empty<ActiveInputScheme>()));
    }

    public Task<IOutput<InputScheme>> GetInputSchemeAsync(string inputDefinitionName, string controllerName,
        string inputSchemeName, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(outputFactory.NotFound<InputScheme>());
    }

    public Task<IOutput<IEnumerable<InputScheme>>> GetInputSchemesAsync(string inputDefinitionName, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(outputFactory.Succeed(Enumerable.Empty<InputScheme>()));
    }

    public Task<IOutput<InputScheme>> SaveInputSchemeAsync(string inputDefinitionName, InputScheme inputScheme, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(outputFactory.Succeed(inputScheme));
    }
}
