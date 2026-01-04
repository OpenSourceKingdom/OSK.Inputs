using OSK.Inputs.Abstractions.Configuration;
using OSK.Inputs.Models;

namespace OSK.Inputs.Ports;

public interface IInputSystemConfigurationValidator
{
    InputConfigurationValidationResult Validate(InputSystemConfiguration configuration);

    InputConfigurationValidationResult ValidateCustomScheme(InputSystemConfiguration configuration, CustomInputScheme scheme, bool allowDuplicateCustomScheme);
}
