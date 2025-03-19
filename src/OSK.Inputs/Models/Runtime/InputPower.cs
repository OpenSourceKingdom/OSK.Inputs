using System;
using System.Collections.Generic;
using System.Linq;

namespace OSK.Inputs.Models.Runtime;

/// <summary>
/// Represents the power that a particlar <see cref="ActivatedInput"/> has
/// </summary>
/// <param name="inputPower">The enumerable representing the input power on different axes of a particular input (a joystick has x/y axes for example)</param>
public class InputPower(IEnumerable<float> inputPower)
{
    #region Static

    public static InputPower FullPower(int axisCount) => new InputPower(Enumerable.Repeat(1f, axisCount));

    #endregion

    #region Variables

    private float[] _inputPowers = inputPower.ToArray();

    #endregion

    #region Helpers

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
