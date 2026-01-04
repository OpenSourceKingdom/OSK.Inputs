using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OSK.Functions.Outputs.Abstractions;
using OSK.Functions.Outputs.Logging.Abstractions;
using OSK.Inputs.Abstractions;
using OSK.Inputs.Abstractions.Configuration;
using OSK.Inputs.Abstractions.Runtime;

namespace OSK.Inputs.Internal.Services;

internal class InMemorySchemeRepository(IOutputFactory<InMemorySchemeRepository> outputFactory) : IInputSchemeRepository
{
    #region Variables

    internal readonly Dictionary<int, List<PreferredInputScheme>> _preferredSchemeLookup = [];

    #endregion

    #region IInputSchemeRepository

    public bool AllowCustomSchemes => false;

    public Task<IOutput<PreferredInputScheme>> SavePreferredSchemeAsync(PreferredInputScheme scheme, CancellationToken cancellationToken = default)
    {
        if (!_preferredSchemeLookup.TryGetValue(scheme.UserId, out var schemes))
        {
            schemes = [];
        }

        _preferredSchemeLookup[scheme.UserId] = schemes.Where(s => !s.DefinitionName.Equals(scheme.DefinitionName, StringComparison.OrdinalIgnoreCase))
                    .Append(scheme)
                    .ToList();

        return Task.FromResult(outputFactory.Succeed(scheme));
    }

    public Task<IOutput<IEnumerable<PreferredInputScheme>>> GetPreferredSchemesAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(outputFactory.Succeed(_preferredSchemeLookup.Values.SelectMany(v => v)));
    }

    public Task<IOutput> DeleteCustomSchemeAsync(string inputDefinitionId, string schemeName, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException($"Default scheme repository does not support custom schemes, please register a custom repository if this is desired.");
    }

    public Task<IOutput<IEnumerable<CustomInputScheme>>> GetCustomSchemesAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException($"Default scheme repository does not support custom schemes, please register a custom repository if this is desired.");
    }

    public Task<IOutput<CustomInputScheme>> SaveCustomInputScheme(CustomInputScheme scheme, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException($"Default scheme repository does not support custom schemes, please register a custom repository if this is desired.");
    }

    #endregion
}
