namespace OSK.Inputs.Models.Inputs;

/// <summary>
/// Represents some form of input. i.e. physical buttons, software enabled input, or anything else
/// </summary>
public interface IInput
{
    int Id { get; }

    string DeviceType { get; }

    string Name { get; }
}
