using OSK.Inputs.Options;

namespace OSK.Inputs.Models.Configuration;

public readonly struct InputAction(string actionKey, string? description, InputActionOptions? options = null)
{
    public string ActionKey => actionKey;

    public string? Description => description;

    public InputActionOptions Options { get; } = options ?? new InputActionOptions();
}
