using OSK.Inputs.Abstractions.Configuration;

namespace OSK.Extensions.Inputs.Configuration.Options;

/// <summary>
/// Defines a set of options to configure the join policy of the input system
/// </summary>
public class InputSystemJoinPolicyOptions
{
    /// <summary>
    /// Informs the input system of the total local users to expect to join and play the game. Players are joined up to the limit, based on the <see cref="UserJoinBehavior"/>
    /// </summary>
    public int MaxLocalUsers { get; set; }

    /// <summary>
    /// Describes how pairing a new device should occur
    /// </summary>
    public DevicePairingBehavior DevicePairingBehavior { get; set; }

    /// <summary>
    /// Describes the behavior the input system uses when encountering a potential new user
    /// </summary>
    public UserJoinBehavior UserJoinBehavior { get; set; }
}
