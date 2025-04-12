using System.Collections.Generic;
using OSK.Inputs.Models.Configuration;

namespace OSK.Inputs.Models.Runtime;
public interface IApplicationInputUser
{
    int Id { get; }

    IEnumerable<InputControllerIdentifier> ControllerIdentifiers { get; }

    InputDefinition ActiveInputDefinition { get; }

    InputScheme? GetActiveInputScheme(InputDeviceName deviceName);
}
