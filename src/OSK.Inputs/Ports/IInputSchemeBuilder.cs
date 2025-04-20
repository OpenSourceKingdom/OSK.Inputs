using System;
using OSK.Hexagonal.MetaData;
using OSK.Inputs.Models.Configuration;
using OSK.Inputs.Models.Inputs;

namespace OSK.Inputs.Ports;

[HexagonalIntegration(HexagonalIntegrationType.LibraryProvided)]
public interface IInputSchemeBuilder
{
    IInputSchemeBuilder AddDevice(InputDeviceName deviceName, Action<IInputDeviceActionBuilder> builder);

    IInputSchemeBuilder MakeDefault();
}
