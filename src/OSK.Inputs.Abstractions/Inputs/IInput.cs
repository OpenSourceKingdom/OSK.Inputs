using OSK.Inputs.Abstractions.Devices;

namespace OSK.Inputs.Abstractions.Inputs;

public interface IInput
{
    InputDeviceType DeviceType { get; }
}
