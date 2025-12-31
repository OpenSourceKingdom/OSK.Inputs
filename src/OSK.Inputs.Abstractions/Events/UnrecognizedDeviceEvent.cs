namespace OSK.Inputs.Abstractions.Events;

public class UnrecognizedDeviceEvent(RuntimeDeviceIdentifier device): InputDeviceEvent(device)
{
}
