namespace OSK.Inputs.Abstractions.Configuration;

/// <summary>
/// The behavior that should be used when attempting to pair a new device to a user
/// </summary>
public enum DevicePairingBehavior
{
    /// <summary>
    /// Give the new device to a user based on whether they are closest to a completed <see cref="InputDeviceCombination"/>
    /// or if they have the fewest devices if other things are equal.
    /// 
    /// For example, if there are 2 users and one has a keyboard and the other has no input device, then a newly activate mouse
    /// will be given to the user with a keyboard to complete their comnbination before giving the device to a different user.
    /// If both users have completed combinations, then the device will be given to the first user that has the fewest number of
    /// devices
    /// </summary>
    Balanced,

    /// <summary>
    /// The input system should not handle device pairings automatically and the integrating application will handle it manually,
    /// calling the appropriate user manager methods as needed
    /// </summary>
    Manual
}
