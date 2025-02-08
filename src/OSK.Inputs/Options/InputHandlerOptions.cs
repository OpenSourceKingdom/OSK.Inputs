using System;

namespace OSK.Inputs.Options;
public class InputHandlerOptions
{
    public TimeSpan ControllerReadTime { get; set; }

    public bool RunInputReceiversInParallel { get; set; }

    public int MaxConcurrentReceivers { get; set; } = 1;
}
