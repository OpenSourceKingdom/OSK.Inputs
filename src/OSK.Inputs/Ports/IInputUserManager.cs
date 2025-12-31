using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OSK.Functions.Outputs.Abstractions;
using OSK.Hexagonal.MetaData;
using OSK.Inputs.Abstractions;
using OSK.Inputs.Options;

namespace OSK.Inputs.Ports;

[HexagonalIntegration(HexagonalIntegrationType.LibraryProvided)]
public interface IInputUserManager
{
    IOutput<IInputUser> CreateUser(UserJoinOptions joinOptions);

    void RemoveUser(int userId);

    IInputUser? GetUser(int userId);

    IEnumerable<IInputUser> GetUsers();

    IInputUser? GetInputUserForDevice(int deviceId);

    IOutput PairDevice(int userId, RuntimeDeviceIdentifier device);

    void UnpairDevice(int userId, int deviceId);

    void SetActiveDefinition(int userId, string definitionName);

    Task<IOutput> SavePreferredSchemeAsync(PreferredInputScheme scheme, CancellationToken cancellationToken = default);

    Task<IOutput> LoadUserConfigurationAsync(CancellationToken cancellationToken = default);
}
