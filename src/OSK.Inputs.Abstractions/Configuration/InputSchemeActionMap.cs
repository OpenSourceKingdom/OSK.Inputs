using System.Collections.Generic;
using OSK.Inputs.Abstractions.Inputs;

namespace OSK.Inputs.Abstractions.Configuration;

public class InputSchemeActionMap(IEnumerable<DeviceSchemeActionMap> deviceSchemeMaps)
{
    public IEnumerable<DeviceSchemeActionMap> DeviceSchemeMaps => deviceSchemeMaps;
}
