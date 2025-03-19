using System;
using OSK.Hexagonal.MetaData;
using OSK.Inputs.Models.Configuration;

namespace OSK.Inputs.Ports;

[HexagonalIntegration(HexagonalIntegrationType.LibraryProvided)]
public interface IInputDefinitionBuilder
{
    IInputDefinitionBuilder AddAction(InputAction action);

    IInputDefinitionBuilder AddInputScheme(string controllerName, string schemeName, Action<IInputSchemeBuilder> buildAction);

    InputDefinition Build();
}
