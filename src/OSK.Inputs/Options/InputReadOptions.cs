using System;

namespace OSK.Inputs.Options;
public class InputReadOptions
{
    public TimeSpan? ControllerReadTime { get; set; }

    public bool RunInputUsersInParallel { get; set; }

    public int MaxConcurrentControllers { get; set; } = 1;
}
