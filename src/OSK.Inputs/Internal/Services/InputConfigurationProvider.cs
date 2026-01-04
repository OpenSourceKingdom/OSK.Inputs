using OSK.Inputs.Abstractions.Configuration;

namespace OSK.Inputs.Internal.Services;

internal partial class InputConfigurationProvider(InputSystemConfiguration configuration): IInputConfigurationProvider
{
    #region IInputSystemConfigurationProvider

    public InputSystemConfiguration Configuration => configuration;

    #endregion
}
