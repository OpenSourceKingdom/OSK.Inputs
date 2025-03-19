namespace OSK.Inputs.Options;

public class AnalogInputOptions
    : InputOptions
{
    /// <summary>
    /// The minimum required power for the input to be considered active
    /// </summary>
    public float PowerThreshold { get; set; }
}
