using System;
using OSK.Hexagonal.MetaData;
using OSK.Inputs.Options;

namespace OSK.Inputs.Ports;

[HexagonalIntegration(HexagonalIntegrationType.LibraryProvided)]
public interface IInputBuilder
{
    IInputBuilder AddInputDefinition(string name, Action<IInputDefinitionBuilder> buildAction);

    IInputBuilder WithHandlerOptions(Action<InputHandlerOptions> optionConfiguration);

    IInputBuilder UseInputSchemeRepository<TRepository>()
        where TRepository : IInputSchemeRepository;
}
