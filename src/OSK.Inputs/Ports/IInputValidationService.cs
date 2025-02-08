using OSK.Hexagonal.MetaData;
using OSK.Inputs.Models;
using OSK.Inputs.Models.Configuration;

namespace OSK.Inputs.Ports;

[HexagonalIntegration(HexagonalIntegrationType.LibraryProvided)]
public interface IInputValidationService
{
    InputValidationContext ValidateInputDefinition(InputDefinition inputDefinition);

    InputValidationContext ValidateInputScheme(InputDefinition inputDefinition, InputScheme inputScheme);
}
