using OSK.Inputs.Abstractions.Events;
using OSK.Inputs.Ports;

namespace OSK.Inputs.Internal;

internal interface IInputNotificationPublisher: IInputSystemNotifier
{
    void Notify(IInputSystemEvent inputSystemEvent);
}
