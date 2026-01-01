namespace OSK.Inputs.Abstractions.Inputs;

public readonly struct InputPower(InputAxis axis, float power)
{
    public InputAxis Axis => axis;

    public float Power => power;
}
