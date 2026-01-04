using System;
using System.Threading;
using System.Threading.Tasks;
using OSK.Functions.Outputs.Abstractions;
using OSK.Hexagonal.MetaData;
using OSK.Inputs.Abstractions.Configuration;
using OSK.Inputs.Models;

namespace OSK.Inputs.Ports;

/// <summary>
/// The core input system that consumers will use when wanting to manage and handle input
/// </summary>
[HexagonalIntegration(HexagonalIntegrationType.LibraryProvided, HexagonalIntegrationType.ConsumerPointOfEntry)]
public interface IInputSystem
{
    /// <summary>
    /// The current input system configuration used by the system
    /// </summary>
    InputSystemConfiguration Configuration { get; }

    /// <summary>
    /// The notifier that transmits notifications events
    /// </summary>
    IInputSystemNotifier Notifier { get; }

    /// <summary>
    /// The user manager the input system is using
    /// </summary>
    IInputUserManager UserManager { get; }

    /// <summary>
    /// Describes if the input system is capable of handling custom input schemes or not
    /// </summary>
    bool AllowCustomSchemes { get; }

    /// <summary>
    /// Describes if input processing has currently been paused
    /// </summary>
    bool PausedInput { get; }

    /// <summary>
    /// Toggle the input processing state
    /// </summary>
    /// <param name="pause">Whether input processing should enabled or disabled</param>
    void ToggleInputProcessing(bool pause);

    /// <summary>
    /// Initializes the input system
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation</param>
    /// <returns>A task for the operation</returns>
    Task InitializeAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Attempts to delete the specified custom scheme from the input system
    /// </summary>
    /// <param name="definitionName">The name of the definition</param>
    /// <param name="schemeName">The scheme to delete from the definition</param>
    /// <param name="cancellationToken">A token to cancel the operation</param>
    /// <returns>An output describing the result of the operation</returns>
    Task<IOutput> DeleteCustomSchemeAsync(string definitionName, string schemeName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Attempts to save the custom scheme to the inpt system
    /// </summary>
    /// <param name="scheme">The custom scheme to save</param>
    /// <param name="saveFlags">The various flag options that will impact the save operation</param>
    /// <param name="cancellationToken">A token to cancel the operation</param>
    /// <returns>An output describing the result of the operation</returns>
    Task<IOutput> SaveCustomSchemeAsync(CustomInputScheme scheme, SchemeSaveFlags saveFlags, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates the input system using the specified delta time
    /// </summary>
    /// <remarks>This process method may be ignored by the input system if the input system is pausing input</remarks>
    /// <param name="deltaTime">the time that has passed since the last update</param>
    void Update(TimeSpan deltaTime);
}
