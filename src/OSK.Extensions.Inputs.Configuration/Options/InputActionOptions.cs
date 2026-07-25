using OSK.Inputs.Abstractions.Inputs;

namespace OSK.Extensions.Inputs.Configuration.Options;

/// <summary>
/// A set of options specific to input actions
/// </summary>
public class InputActionOptions
{
    /// <summary>
    /// An optional grouping that can be used to group specific input actions together. i.e. group pointer actions or similar so that they can be manipulated as group. This is best used when wanting to suppress
    /// groups of input, as an example.
    /// </summary>
    public int? ActionGroup { get; set; }

    /// <summary>
    /// Extra input streams that are included with the action. For example, an input action can include the pointer input stream data if it is relevant to the action's execution
    /// </summary>
    public InputStreamType[] IncludedInputStreams { get; set; } = [];

    /// <summary>
    /// Describes the input action, best utilized with UI for input configuration
    /// </summary>
    public string? Description { get; set; }
}
