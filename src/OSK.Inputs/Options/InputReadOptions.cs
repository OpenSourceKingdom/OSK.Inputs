using System;
using OSK.Inputs.Ports;

namespace OSK.Inputs.Options;
public class InputReadOptions
{
    #region Static

    /// <summary>
    /// Meant for systems that require operating on a universal 'main' thread and are not fully able to utilize the .NET multi-threaded
    /// runtime without system performance issues. 
    /// 
    /// This set of options will only allow sequential reading of input devices per user
    /// </summary>
    public static InputReadOptions SingleThreaded { get; } = new InputReadOptions()
    {
        DeviceReadTime = TimeSpan.FromMilliseconds(10),
        MaxConcurrentDevices = 1,
        MaxConcurrenUsers = 1,
        RunInputUsersInParallel = false
    };

    /// <summary>
    /// Meant for single user input reading (i.e. single-player games or similar usage). The goal is to read more than a single device (keyboard + mouse)
    /// for an individual user
    /// </summary>
    public static InputReadOptions DefaultSingleUser { get; } = new InputReadOptions()
    {
        DeviceReadTime = TimeSpan.FromMilliseconds(10),
        MaxConcurrentDevices = 2,
        MaxConcurrenUsers = 1,
        RunInputUsersInParallel = false
    };

    /// <summary>
    /// Meant for dual user input reading (i.e. two-player local co-op or similar usage). The goal is to read a single device (game pad) for more than one
    /// two users
    /// </summary>
    public static InputReadOptions DefaultTwoUsers { get; } = new InputReadOptions()
    {
        DeviceReadTime = TimeSpan.FromMilliseconds(10),
        MaxConcurrentDevices = 1,
        MaxConcurrenUsers = 2,
        RunInputUsersInParallel = true
    };

    #endregion

    /// <summary>
    /// The <see cref="TimeSpan"/> period allotted to an individual <see cref="IInputDeviceReader"/> during a read phase
    /// </summary>
    public TimeSpan? DeviceReadTime { get; set; }

    /// <summary>
    /// Sets whether input users should be run in parallel when attempting to read inputs.
    /// </summary>
    /// <remarks>
    /// Note: this can potentially impact system performance and/or cause issues and cause an explosion of simultaneous
    ///  device reads if used in conjunction with higher values for <see cref="MaxConcurrentDevices"/> and <see cref="MaxConcurrenUsers"/>
    /// </remarks>
    public bool RunInputUsersInParallel { get; set; }

    /// <summary>
    /// The total maximum concurrent users, whether multi-threading or in parallel, that is allowed
    /// </summary>
    /// <remarks>
    /// Note: this can potentially impact system performance and/or cause issues and cause an explosion of simultaneous
    ///  device reads if used in conjunction with <see cref="MaxConcurrentDevices"/>
    /// </remarks>
    public int MaxConcurrenUsers { get; set; }

    /// <summary>
    /// 
    /// The total maximum concurrent devices, per user, whether multi-threading or in parallel, that is allowed
    /// </summary>
    /// <remarks>
    /// Note: this can potentially impact system performance and/or cause issues and cause an explosion of simultaneous
    ///  device reads if used in conjunction with <see cref="MaxConcurrenUsers"/>
    /// </remarks>
    public int MaxConcurrentDevices { get; set; }
}
