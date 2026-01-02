using System;
using System.Collections.Generic;
using System.Text;

namespace OSK.Inputs.Models;

[Flags]
public enum SchemeSaveFlags
{
    None = 0,
    Overwrite = 1
}
