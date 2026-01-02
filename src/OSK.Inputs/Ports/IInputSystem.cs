using System;
using System.Threading;
using System.Threading.Tasks;
using OSK.Functions.Outputs.Abstractions;
using OSK.Hexagonal.MetaData;
using OSK.Inputs.Abstractions.Configuration;
using OSK.Inputs.Models;

namespace OSK.Inputs.Ports;

[HexagonalIntegration(HexagonalIntegrationType.LibraryProvided, HexagonalIntegrationType.ConsumerPointOfEntry)]
public interface IInputSystem
{
    InputSystemConfiguration Configuration { get; }

    IInputSystemNotifier Notifier { get; }

    IInputUserManager UserManager { get; }

    bool AllowCustomSchemes { get; }

    bool PausedInput { get; }

    void ToggleInputProcessing(bool pause);

    Task InitializeAsync(CancellationToken cancellationToken = default);

    Task<IOutput> DeleteCustomSchemeAsync(string definitionName, string schemeName, CancellationToken cancellationToken = default);

    Task<IOutput> SaveCustomSchemeAsync(CustomInputScheme scheme, SchemeSaveFlags saveFlags, CancellationToken cancellationToken = default);

    void Update(TimeSpan deltaTime);
}
