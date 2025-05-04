using System;
using System.Collections.Generic;
using System.Linq;
using OSK.Inputs.Models.Configuration;
using OSK.Inputs.Models.Inputs;
using OSK.Inputs.Ports;

namespace OSK.Inputs.Internal;
internal class DefaultInputDeviceConfiguration(InputDeviceName deviceName, Type readerType, IEnumerable<IInput> inputs,
    Func<IInput, bool>? validator) : IInputDeviceConfiguration
{
    private readonly Dictionary<int, IInput> _inputLookup = inputs?.ToDictionary(input => input.Id) ?? [];

    /// <summary>
    /// Strongly typed device name
    /// </summary>
    public InputDeviceName DeviceName => deviceName;

    /// <summary>
    /// The type of object that is able process input from this device. See <see cref="IInputDeviceReader"/>
    /// </summary>
    public Type InputReaderType => readerType;

    /// <summary>
    /// The collection of <see cref="IInput"/>s associated to this device
    /// </summary>
    public IReadOnlyCollection<IInput> Inputs { get; } = inputs?.ToArray() ?? [];

    /// <summary>
    /// Used to validate input schemes that are associated with this device
    /// </summary>
    /// <param name="input">The input being added to a scheme</param>
    /// <returns>Whether the input is valid for this device</returns>
    public bool IsValidInput(IInput input) 
        => validator?.Invoke(input) 
        ?? _inputLookup.TryGetValue(input.Id, out var inputValue) && inputValue.DeviceType.Equals(input.DeviceType, StringComparison.Ordinal);
}
