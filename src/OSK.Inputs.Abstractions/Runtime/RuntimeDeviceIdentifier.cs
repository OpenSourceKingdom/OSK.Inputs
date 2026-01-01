using OSK.Inputs.Abstractions.Configuration;

namespace OSK.Inputs.Abstractions.Runtime;

public readonly record struct RuntimeDeviceIdentifier(int DeviceId, InputDeviceIdentity Identity)
{
}
