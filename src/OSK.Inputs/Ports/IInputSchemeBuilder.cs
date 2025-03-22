using OSK.Hexagonal.MetaData;
using OSK.Inputs.Models.Inputs;

namespace OSK.Inputs.Ports;

[HexagonalIntegration(HexagonalIntegrationType.LibraryProvided)]
public interface IInputSchemeBuilder
{
    IInputSchemeBuilder AssignInput(IInput input, InputPhase inputPhase, string actionKey);

    IInputSchemeBuilder MakeDefault();
}
