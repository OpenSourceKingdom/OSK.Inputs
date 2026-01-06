using OSK.Inputs.Abstractions.Configuration;

namespace OSK.Inputs.Internal.Services;

internal partial class InputConfigurationProvider: IInputConfigurationProvider
{
    #region IInputSystemConfigurationProvider

    public required InputSystemConfiguration Configuration { get; set; }

    #endregion
}
