using System;

namespace OSK.Inputs.Options;

public class DigitalInputOptions(int tapCount, TimeSpan activationDuration): InputOptions(activationDuration)
{
    public int TapCount => tapCount;
}
