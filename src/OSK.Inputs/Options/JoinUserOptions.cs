using System.Collections.Generic;
using OSK.Inputs.Models.Runtime;

namespace OSK.Inputs.Options;
public class JoinUserOptions
{
    #region Static

    public static JoinUserOptions Default = new JoinUserOptions();

    #endregion

    public IEnumerable<InputControllerIdentifier> ControllerIdentifiers { get; set; } = [];

    public string? ActiveInputDefinitionName { get; set; } = null;
}
