using OSK.Hexagonal.MetaData;
using OSK.Inputs.Abstractions.Configuration;

namespace OSK.Inputs.Abstractions;

/// <summary>
/// A configuration source provides the <see cref="InputSystemConfiguration"/> that is available for the input system to use.
/// </summary>
[HexagonalIntegration(HexagonalIntegrationType.ConsumerRequired)]
public interface IInputSystemConfigurationSource
{
    /// <summary>
    /// Returns an input sytem configuration.
    /// 
    /// <br />
    /// Note: This configuration is not guaranteed to be fully usable, it must be validated prior to usage
    /// </summary>
    /// <returns><see cref="InputSystemConfiguration"/></returns>
    InputSystemConfiguration GetConfiguration();
}
