using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using OSK.Inputs.Internal;
using OSK.Inputs.Models.Inputs;

namespace OSK.Inputs.Models.Configuration;
public abstract class InputDevice(InputDeviceName deviceName, Type inputReaderType)
{
    #region Helpers

    public abstract IEnumerable<IInput> AllInputs { get; }

    internal IInputDeviceConfiguration BuildDeviceConfiguration()
    {
        var duplicates = AllInputs.GroupBy(input => input.Name).Where(group => group.Count() > 1);
        if (duplicates.Any())
        {
            var error = string.Join(",", duplicates.Select(group => group.Key));
            throw new DuplicateNameException($"One or more input names had duplicates for the {deviceName} device. The following inputs had the error: {error}");
        }

        return new DefaultInputDeviceConfiguration(deviceName, inputReaderType, AllInputs, null);
    }

    #endregion
}
