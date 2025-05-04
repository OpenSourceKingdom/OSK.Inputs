using OSK.Inputs.Models.Inputs;
using OSK.Inputs.Models.Runtime;
using OSK.Inputs.Ports;

namespace OSK.Inputs.UnitTests._Helpers;
public class TestInputReader(InputReaderParameters parameters) : IInputDeviceReader
{
    public InputDeviceIdentifier ControllerIdentifier => parameters.DeviceIdentifier;
    public IEnumerable<IInput> Inputs => parameters.Inputs;

    public event Action<InputDeviceIdentifier> OnControllerDisconnected = _ => { };
    public event Action<InputDeviceIdentifier> OnControllerReconnected = _ => { };

    public void Dispose()
    {
    }

    public Task ReadInputsAsync(UserInputReadContext readContext, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public void TriggerConnectionEvent()
    {
        OnControllerReconnected.Invoke(ControllerIdentifier);
    }

    public void TriggerDisconnectedEvent()
    {
        OnControllerDisconnected.Invoke(ControllerIdentifier);
    }
}
