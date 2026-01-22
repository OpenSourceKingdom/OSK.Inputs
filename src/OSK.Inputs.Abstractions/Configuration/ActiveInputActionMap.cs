namespace OSK.Inputs.Abstractions.Configuration;

public class ActiveInputActionMap: InputActionMap
{
    /// <summary>
    /// The specific action this map references
    /// </summary>
    public required InputAction Action { get; init; }
}
