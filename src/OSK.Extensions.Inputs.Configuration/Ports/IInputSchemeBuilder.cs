using System;
using OSK.Inputs.Abstractions.Configuration;

namespace OSK.Extensions.Inputs.Configuration.Ports;

public interface IInputSchemeBuilder
{
    IInputSchemeBuilder WithDevice(InputDeviceIdentity deviceIdentity, Action<IInputDeviceMapBuilder> mapBuilderConfigurator);

    IInputSchemeBuilder MakeDefault();
}
