namespace OSK.Inputs.Options;

public class DigitalInputOptions: InputOptions
{
    /// <summary>
    /// The required number of quick successive presses for the input to be considered active
    /// </summary>
    public int TapCount { get; set; }
}
