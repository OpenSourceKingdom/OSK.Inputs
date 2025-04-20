using System;
using System.Collections.Generic;
using System.Text;
using OSK.Inputs.Models.Configuration;

namespace OSK.Inputs.Internal.Services;
internal class RuntimeInputController(InputControllerConfiguration configuration, InputScheme inputScheme,
    IEnumerable<RuntimeInputDevice> inputDevices)
{
    #region Variables

    public string ControllerId => configuration.ControllerName;

    public InputScheme InputScheme => inputScheme;

    public InputControllerConfiguration Configuration => configuration;

    public IEnumerable<RuntimeInputDevice> InputDevices => inputDevices;

    #endregion
}
