using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OSK.Inputs.Models.Configuration;
using OSK.Inputs.Models.Runtime;
using OSK.Inputs.Ports;

namespace OSK.Inputs.Internal;
internal class RuntimeInputDevice(int userId, InputDeviceIdentifier deviceIdentifier, IInputDeviceConfiguration configuration,
    IInputReader inputReader) : IDisposable
{
    #region Variables

    public InputDeviceIdentifier DeviceIdentifier => deviceIdentifier;

    public IInputDeviceConfiguration Configuration => configuration;

    public IInputReader InputReader => inputReader;

    private readonly UserInputReadContext _readContext = new UserInputReadContext(userId, deviceIdentifier.DeviceName);

    #endregion

    #region Public

    public async Task<IEnumerable<ActivatedInput>> ReadInputsAsync(CancellationToken cancellationToken)
    {
        _readContext.PrepareForNextRead();
        await InputReader.ReadInputsAsync(_readContext, cancellationToken).ConfigureAwait(false);

        return _readContext.GetActivatedInputs();
    }

    public void SetInputScheme(IEnumerable<InputActionMapPair> actionMapPairs) 
    {
        _readContext.InputActionPairs = actionMapPairs;
    }

    public void Dispose()
    {
        inputReader.Dispose();
    }

    #endregion
}
