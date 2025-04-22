using System.Collections.Generic;
using OSK.Inputs.Models.Configuration;

namespace OSK.Inputs.Models.Runtime;
public interface IApplicationInputUser
{
    int Id { get; }

    IEnumerable<InputDeviceIdentifier> DeviceIdentifiers { get; }

    InputDefinition ActiveInputDefinition { get; }

    InputScheme? GetActiveInputScheme(string controllerId);
}
