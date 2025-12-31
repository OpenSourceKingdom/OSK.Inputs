using System.Collections.Generic;

namespace OSK.Inputs.Abstractions;

public class InputSchemeActionMap(IEnumerable<VirtualInput> virtualInputs, IEnumerable<DeviceSchemeActionMap> deviceSchemeMaps)
{
    public IEnumerable<VirtualInput> VirtualInputs => virtualInputs;

    public IEnumerable<DeviceSchemeActionMap> DeviceSchemeMaps => deviceSchemeMaps;
}
