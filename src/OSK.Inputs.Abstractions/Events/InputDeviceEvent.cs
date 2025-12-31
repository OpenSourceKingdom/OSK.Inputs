namespace OSK.Inputs.Abstractions.Events;

public abstract class InputDeviceEvent(RuntimeDeviceIdentifier deviceIdentifier): IInputSystemEvent
{
    public RuntimeDeviceIdentifier DeviceIdentifier => deviceIdentifier;
}
