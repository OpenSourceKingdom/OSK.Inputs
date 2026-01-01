using System;
using OSK.Hexagonal.MetaData;
using OSK.Inputs.Abstractions.Notifications;

namespace OSK.Inputs.Ports;

[HexagonalIntegration(HexagonalIntegrationType.LibraryProvided)]
public interface IInputSystemNotifier
{
    event Action<InputDeviceNotification> OnDeviceNotification;
    event Action<InputUserNotification> OnUserNotification;
    event Action<InputSystemNotification> OnSystemNotification;
}
