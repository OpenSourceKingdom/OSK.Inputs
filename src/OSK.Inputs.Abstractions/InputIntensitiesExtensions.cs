using System;
using System.Collections.Generic;
using System.Linq;
using OSK.Inputs.Abstractions.Inputs;

namespace OSK.Inputs.Abstractions;

public static class InputIntensitiesExtensions
{
    /// <summary>
    /// Calculates the magnitude of a given enumeration of <see cref="InputIntensity"/>
    /// </summary>
    /// <param name="inputIntensities">the intensities enumeration to calculate a magnitude for</param>
    /// <returns>The magnitude</returns>
    public static float CalculateMagnitude(this IEnumerable<InputIntensity> inputIntensities)
    {
        var sumOfSquares = inputIntensities.Sum(intensity => MathF.Pow(intensity.Power, 2));

        return MathF.Sqrt(sumOfSquares);
    }
}
