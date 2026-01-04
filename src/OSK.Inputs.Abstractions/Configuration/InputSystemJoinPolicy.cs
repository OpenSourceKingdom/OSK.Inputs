namespace OSK.Inputs.Abstractions.Configuration;

/// <summary>
/// Specifies the policy by which the input system will handle device pairing, new users, and more.
/// </summary>
public class InputSystemJoinPolicy
{
    /// <summary>
    /// The total number of users the input system will expect to handle
    /// </summary>
    public int MaxUsers { get; init; }

    /// <summary>
    /// The specific type of behavior the input system will use when interacting with new users
    /// </summary>
    public UserJoinBehavior UserJoinBehavior { get; init; }

    /// <summary>
    /// The specific type of behavior the input system will use when interacting with new devices
    /// </summary>
    public DevicePairingBehavior DeviceJoinBehavior { get; init; }
}
