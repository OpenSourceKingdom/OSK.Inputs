using System.Collections.Generic;
using System.Linq;

namespace OSK.Inputs.Models.Configuration;
public class InputDeviceActionMap(InputDeviceName deviceName, IEnumerable<InputActionMap> inputActionMaps)
{
    public InputDeviceName DeviceName => deviceName;

    public IReadOnlyCollection<InputActionMap> InputActionMaps { get; } = inputActionMaps.ToArray();
}
