using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using OSK.Inputs.Internal;
using OSK.Inputs.Models.Inputs;
using OSK.Inputs.Options;

namespace OSK.Inputs.Models.Configuration;
public abstract class InputDevice(InputControllerName controllerName, Type inputReaderType)
{
    #region Helpers

    public abstract IEnumerable<IInput> AllInputs { get; }

    internal IInputControllerConfiguration BuildControllerConfiguration()
    {
        var duplicates = AllInputs.GroupBy(input => input.Name).Where(group => group.Count() > 1);
        if (duplicates.Any())
        {
            var error = string.Join(",", duplicates.Select(group => group.Key));
            throw new DuplicateNameException($"One or more input names had duplicates for the {controllerName} controller. The following inputs had the error: {error}");
        }

        return new DefaultInputControllerConfiguration(controllerName, inputReaderType, AllInputs, null);
    }

    #endregion
}
