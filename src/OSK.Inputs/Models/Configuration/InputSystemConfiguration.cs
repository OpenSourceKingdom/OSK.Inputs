using System.Collections.Generic;
using System.Linq;

namespace OSK.Inputs.Models.Configuration;
public class InputSystemConfiguration(IEnumerable<InputDefinition> inputDefinitions, IEnumerable<IInputDeviceConfiguration> controllerConfigurations, 
    bool allowCustomInputSchemes, int maxLocalUsers)
{
    public bool AllowCustomInputSchemes => allowCustomInputSchemes;

    public int MaxLocalUsers => maxLocalUsers;

    public IReadOnlyCollection<IInputDeviceConfiguration> SupportedInputControllers { get; } = controllerConfigurations?.ToArray() ?? [];

    public IReadOnlyCollection<InputDefinition> InputDefinitions { get; } = inputDefinitions?.ToArray() ?? [];
}
