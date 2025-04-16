using System.Collections.Generic;
using OSK.Inputs.Models.Runtime;

namespace OSK.Inputs.Options;
public class JoinUserOptions
{
    #region Static

    public static JoinUserOptions Default = new JoinUserOptions();

    #endregion

    public IEnumerable<InputDeviceIdentifier> DeviceIdentifiers { get; set; } = [];

    public string? ActiveInputDefinitionName { get; set; } = null;
}
