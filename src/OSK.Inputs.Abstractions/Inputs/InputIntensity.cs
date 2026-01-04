namespace OSK.Inputs.Abstractions.Inputs;

/// <summary>
/// Represents the intensity level a particular <see cref="Input"/> has been given.
/// </summary>
/// <param name="Axis">The specific <see cref="InputAxis"/> the intensity is associated to.</param>
/// <param name="Power">The level of power, pressure, or similar semantic given to the input</param>
public readonly record struct InputIntensity(InputAxis Axis, float Power)
{
    #region Static

    /// <summary>
    /// Creates an intensity on the given axis that is set to full power
    /// </summary>
    /// <param name="axis">The <see cref="InputAxis"/> the intensity is associated with</param>
    /// <returns>An intensity at full power for the axis</returns>
    public static InputIntensity Full(InputAxis axis) => new(axis, 1);

    /// <summary>
    /// Creates an intensity on the given axis that is set to no power
    /// </summary>
    /// <param name="axis">The <see cref="InputAxis"/> the inensity is associated with</param>
    /// <returns>An intensity at zero power for the axis</returns>
    public static InputIntensity Zero(InputAxis axis) => new(axis, 0);

    #endregion
}
