namespace OSK.Inputs.Abstractions.Inputs;

public class PointerInputStream(InputDeviceType deviceType, int id)
    : InputStream(deviceType, InputStreamType.Pointer, id)
{
}
