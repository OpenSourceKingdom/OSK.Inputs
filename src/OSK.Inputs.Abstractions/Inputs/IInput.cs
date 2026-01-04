namespace OSK.Inputs.Abstractions.Inputs;

public interface IInput
{
    public int Id { get; }

    string DeviceType { get; }

    InputType InputType { get; }
}
