using System;
using OSK.Hexagonal.MetaData;
using OSK.Inputs.Abstractions.Notifications;

namespace OSK.Inputs.Ports;

/// <summary>
/// A notifier that transmits notifications relating to various device, user, or other input system events
/// </summary>
[HexagonalIntegration(HexagonalIntegrationType.LibraryProvided)]
public interface IInputSystemNotifier
{
    /// <summary>
    /// Action event for device notifications
    /// </summary>
    event Action<InputDeviceNotification> OnDeviceNotification;

    /// <summary>
    /// Action event for input user notifications
    /// </summary>
    event Action<InputUserNotification> OnUserNotification;

    /// <summary>
    /// Action event for input system notifications
    /// </summary>
    event Action<InputSystemNotification> OnSystemNotification;
}
