using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OSK.Inputs.Models.Configuration;
using OSK.Inputs.Models.Runtime;
using OSK.Inputs.Options;
using OSK.Inputs.Ports;

namespace OSK.Inputs.Internal;

internal class InputSystemListener(InputControllerConfiguration controller, IEnumerable<IInputSystem> inputSystems) 
    : IInputControllerListener
{
    #region IInputControllerListener

    public InputControllerConfiguration ControllerConfiguration => controller;

    public async Task<IEnumerable<ActivatedInput>> ReadInputsAsync(InputHandlerOptions options)
    {
        using var cancellationTokenSource = new CancellationTokenSource(options.ControllerReadTime);

        var activatedInputs = await inputSystems.ExecuteConcurrentResultAsync(
            inputReceiver => inputReceiver.ReadInputsAsync(cancellationTokenSource.Token),
            maxDegreesOfConcrruency: options.MaxConcurrentReceivers,
            isParallel: options.RunInputReceiversInParallel,
            cancellationToken: cancellationTokenSource.Token);

        return activatedInputs.SelectMany(inputs => inputs);
    }

    public void Dispose()
    {
        foreach (var inputReceiver in inputSystems)
        {
            inputReceiver.Dispose();
        }
    }

    #endregion
}
