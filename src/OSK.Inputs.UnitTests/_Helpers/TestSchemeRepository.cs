using OSK.Functions.Outputs.Abstractions;
using OSK.Inputs.Models.Configuration;
using OSK.Inputs.Models.Runtime;
using OSK.Inputs.Ports;

namespace OSK.Inputs.UnitTests._Helpers;

public class TestSchemeRepository : IInputSchemeRepository
{
    public Task<IOutput> DeleteInputSchemeAsync(string inputDefinitionName, string controllerName, string inputSchemeName, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<IOutput<IEnumerable<ActiveInputScheme>>> GetActiveInputSchemesAsync(string inputDefinitionName, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<IOutput<InputScheme>> GetInputSchemeAsync(string inputDefinitionName, string controllerName, string inputSchemeName, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<IOutput<IEnumerable<InputScheme>>> GetInputSchemesAsync(string inputDefinitionName, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<IOutput<InputScheme>> SaveInputSchemeAsync(string inputDefinitionName, InputScheme inputScheme, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
