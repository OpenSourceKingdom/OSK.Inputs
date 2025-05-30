﻿using OSK.Hexagonal.MetaData;
using OSK.Inputs.Models.Configuration;
using OSK.Inputs.Models.Runtime;

namespace OSK.Inputs.Ports;

[HexagonalIntegration(HexagonalIntegrationType.LibraryProvided, HexagonalIntegrationType.IntegrationOptional)]
public interface IInputReaderProvider
{
    IInputDeviceReader GetInputReader(IInputDeviceConfiguration deviceConfiguration, InputDeviceIdentifier deviceIdentifier);
}
