namespace OSK.Inputs.Abstractions.Configuration;

public class InputSystemJoinPolicy
{
    public int MaxUsers { get; init; }

    public UserJoinBehavior UserJoinBehavior { get; init; }

    public DevicePairingBehavior DeviceJoinBehavior { get; init; }
}
