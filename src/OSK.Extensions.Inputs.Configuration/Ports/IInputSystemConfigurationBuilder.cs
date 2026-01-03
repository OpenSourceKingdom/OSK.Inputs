using System;
using System.Collections.Generic;
using System.Text;
using OSK.Extensions.Inputs.Configuration.Options;
using OSK.Inputs.Abstractions.Configuration;

namespace OSK.Extensions.Inputs.Configuration.Ports;

public interface IInputSystemConfigurationBuilder
{
    IInputSystemConfigurationBuilder WithJoinPolicy(Action<InputSystemJoinPolicyOptions> optionsConfigurator);

    IInputSystemConfigurationBuilder WithInputProcessing(Action<InputProcessingOptions> optionsConfigurator);

    IInputSystemConfigurationBuilder WithInputDefinition(string definitionName, Action<IInputDefinitionBuilder> definitionBuildConfigurator);

    IInputSystemConfigurationBuilder WithDevice(InputDeviceSpecification specification);

    InputSystemConfiguration Build();
}
