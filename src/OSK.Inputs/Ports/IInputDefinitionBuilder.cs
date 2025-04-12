using System;
using OSK.Hexagonal.MetaData;
using OSK.Inputs.Models.Configuration;

namespace OSK.Inputs.Ports;

[HexagonalIntegration(HexagonalIntegrationType.LibraryProvided)]
public interface IInputDefinitionBuilder
{
    IInputDefinitionBuilder AddAction(InputAction action);

    IInputDefinitionBuilder AddInputScheme(InputDeviceName deviceName, string schemeName, Action<IInputSchemeBuilder> buildAction);

    InputDefinition Build();
}
