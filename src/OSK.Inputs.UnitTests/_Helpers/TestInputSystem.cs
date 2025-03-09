using OSK.Inputs.Models.Configuration;
using OSK.Inputs.Models.Runtime;
using OSK.Inputs.Ports;

namespace OSK.Inputs.UnitTests._Helpers;
public class TestInputSystem(PlayerInputSystemConfiguration playerInputSystemConfiguration) : IInputSystem
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
