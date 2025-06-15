using System;
using System.Collections.Generic;
using System.Text;
using OSK.Inputs.Models.Configuration;

namespace OSK.Inputs.Models.Runtime;

/// <summary>
/// A data object that allows configuring certain <see cref="InputSystemConfiguration"/> settings at runtime
/// </summary>
public class InputManagerRuntimeConfigurator
{
    #region Variables

    internal int? MaxLocalUsers { get; private set; }

    #endregion

    public InputManagerRuntimeConfigurator SetMaxLocalUsers(int maxLocalUsers)
    {
        MaxLocalUsers = maxLocalUsers;
        return this;
    }
}
