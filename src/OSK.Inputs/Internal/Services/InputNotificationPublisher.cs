using System;
using OSK.Inputs.Abstractions.Events;

namespace OSK.Inputs.Internal.Services;

internal class InputNotificationPublisher : IInputNotificationPublisher
{
    #region IInputNotificationPublisher

    public event Action<InputDeviceEvent> OnDeviceEvent = delegate { };
    public event Action<InputUserEvent> OnUserEvent = delegate { };

    public void Notify(IInputSystemEvent inputSystemEvent)
    {
        if (inputSystemEvent is null)
        {
            throw new ArgumentNullException(nameof(inputSystemEvent));
        }

        switch (inputSystemEvent)
        {
            case InputDeviceEvent deviceEvent:
                OnDeviceEvent(deviceEvent);
                break;
            case InputUserEvent userEvent:
                OnUserEvent(userEvent);
                break;
            default:
                throw new InvalidOperationException($"The notifier was not configured to publish an event of type '{inputSystemEvent.GetType().FullName}'.");
        }
    }

    #endregion
}
