using OSK.Functions.Outputs.Abstractions;
using OSK.Inputs.Models.Configuration;
using OSK.Inputs.Models.Runtime;
using OSK.Inputs.Ports;

namespace OSK.Inputs.UnitTests._Helpers;

public class TestSchemeRepository : IInputSchemeRepository
{
    public Task<IOutput> DeleteActiveInputSchemeAsync(int userId, string inputDefinitionName, InputControllerName controllerName, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<IOutput> DeleteCustomInputSchemeAsync(string inputDefinitionName, InputControllerName controllerName, string inputSchemeName, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<IOutput<IEnumerable<ActiveInputScheme>>> GetActiveInputSchemesAsync(int userId, string inputDefinitionName, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<IOutput<InputScheme>> GetCustomInputSchemeAsync(string inputDefinitionName, InputControllerName controllerName, string inputSchemeName, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<IOutput<IEnumerable<InputScheme>>> GetCustomInputSchemesAsync(string inputDefinitionName, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<IOutput<ActiveInputScheme>> SaveActiveInputSchemeAsync(ActiveInputScheme inputScheme, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<IOutput<InputScheme>> SaveCustomInputSchemeAsync(string inputDefinitionName, InputScheme inputScheme, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
