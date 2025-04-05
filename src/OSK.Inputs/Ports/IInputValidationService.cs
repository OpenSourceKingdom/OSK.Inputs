using OSK.Hexagonal.MetaData;
using OSK.Inputs.Models.Configuration;

namespace OSK.Inputs.Ports;

[HexagonalIntegration(HexagonalIntegrationType.LibraryProvided)]
public interface IInputValidationService
{
    InputValidationContext ValidateInputSystemConfiguration(InputSystemConfiguration inputSystemConfiguration);

    InputValidationContext ValidateCustomInputScheme(InputSystemConfiguration inputSystemConfiguration, InputScheme inputScheme);
}
