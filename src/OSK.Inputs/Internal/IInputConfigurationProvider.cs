using OSK.Inputs.Abstractions.Configuration;

namespace OSK.Inputs.Internal;

internal interface IInputConfigurationProvider
{
    InputSystemConfiguration Configuration { get; set; }
}
