using OSK.Inputs.Ports;
using OSK.Inputs.Abstractions.Notifications;

namespace OSK.Inputs.Internal;

internal interface IInputNotificationPublisher: IInputSystemNotifier
{
    void Notify(IInputNotification inputNotification);
}
