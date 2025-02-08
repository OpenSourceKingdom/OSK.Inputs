using System;

namespace OSK.Inputs.Options;

public class AnalogInputOptions(float powerThreshold, TimeSpan activationDuration)
    : InputOptions(activationDuration)
{
    public float PowerThreshold => powerThreshold;
}
