using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OSK.Functions.Outputs.Abstractions;
using OSK.Hexagonal.MetaData;
using OSK.Inputs.Models.Configuration;
using OSK.Inputs.Models.Events;
using OSK.Inputs.Models.Runtime;
using OSK.Inputs.Options;

namespace OSK.Inputs.Ports;

[HexagonalIntegration(HexagonalIntegrationType.LibraryProvided, HexagonalIntegrationType.ConsumerPointOfEntry)]
public interface IInputManager
{
    /// <summary>
    /// The <see cref="InputSystemConfiguration"/> that represents the input manager's input system. All validations, usage, etc. rely on this configuration
    /// </summary>
    InputSystemConfiguration Configuration { get; }

    /// <summary>
    /// Triggered when an input device was disconnected from the input system
    /// </summary>
    event Action<ApplicationUserInputDeviceEvent> OnInputDeviceDisconnected;

    /// <summary>
    /// Triggered when a previously connected input device reconnects to the input system after being disconnected.
    /// i.e. this should trigger only after an event for a device disconnection was sent, if the same device later reconnects.
    /// </summary>
    event Action<ApplicationUserInputDeviceEvent> OnInputDeviceReconnected;
    
    /// <summary>
    /// Triggered when a new input device was added to the input system
    /// </summary>
    event Action<ApplicationUserInputDeviceEvent> OnInputDeviceAdded;

    /// <summary>
    /// Adds a new user to the input manager to monitor inputs for during reads.
    /// </summary>
    /// <param name="userId">The application's user id that is associated to the user that is being managed</param>
    /// <param name="options">The <see cref="JoinUserOptions"/> allows for extra configuration on a joining user</param>
    /// <param name="cancellationToken">The token to cancel the request</param>
    /// <returns>The <see cref="IApplicationInputUser"/> that is managed by the input manager</returns>
    Task<IOutput<IApplicationInputUser>> JoinUserAsync(int userId, JoinUserOptions options, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Removes a previously added user
    /// </summary>
    /// <param name="userId">The user id associated to the application</param>
    void RemoveUser(int userId);

    /// <summary>
    /// Pairs a device from the input system to the given user
    /// </summary>
    /// <param name="userId">The user id to associate the <see cref="InputDevice"/> to</param>
    /// <param name="deviceIdentifier">An <see cref="InputDeviceIdentifier"/> that is associated to a device on the input system</param>
    void PairDevice(int userId, InputDeviceIdentifier deviceIdentifier);

    /// <summary>
    /// Unpairs a device from the input system and associated user
    /// </summary>
    /// <param name="deviceIdentifier">The device identifier that will be unpaired</param>
    void UnpairDevice(InputDeviceIdentifier deviceIdentifier);

    /// <summary>
    /// Gets all the users that have been joined to the input manager
    /// </summary>
    /// <returns>A collection of the input users attached to the input manager</returns>
    IEnumerable<IApplicationInputUser> GetApplicationInputUsers();

    /// <summary>
    /// Gets an individual <see cref="IApplicationInputUser"/>
    /// </summary>
    /// <param name="userId">The user id for the user</param>
    /// <returns></returns>
    IApplicationInputUser GetApplicationInputUser(int userId);

    /// <summary>
    /// Gets the list of input definitions available to the <see cref="InputDefinition"/>
    /// 
    /// Note: this method is a task due to the potential of custom <see cref="InputScheme"/>s that could be added to the input definition via hard drive or some other external location.
    /// </summary>
    /// <param name="cancellationToken">The token to cancel the request</param>
    /// <returns>The list of inputs, if successful</returns>
    ValueTask<IOutput<IEnumerable<InputDefinition>>> GetInputDefinitionsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets the currently active input definition for the specified user. i.e. this is the definition that will handle triggering the input actions based on user inputs
    /// 
    /// Note: this method is a task due to the potential of custom <see cref="InputScheme"/>s that could be added to the input definition via hard drive or some other external location.
    /// </summary>
    /// <param name="userId">The user id being updated</param>
    /// <param name="inputDefinitionName">The input definition the user will start using</param>
    /// <param name="cancellationToken">The token to cancel the request</param>
    /// <returns></returns>
    Task<IOutput> SetUserActiveInputDefinitionAsync(int userId, string inputDefinitionName, CancellationToken cancellationToken = default);
    Task<IOutput> ResetUserActiveInputSchemeAsync(int userId, string inputDefinitionId, string controllerId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates the user's active input scheme
    /// </summary>
    /// <param name="userId">The user id for the user</param>
    /// <param name="inputDefinitionId">The input definition the scheme belongs to</param>
    /// <param name="controllerId">The controller the scheme belongs to</param>
    /// <param name="schemeId">the id for the scheme</param>
    /// <param name="cancellationToken">a token to cancel the request</param>
    /// <returns>An <see cref="ActiveInputScheme"/> if successful</returns>
    Task<IOutput<ActiveInputScheme>> SetActiveInputSchemeAsync(int userId, string inputDefinitionId, string controllerId, string schemeId,
        CancellationToken cancellationToken = default);
    Task<IOutput<InputScheme>> SaveCustomInputSchemeAsync(InputScheme inputScheme, CancellationToken cancellationToken = default);
    Task<IOutput> DeleteCustomInputSchemeAsync(string inputDefinitionName, string controllerId, string schemeName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Attempts to reconfigure the input manager with a new configuration
    /// </summary>
    /// <param name="configuration">The configuration action to apply to a new input configuration that the input manager will use/</param>
    /// <returns>An output that represents the outcome of the configuration attempt</returns>
    IOutput Reconfigure(Action<InputManagerRuntimeConfigurator> configuration);

    Task<InputActivationContext> ReadInputsAsync(InputReadOptions readOptions, CancellationToken cancellationToken = default);
}