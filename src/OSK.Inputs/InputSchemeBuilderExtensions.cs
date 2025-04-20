using System;
using System.Collections.Generic;
using System.Text;
using OSK.Inputs.Models.Configuration;
using OSK.Inputs.Ports;

namespace OSK.Inputs;
public static class InputSchemeBuilderExtensions
{
    public static IInputSchemeBuilder AddKeyboardMaps(this IInputSchemeBuilder builder, Action<IInputDeviceActionBuilder> actionConfigurator)
    {

        return builder;
    }
}
