using System;
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
    private readonly Dictionary<int, Dictionary<string, Dictionary<string, ActiveInputScheme>>> _activeSchemes = [];

    public Task<IOutput> DeleteInputSchemeAsync(string inputDefinitionName, string controllerName,
        string inputSchemeName, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(outputFactory.Succeed());
    }

    public Task<IOutput<IEnumerable<ActiveInputScheme>>> GetActiveInputSchemesAsync(string inputDefinitionName, int playerId, CancellationToken cancellationToken = default)
    {
        var activeSchemes = Enumerable.Empty<ActiveInputScheme>();
        if (_activeSchemes.TryGetValue(playerId, out var activePlayerSchemes)
             && activePlayerSchemes.TryGetValue(inputDefinitionName, out var activeControllerSchemes))
        {
            activeSchemes = activeControllerSchemes.Values;
        }

        return Task.FromResult(outputFactory.Succeed(activeSchemes));
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

    public Task<IOutput<ActiveInputScheme>> SaveActiveInputSchemeAsync(ActiveInputScheme inputScheme, CancellationToken cancellationToken = default)
    {
        if (inputScheme is null)
        {
            throw new ArgumentNullException(nameof(inputScheme));
        }

        if (!_activeSchemes.TryGetValue(inputScheme.PlayerId, out var activePlayerSchemes))
        {
            activePlayerSchemes = [];
            _activeSchemes.Add(inputScheme.PlayerId, activePlayerSchemes);
        }
        if (!activePlayerSchemes.TryGetValue(inputScheme.InputDefinitionName, out var activeSchemes))
        {
            activeSchemes = [];
            activePlayerSchemes.Add(inputScheme.InputDefinitionName, activeSchemes);
        }

        activeSchemes.Add(inputScheme.ControllerName, inputScheme);
        return Task.FromResult(outputFactory.Succeed(inputScheme));
    }

    public Task<IOutput<InputScheme>> SaveInputSchemeAsync(string inputDefinitionName, InputScheme inputScheme, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(outputFactory.Succeed(inputScheme));
    }
}
