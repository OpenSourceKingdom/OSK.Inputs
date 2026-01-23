namespace OSK.Inputs.Abstractions.Configuration;

/// <summary>
/// A map that directly triggers a specific action
/// </summary>
public class ActiveInputActionMap: InputActionMap
{
    /// <summary>
    /// The specific action this map references
    /// </summary>
    public required InputAction Action { get; init; }
}
