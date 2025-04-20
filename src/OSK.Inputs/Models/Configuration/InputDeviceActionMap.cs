using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OSK.Inputs.Models.Configuration;
public class InputDeviceActionMap(InputDeviceName deviceName, IEnumerable<InputActionMap> inputActionMaps)
{
    public InputDeviceName DeviceName => deviceName;

    public IReadOnlyCollection<InputActionMap> InputActionMaps { get; } = inputActionMaps.ToArray();
}
