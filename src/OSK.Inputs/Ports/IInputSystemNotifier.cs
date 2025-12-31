using System;
using OSK.Hexagonal.MetaData;
using OSK.Inputs.Abstractions.Events;

namespace OSK.Inputs.Ports;

[HexagonalIntegration(HexagonalIntegrationType.LibraryProvided)]
public interface IInputSystemNotifier
{
    event Action<InputDeviceEvent> OnDeviceEvent;
    event Action<InputUserEvent> OnUserEvent;
}
