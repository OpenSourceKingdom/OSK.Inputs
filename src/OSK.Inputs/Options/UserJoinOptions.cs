using System.Collections.Generic;
using OSK.Inputs.Abstractions.Runtime;

namespace OSK.Inputs.Options;

/// <summary>
/// Provides a set of options when a user is joining the input system
/// </summary>
public class UserJoinOptions
{
    /// <summary>
    /// The input devices to pair to the user upon creation
    /// </summary>
    public IEnumerable<RuntimeDeviceIdentifier>? DevicesToPair { get; set; }

    /// <summary>
    /// The preferred active definition name for the user upon creation
    /// </summary>
    public string? ActiveDefinitionName { get; set; }
}
