using System.Collections.Generic;

namespace OSK.Inputs.Abstractions.Configuration;

public class InputSchemeActionMap(IEnumerable<DeviceSchemeActionMap> deviceSchemeMaps)
{
    public IEnumerable<DeviceSchemeActionMap> DeviceSchemeMaps => deviceSchemeMaps;
}
