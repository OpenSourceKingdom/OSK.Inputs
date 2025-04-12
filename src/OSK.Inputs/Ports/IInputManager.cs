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
    InputSystemConfiguration Configuration { get; }

    event Action<ApplicationUserInputControllerEvent> OnInputControllerDisconnected;
    event Action<ApplicationUserInputControllerEvent> OnInputControllerConnected;
    event Action<ApplicationUserInputControllerEvent> OnInputControllerAdded;

    Task<IOutput<IApplicationInputUser>> JoinUserAsync(int userId, JoinUserOptions options, CancellationToken cancellationToken = default);
    void RemoveUser(int userId);
    void PairController(int userId, InputControllerIdentifier controllerIdentifier);
    IEnumerable<IApplicationInputUser> GetApplicationInputUsers();
    IApplicationInputUser GetApplicationInputUser(int userId);


    ValueTask<IOutput<IEnumerable<InputDefinition>>> GetInputDefinitionsAsync(CancellationToken cancellationToken = default);

    Task<IOutput> SetUserActiveInputDefinitionAsync(int userId, string inputDefinitionName, CancellationToken cancellationToken = default);
    Task<IOutput> ResetUserActiveInputSchemeAsync(int userId, string inputDefinitionId, InputDeviceName deviceName, CancellationToken cancellationToken = default);
    Task<IOutput<ActiveInputScheme>> SetActiveInputSchemeAsync(int userId, string inputDefinitionId, InputDeviceName deviceName, string schemeId,
        CancellationToken cancellationToken = default);
    Task<IOutput<InputScheme>> SaveCustomInputSchemeAsync(InputScheme inputScheme, CancellationToken cancellationToken = default);
    Task<IOutput> DeleteCustomInputSchemeAsync(string inputDefinitionName, InputDeviceName deviceName, string schemeName,
        CancellationToken cancellationToken = default);

    Task<InputActivationContext> ReadInputsAsync(InputReadOptions readOptions, CancellationToken cancellationToken = default);
}