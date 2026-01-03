using System;
using System.Collections.Generic;
using System.Text;
using OSK.Inputs.Abstractions.Configuration;

namespace OSK.Extensions.Inputs.Configuration.Ports;

public interface IInputSchemeBuilder
{
    IInputSchemeBuilder WithDevice(InputDeviceIdentity deviceIdentity, Action<IInputDeviceMapBuilder> mapBuilderConfigurator);

    IInputSchemeBuilder MakeDefault();
}
