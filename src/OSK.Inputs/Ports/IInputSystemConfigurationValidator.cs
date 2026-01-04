using OSK.Inputs.Abstractions.Configuration;
using OSK.Inputs.Models;

namespace OSK.Inputs.Ports;

/// <summary>
/// A validator for <see cref="InputSystemConfiguration"/>s
/// </summary>
public interface IInputSystemConfigurationValidator
{
    /// <summary>
    /// Validates an input system configuration, returning validation information
    /// </summary>
    /// <param name="configuration">The configurationt to validate</param>
    /// <returns>A result containing validation information</returns>
    InputConfigurationValidationResult Validate(InputSystemConfiguration configuration);

    /// <summary>
    /// Validates the custom input scheme against the input system configuration
    /// </summary>
    /// <param name="configuration">The input system configuration to validate against</param>
    /// <param name="scheme">The custom input scheme being validated</param>
    /// <param name="allowDuplicateCustomScheme">Whether a duplicate custom scheme should be allowed or not when validating</param>
    /// <returns>A result containing validation information</returns>
    InputConfigurationValidationResult ValidateCustomScheme(InputSystemConfiguration configuration, CustomInputScheme scheme, bool allowDuplicateCustomScheme);
}
