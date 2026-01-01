using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OSK.Functions.Outputs.Abstractions;
using OSK.Hexagonal.MetaData;
using OSK.Inputs.Abstractions.Configuration;
using OSK.Inputs.Abstractions.Runtime;

namespace OSK.Inputs.Abstractions;

[HexagonalIntegration(HexagonalIntegrationType.ConsumerOptional)]
public interface IInputSchemeRepository
{
    bool AllowCustomSchemes { get; }

    Task<IOutput<PreferredInputScheme>> SavePreferredSchemeAsync(PreferredInputScheme scheme, CancellationToken cancellationToken = default);

    Task<IOutput<IEnumerable<PreferredInputScheme>>> GetPreferredSchemesAsync(CancellationToken cancellationToken = default);

    Task<IOutput<CustomInputScheme>> SaveCustomInputScheme(CustomInputScheme scheme, CancellationToken cancellationToken = default);

    Task<IOutput> DeleteCustomSchemeAsync(string definitionName, string schemeName, CancellationToken cancellationToken = default);

    Task<IOutput<IEnumerable<CustomInputScheme>>> GetCustomSchemesAsync(CancellationToken cancellationToken = default);
}
