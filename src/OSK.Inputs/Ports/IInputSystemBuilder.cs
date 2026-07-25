using OSK.Hexagonal.MetaData;
using OSK.Inputs.Abstractions;

namespace OSK.Inputs.Ports;

/// <summary>
/// A builder that is used to configure the input system
/// </summary>
[HexagonalIntegration(HexagonalIntegrationType.LibraryProvided)]
public interface IInputSystemBuilder
{
    /// <summary>
    /// Specifies the specific input scheme repository to use 
    /// </summary>
    /// <typeparam name="TSchemeRepository">The input scheme repository that is desired</typeparam>
    /// <returns>The builder for chaining</returns>
    IInputSystemBuilder UseSchemeRepository<TSchemeRepository>()
        where TSchemeRepository: class, IInputSchemeRepository;
}
