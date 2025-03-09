using System;
using System.Collections.Generic;
using System.Text;

namespace OSK.Inputs.Models.Configuration;
public class PlayerInputSystemConfiguration(int playerId, IEnumerable<InputReceiverConfiguration> receiverConfigurations)
{
    public int PlayerId => playerId;

    public IEnumerable<InputReceiverConfiguration> ReceiverConfigurations => receiverConfigurations;
}
