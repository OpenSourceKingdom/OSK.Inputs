using System;
using OSK.Inputs.Abstractions.Notifications;

namespace OSK.Inputs.Internal.Services;

internal class InputNotificationPublisher : IInputNotificationPublisher
{
    #region IInputNotificationPublisher

    public event Action<InputDeviceNotification> OnDeviceNotification = delegate { };
    public event Action<InputUserNotification> OnUserNotification = delegate { };
    public event Action<InputSystemNotification> OnSystemNotification = delegate { };

    public void Notify(IInputNotification notification)
    {
        if (notification is null)
        {
            throw new ArgumentNullException(nameof(notification));
        }

        switch (notification)
        {
            case InputDeviceNotification deviceNotification:
                OnDeviceNotification(deviceNotification);
                break;
            case InputUserNotification userNotification:
                OnUserNotification(userNotification);
                break;            
            case InputSystemNotification systemNotification:
                OnSystemNotification(systemNotification);
                break;
            default:
                throw new InvalidOperationException($"The notifier was not configured to publish an event of type '{notification.GetType().FullName}'.");
        }
    }

    #endregion
}
