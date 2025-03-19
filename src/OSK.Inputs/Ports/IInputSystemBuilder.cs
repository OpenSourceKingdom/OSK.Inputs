using System;
using OSK.Hexagonal.MetaData;
using OSK.Inputs.Models.Configuration;

namespace OSK.Inputs.Ports;

[HexagonalIntegration(HexagonalIntegrationType.LibraryProvided)]
public interface IInputSystemBuilder
{
    IInputSystemBuilder AddInputDefinition(string name, Action<IInputDefinitionBuilder> buildAction);

    IInputSystemBuilder AddInputController(IInputControllerConfiguration configuration);

    IInputSystemBuilder WithMaxLocalUsers(int maxLocalUsers);

    IInputSystemBuilder AllowCustomSchemes();

    IInputSystemBuilder UseInputSchemeRepository<TRepository>()
        where TRepository : IInputSchemeRepository;
}
