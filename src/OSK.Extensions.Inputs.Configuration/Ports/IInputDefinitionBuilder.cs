using System;
using System.Collections.Generic;
using System.Text;
using OSK.Extensions.Inputs.Configuration.Options;
using OSK.Inputs.Abstractions.Configuration;
using OSK.Inputs.Abstractions.Inputs;
using OSK.Inputs.Abstractions.Runtime;

namespace OSK.Extensions.Inputs.Configuration.Ports;

public interface IInputDefinitionBuilder
{
    IInputDefinitionBuilder WithAction(string name, Action<InputEventContext> executor, IEnumerable<InputPhase> triggerPhases,
        InputActionOptions actionOptions);

    IInputDefinitionBuilder WithScheme(string name, Action<IInputSchemeBuilder> schemeBuildConfiguration);

    IInputDefinitionBuilder MakeDefault();
}
