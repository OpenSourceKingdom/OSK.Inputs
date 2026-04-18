using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OSK.Inputs.Abstractions;
using OSK.Inputs.Abstractions.Configuration;
using OSK.Inputs.Abstractions.Runtime;
using OSK.Operations.Outputs;
using OSK.Operations.Outputs.Models;

namespace OSK.Inputs.Internal.Services;

internal class InMemorySchemeRepository : IInputSchemeRepository
{
    #region Variables

    internal readonly Dictionary<int, List<PreferredInputScheme>> _preferredSchemeLookup = [];

    #endregion

    #region IInputSchemeRepository

    public bool AllowCustomSchemes => false;

    public Task<Output<PreferredInputScheme>> SavePreferredSchemeAsync(PreferredInputScheme scheme, CancellationToken cancellationToken = default)
    {
        if (!_preferredSchemeLookup.TryGetValue(scheme.UserId, out var schemes))
        {
            schemes = [];
        }

        _preferredSchemeLookup[scheme.UserId] = schemes.Where(s => !s.DefinitionName.Equals(scheme.DefinitionName, StringComparison.OrdinalIgnoreCase))
                    .Append(scheme)
                    .ToList();

        return Task.FromResult(Out.Success(scheme));
    }

    public Task<Output<IEnumerable<PreferredInputScheme>>> GetPreferredSchemesAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Out.Success(_preferredSchemeLookup.Values.SelectMany(v => v)));
    }

    public Task<Output> DeleteCustomSchemeAsync(string inputDefinitionId, string schemeName, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException($"Default scheme repository does not support custom schemes, please register a custom repository if this is desired.");
    }

    public Task<Output<IEnumerable<CustomInputScheme>>> GetCustomSchemesAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException($"Default scheme repository does not support custom schemes, please register a custom repository if this is desired.");
    }

    public Task<Output<CustomInputScheme>> SaveCustomInputScheme(CustomInputScheme scheme, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException($"Default scheme repository does not support custom schemes, please register a custom repository if this is desired.");
    }

    #endregion
}
