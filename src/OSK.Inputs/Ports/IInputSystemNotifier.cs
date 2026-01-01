using System;
using OSK.Hexagonal.MetaData;
using OSK.Inputs.Abstractions.Notifications;

namespace OSK.Inputs.Ports;

[HexagonalIntegration(HexagonalIntegrationType.LibraryProvided)]
public interface IInputSystemNotifier
{
    event Action<InputDeviceNotification> OnDeviceEvent;
    event Action<InputUserNotification> OnUserEvent;
}
