using OSK.Inputs.Models.Configuration;
using OSK.Inputs.Models.Runtime;
using OSK.Inputs.Ports;

namespace OSK.Inputs.UnitTests._Helpers;
public class TestInputReader(InputReaderParameters parameters) : IInputReader
{
    public InputControllerIdentifier ControllerIdentifier => parameters.ControllerIdentifier;
    public IEnumerable<InputConfiguration> InputConfigurations => parameters.InputCnfigurations;

    public event Action<InputControllerIdentifier> OnControllerDisconnected = _ => { };
    public event Action<InputControllerIdentifier> OnControllerReconnected = _ => { };

    public void Dispose()
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<ActivatedInput>> ReadInputsAsync(InputReadContext readContext, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
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
