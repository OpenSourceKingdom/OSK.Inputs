namespace OSK.Inputs.Abstractions.Runtime;

/// <summary>
/// The status of a device with the input system
/// </summary>
public enum DeviceStatus
{
    /// <summary>
    /// The device is currently connected and receiving inputs
    /// </summary>
    Active,

    /// <summary>
    ///  The device has been put into a state where input can not be received or interacted with by the input system
    /// </summary>
    Disconnected
}
