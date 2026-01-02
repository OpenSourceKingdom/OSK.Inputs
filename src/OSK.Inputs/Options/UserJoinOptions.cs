using System.Collections.Generic;
using OSK.Inputs.Abstractions.Runtime;

namespace OSK.Inputs.Options;

public class UserJoinOptions
{
    public IEnumerable<RuntimeDeviceIdentifier>? DevicesToPair { get; set; }

    public ActiveInputScheme? ActiveScheme { get; set; }
}
