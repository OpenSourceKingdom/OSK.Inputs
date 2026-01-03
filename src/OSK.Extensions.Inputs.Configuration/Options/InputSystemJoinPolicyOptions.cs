using System;
using System.Collections.Generic;
using System.Text;
using OSK.Inputs.Abstractions.Configuration;

namespace OSK.Extensions.Inputs.Configuration.Options;

public class InputSystemJoinPolicyOptions
{
    public int MaxLocalUsers { get; set; }

    public DevicePairingBehavior DevicePairingBehavior { get; set; }

    public UserJoinBehavior UserJoinBehavior { get; set; }
}
