using System;

namespace OSK.Inputs.Models;

/// <summary>
/// A set of flags that determines how saving an input scheme should be handled
/// </summary>
[Flags]
public enum SchemeSaveFlags
{
    /// <summary>
    /// No special save behavior. If a file already exists, the save will fail.
    /// </summary>
    None = 0,

    /// <summary>
    /// The saving operation should overwrite any existing scheme with the same name
    /// </summary>
    Overwrite = 1
}
