using OSK.Hexagonal.MetaData;
using OSK.Inputs.Abstractions.Configuration;

namespace OSK.Inputs.Abstractions;

[HexagonalIntegration(HexagonalIntegrationType.ConsumerRequired)]
public interface IInputSystemConfigurationSource
{
    InputSystemConfiguration GetConfiguration();
}
