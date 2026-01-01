using System;
using OSK.Inputs.Abstractions.Notifications;

namespace OSK.Inputs.Internal.Services;

internal class InputNotificationPublisher : IInputNotificationPublisher
{
    #region IInputNotificationPublisher

    public event Action<InputDeviceNotification> OnDeviceEvent = delegate { };
    public event Action<InputUserNotification> OnUserEvent = delegate { };

    public void Notify(IInputSystemNotification inputSystemEvent)
    {
        if (inputSystemEvent is null)
        {
            throw new ArgumentNullException(nameof(inputSystemEvent));
        }

        switch (inputSystemEvent)
        {
            case InputDeviceNotification deviceEvent:
                OnDeviceEvent(deviceEvent);
                break;
            case InputUserNotification userEvent:
                OnUserEvent(userEvent);
                break;
            default:
                throw new InvalidOperationException($"The notifier was not configured to publish an event of type '{inputSystemEvent.GetType().FullName}'.");
        }
    }

    #endregion
}
