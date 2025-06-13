using System;
using System.Collections.Generic;
using System.Linq;

namespace OSK.Inputs.Models.Runtime;

/// <summary>
/// Represents the power that a particlar <see cref="ActivatedInput"/> has in anaglog inputs. Power ranges from -1 to 1, where 0 is no power and 1 is full power.
/// </summary>
/// <param name="inputPower">The enumerable representing the input power on different axes of a particular input (a joystick has x/y axes for example)</param>
public class InputPower(IEnumerable<float> inputPower)
{
    #region Static

    /// <summary>
    /// An empty input power, representing no power on any axis
    /// </summary>
    public static InputPower None => new InputPower([]);

    /// <summary>
    /// Creates an input power with full power on all axes, in the forward direction, where the axis count is specified
    /// </summary>
    /// <param name="axisCount">The total number of axes that should be set to full power</param>
    /// <returns>An input power with all axes set to full</returns>
    public static InputPower FullPower(int axisCount) => new InputPower(Enumerable.Repeat(1f, axisCount));

    /// <summary>
    /// Creates an input power with full power on all axes, in the reverse direction, where the axis count is specified
    /// </summary>
    /// <param name="axisCount">The total number of axes that should be set to full rever power</param>
    /// <returns>An input power with all axes set to full reverse</returns>
    public static InputPower ReverseFullPower(int axisCount) => new InputPower(Enumerable.Repeat(-1f, axisCount));

    /// <summary>
    /// Creates an input power from a set of power levels, where each level corresponds to an axis of the input
    /// </summary>
    /// <param name="powerLevels">The set of power levels for each corresponding axis</param>
    /// <returns>An input power representing all of the axes power levels</returns>
    public static InputPower FromPowerLevels(params float[] powerLevels) => new InputPower(powerLevels);

    #endregion

    #region Variables

    private float[] _inputPowers = inputPower.ToArray();

    #endregion

    #region Helpers

    /// <summary>
    /// An indexer to access the input power for a specific axis by its index. Short-hand for <see cref="GetAxis(int)"/>
    /// </summary>
    /// <param name="index">The specific index to grab input power for</param>
    /// <returns>The power for the axis</returns>
    float this[int index] => GetAxis(index);

    /// <summary>
    /// Retrieves the input power for a specific axis, clamped to -1 to 1.
    /// 
    /// <br />
    /// <br />
    /// Note: If the index is greater than the number of axes, it will return 0.
    /// </summary>
    /// <param name="axisIndex">The axis index to get input power for</param>
    /// <returns>The power of the axis, clamped between -1 to 1</returns>
    /// <exception cref="IndexOutOfRangeException">If the axis index is below 0</exception>
    public float GetAxis(int axisIndex)
    {
        if (axisIndex < 0)
        {
            throw new IndexOutOfRangeException("Axis index can not be less than 0.");
        }
        if (axisIndex >= _inputPowers.Length)
        {
            return 0;
        }

        return Math.Clamp(_inputPowers[axisIndex], -1, 1);
    }

    #endregion
}
