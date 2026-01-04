using System;

namespace OSK.Extensions.Inputs.Configuration.Options;

public class InputProcessingOptions
{
    public TimeSpan? TapDelayTime { get; set; } = TimeSpan.FromSeconds(1);

    public TimeSpan? StartPhaseDelayBeforeActive { get; set; } = TimeSpan.FromSeconds(1);
}
