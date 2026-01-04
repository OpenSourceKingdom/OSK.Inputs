using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OSK.Functions.Outputs.Abstractions;
using OSK.Hexagonal.MetaData;
using OSK.Inputs.Abstractions.Configuration;
using OSK.Inputs.Abstractions.Runtime;

namespace OSK.Inputs.Abstractions;

/// <summary>
/// A repository that is able to store and retrieve persistent data related to user schemes for preferences and custom schemes.
/// </summary>
[HexagonalIntegration(HexagonalIntegrationType.ConsumerOptional, HexagonalIntegrationType.LibraryProvided)]
public interface IInputSchemeRepository
{
    /// <summary>
    /// Informs the Input System whether attempts to save custom schemes by users can be made
    /// </summary>
    bool AllowCustomSchemes { get; }

    /// <summary>
    /// Saves a user's preferred scheme to persistent storage
    /// </summary>
    /// <param name="scheme">The <see cref="PreferredInputScheme"/> for the user</param>
    /// <param name="cancellationToken">A token to cancel the operation</param>
    /// <returns>An output that contains the <see cref="PreferredInputScheme"/> if successful</returns>
    Task<IOutput<PreferredInputScheme>> SavePreferredSchemeAsync(PreferredInputScheme scheme, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the entire list of saved preferred schemes on the device for all users.
    /// </summary>
    /// <param name="cancellationToken">The token to cancel the operation</param>
    /// <returns>An output with the list of <see cref="PreferredInputScheme"/>s on the device</returns>
    Task<IOutput<IEnumerable<PreferredInputScheme>>> GetPreferredSchemesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves a <see cref="CustomInputScheme"/> that can be used by the input system
    /// </summary>
    /// <param name="scheme">The custom scheme to save</param>
    /// <param name="cancellationToken">A token to cancel the operation</param>
    /// <returns>An output with the <see cref="CustomInputScheme"/> if successful</returns>
    Task<IOutput<CustomInputScheme>> SaveCustomInputScheme(CustomInputScheme scheme, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a previously saved <see cref="CustomInputScheme"/> from the device.
    /// </summary>
    /// <param name="definitionName">The definition the scheme is associated to</param>
    /// <param name="schemeName">The name of the custom scheme</param>
    /// <param name="cancellationToken">A token to cancel the operation</param>
    /// <returns>An output that describes the result of the operation</returns>
    Task<IOutput> DeleteCustomSchemeAsync(string definitionName, string schemeName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the entire list of <see cref="CustomInputScheme"/>s that are saved on the device for the input system to use
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation</param>
    /// <returns>An output with the list of <see cref="CustomInputScheme"/>s if successful</returns>
    Task<IOutput<IEnumerable<CustomInputScheme>>> GetCustomSchemesAsync(CancellationToken cancellationToken = default);
}
