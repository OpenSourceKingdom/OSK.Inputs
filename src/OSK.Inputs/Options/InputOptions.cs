using System;

namespace OSK.Inputs.Options;

public class InputOptions(TimeSpan activationDuration)
{
    public TimeSpan ActivationDuration => activationDuration;
}
