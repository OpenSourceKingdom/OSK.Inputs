using System;

namespace OSK.Inputs.Options;

public class InputOptions
{
    /// <summary>
    /// The mimium amount of time that must be pass for the input to be considered active
    /// </summary>
    public TimeSpan RequiredActivationDuration { get; set; } = TimeSpan.Zero;
}
