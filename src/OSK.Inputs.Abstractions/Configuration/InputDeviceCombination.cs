using System;
using System.Collections.Generic;
using System.Linq;
using OSK.Inputs.Abstractions.Devices;

namespace OSK.Inputs.Abstractions.Configuration;

/// <summary>
/// Represents a set of input devices that can be used together when interacting with the system.
/// For example, this could be a Keyboard or a Keyboard and Mouse
/// </summary>
/// <param name="deviceFamilies">The collection of devices the combination refers to</param>
public readonly struct InputDeviceCombination(InputDeviceFamily[] deviceFamilies)
    : IEquatable<InputDeviceCombination>
{
    #region Static

    public static string GetCombinationId(IEnumerable<InputDeviceFamily> deviceFamilies)
        => string.Join("-", deviceFamilies.Select(family => family.Name));

    #endregion

    #region Constructors

    public InputDeviceCombination(IEnumerable<InputDeviceFamily> deviceFamilies)
        : this([.. deviceFamilies])
    {
    }

    #endregion

    #region Api

    /// <summary>
    /// A unique combination id that is based on the device families this combination refers to
    /// </summary>
    public string Id { get; } = GetCombinationId(deviceFamilies);

    /// <summary>
    /// The collection of devices the combination refers to
    /// </summary>
    public IReadOnlyCollection<InputDeviceFamily> DeviceFamilies => deviceFamilies;

    /// <summary>
    /// Calculates a device support confidence score based on the family provided. The output of this function can be used to order lists of supported 
    /// combinations to get the first 'strongest' combination that matches the family. If there are multiple combinations that provide support to a given
    /// device family, the score is determined then by how many devices the combination needs. For example, a keyboard only combination should match a keyboard
    /// family at 1 whereas a keyboard + mouse combination should match at .5
    /// </summary>
    /// <param name="neededFamily">The family to get a support confidence for</param>
    /// <returns>A score between 0 and 1 that represents the confidence level this combination will support a given device family</returns>
    public float GetDeviceSupportConfidence(InputDeviceFamily neededFamily)
    {
        if (deviceFamilies.Length is 0)
        {
            return 0;
        }

        var matchedStrength = deviceFamilies.Contains(neededFamily)
            ? 1
            : deviceFamilies.Where(family => neededFamily.DeviceType == family.DeviceType).Any()
                ? .5f
                : 0;

        return deviceFamilies.Length is 1
            ? matchedStrength
            : matchedStrength / deviceFamilies.Length;
    }

    /// <summary>
    /// Determines if the provided device identity is in the combination
    /// </summary>
    /// <param name="identity">The identity to check the combination for</param>
    /// <returns>Whether this combination includes the device in question</returns>
    public bool Contains(InputDeviceFamily identity) 
        => deviceFamilies.Contains(identity);

    /// <summary>
    /// Attempts to create a display name for the combination that is more readable for a combination: "Keyboard", "Keyboard and Mouse", etc.
    /// </summary>
    /// <returns>A displayable text name for the combination</returns>
    public string GetCombinationName()
    {
        return deviceFamilies.Length switch
        {
            0 => string.Empty,
            1 => deviceFamilies[0].Name,
            2 => $"{deviceFamilies[0].Name} and {deviceFamilies[1].Name}",
            _ => $"{string.Join(", ", deviceFamilies.Take(deviceFamilies.Length - 1).Select(device => device.Name))}, and {deviceFamilies[^1].Name}"
        };
    }

    #endregion

    #region IEquatable

    public bool Equals(InputDeviceCombination other)
    {
        if (other.DeviceFamilies.Count != deviceFamilies.Length)
        {
            return false;
        }

        return deviceFamilies.SequenceEqual(other.DeviceFamilies);
    }

    #endregion
}
