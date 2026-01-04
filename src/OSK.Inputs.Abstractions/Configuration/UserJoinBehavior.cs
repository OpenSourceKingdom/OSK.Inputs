namespace OSK.Inputs.Abstractions.Configuration;

/// <summary>
/// Defines how new users are joined to the input system
/// </summary>
public enum UserJoinBehavior
{
    /// <summary>
    /// When a new device has interacted with the input system, then a new user will be created on the game system's behalf.
    /// 
    /// <br />
    /// Note: The final flow of how users and devices are joined is also associated with <see cref="DevicePairingBehavior"/>
    /// </summary>
    DeviceActivation,

    /// <summary>
    /// This indicates that new users should be managed externally from the input system and the consumer will call the 
    /// appropriate methods for the input system to recognize new users.
    /// </summary>
    Manual
}
