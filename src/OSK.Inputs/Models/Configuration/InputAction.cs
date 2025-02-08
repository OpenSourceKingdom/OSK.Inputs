namespace OSK.Inputs.Models.Configuration;

public readonly struct InputAction(string actionKey, string? description)
{
    public string ActionKey => actionKey;

    public string? Description => description;
}
