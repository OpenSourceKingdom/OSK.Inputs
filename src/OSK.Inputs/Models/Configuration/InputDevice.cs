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
        var duplicateIdentifiers = AllInputs.GroupBy(input => input.Id).Where(group => group.Count() > 1);
        if (duplicateIdentifiers.Any())
        {
            var error = string.Join(",", duplicateIdentifiers.SelectMany(group => group.Select(input => input.Name)));
            throw new DuplicateNameException($"One or more input names had duplicate ids for the {deviceName} device. The following inputs had the error: {error}");
        }

        var duplicateNames = AllInputs.GroupBy(input => input.Name).Where(group => group.Count() > 1);
        if (duplicateNames.Any())
        {
            var error = string.Join(",", duplicateNames.Select(group => group.Key));
            throw new DuplicateNameException($"One or more input names had duplicate names for the {deviceName} device. The following inputs had the error: {error}");
        }

        return new DefaultInputDeviceConfiguration(deviceName, inputReaderType, AllInputs, null);
    }

    #endregion
}
