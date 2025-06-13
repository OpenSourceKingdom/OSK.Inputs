using System.Collections.Generic;
using System.Linq;

namespace OSK.Inputs.Models.Configuration;
public class InputSystemConfiguration(IEnumerable<InputDefinition> inputDefinitions, IEnumerable<InputControllerConfiguration> controllerConfigurations,
    IEnumerable<IInputDeviceConfiguration> deviceConfigurations, bool allowCustomInputSchemes, int maxLocalUsers)
{
    public bool AllowCustomInputSchemes => allowCustomInputSchemes;

    public int MaxLocalUsers { get; internal set; } = maxLocalUsers;

    public IReadOnlyCollection<InputControllerConfiguration> InputControllers { get; } = controllerConfigurations.ToArray();

    public IReadOnlyCollection<IInputDeviceConfiguration> SupportedInputDevices { get; } = deviceConfigurations?.ToArray() ?? [];

    public IReadOnlyCollection<InputDefinition> InputDefinitions { get; } = inputDefinitions?.ToArray() ?? [];
}
