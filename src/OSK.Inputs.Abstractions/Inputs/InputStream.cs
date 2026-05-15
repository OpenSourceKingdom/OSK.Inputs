namespace OSK.Inputs.Abstractions.Inputs;

public abstract class InputStream(InputDeviceType deviceType, InputStreamType streamType, int id)
    : DeviceInput(deviceType, id)
{
    public InputStreamType StreamType { get; } = streamType;
}
