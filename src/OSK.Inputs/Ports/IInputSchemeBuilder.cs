using OSK.Hexagonal.MetaData;
using OSK.Inputs.Models.Inputs;

namespace OSK.Inputs.Ports;

[HexagonalIntegration(HexagonalIntegrationType.LibraryProvided)]
public interface IInputSchemeBuilder
{
    IInputSchemeBuilder AssignInput(string receiverName, string actionKey, IInput input, InputPhase inputPhase);

    IInputSchemeBuilder MakeDefault();
}
