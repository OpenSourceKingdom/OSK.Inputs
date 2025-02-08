using OSK.Inputs.Models.Configuration;
using OSK.Inputs.Models.Runtime;
using OSK.Inputs.Ports;

namespace OSK.Inputs.UnitTests._Helpers;
public class TestInputReceiver(InputReceiverConfiguration configuration) : IInputReceiver
{
    public void Dispose()
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<ActivatedInput>> ReadInputsAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
