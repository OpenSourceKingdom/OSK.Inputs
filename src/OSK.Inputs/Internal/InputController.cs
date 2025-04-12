using System;
using OSK.Inputs.Models.Configuration;
using OSK.Inputs.Models.Runtime;
using OSK.Inputs.Ports;

namespace OSK.Inputs.Internal;
internal class InputController(InputControllerIdentifier controllerIdentifier, IInputDeviceConfiguration configuration, IInputReader inputReader) : IDisposable
{
    #region Variables

    public InputControllerIdentifier ControllerIdentifier => controllerIdentifier;

    public IInputDeviceConfiguration Configuration => configuration;

    public IInputReader InputReader => inputReader;

    #endregion

    #region IDisposable

    public void Dispose()
    {
        inputReader.Dispose();
    }

    #endregion
}
