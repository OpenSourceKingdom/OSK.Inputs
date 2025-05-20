using OSK.Inputs.Models.Inputs;
using OSK.Inputs.Models.Runtime;
using OSK.Inputs.Ports;

namespace OSK.Inputs.UnitTests._Helpers;
public class TestInputReader(InputReaderParameters parameters) : IInputDeviceReader
{
    public InputDeviceIdentifier ControllerIdentifier => parameters.DeviceIdentifier;
    public IEnumerable<IInput> Inputs => parameters.Inputs;

    public event Action<InputDeviceIdentifier> OnDeviceConnected = _ => { };
    public event Action<InputDeviceIdentifier> OnDeviceDisconnected = _ => { };

    public void Dispose()
    {
    }

    public ValueTask ReadInputAsync(UserInputReadContext readContext, int inputId, CancellationToken cancellationToken = default)
    {
        return ValueTask.CompletedTask;
    }

    public void TriggerConnectionEvent()
    {
        OnDeviceConnected.Invoke(ControllerIdentifier);
    }

    public void TriggerDisconnectedEvent()
    {
        OnDeviceDisconnected.Invoke(ControllerIdentifier);
    }
}
