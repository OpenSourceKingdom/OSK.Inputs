using System.Collections.Generic;
using System.Linq;

namespace OSK.Inputs.Models.Configuration;
public class InputControllerConfiguration(string controllerName, IEnumerable<InputDeviceName> deviceNames)
{
    #region Variables

    public string ControllerName => controllerName;

    public IReadOnlyCollection<InputDeviceName> DeviceNames { get; } = deviceNames.ToArray();

    #endregion
}
