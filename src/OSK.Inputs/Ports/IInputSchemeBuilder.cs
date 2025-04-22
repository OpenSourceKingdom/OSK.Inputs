using System;
using OSK.Hexagonal.MetaData;
using OSK.Inputs.Models.Configuration;
using OSK.Inputs.Models.Inputs;

namespace OSK.Inputs.Ports;

[HexagonalIntegration(HexagonalIntegrationType.LibraryProvided)]
public interface IInputSchemeBuilder
{
    IInputSchemeBuilder AddDevice<TInput>(InputDeviceName deviceName, Action<IInputDeviceActionBuilder<TInput>> builder)
        where TInput: IInput;

    IInputSchemeBuilder MakeDefault();
}
