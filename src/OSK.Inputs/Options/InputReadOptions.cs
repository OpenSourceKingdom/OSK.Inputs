using System;

namespace OSK.Inputs.Options;
public class InputReadOptions
{
    public TimeSpan? DeviceReadTime { get; set; }

    public bool RunInputUsersInParallel { get; set; }

    public int MaxConcurrentDevices { get; set; } = 1;
}
