using System;
using OSK.Hexagonal.MetaData;
using OSK.Inputs.Models.Configuration;

namespace OSK.Inputs.Ports;

[HexagonalIntegration(HexagonalIntegrationType.LibraryProvided)]
public interface IInputControllerBuilder
{
    IInputControllerBuilder AddInputReceiver(IInputReceiverDescriptor descriptor);

    IInputControllerBuilder AddInputScheme(string name, Action<IInputSchemeBuilder> buildConfiguration);
}
