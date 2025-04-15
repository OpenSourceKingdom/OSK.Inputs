using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace OSK.Inputs;
public static class VectorExtensions
{
    public static float GetAngleBetween(this Vector2 postion, Vector2 other)
    {
        var dotProduct = Vector2.Dot(Vector2.Normalize(postion), Vector2.Normalize(other));
        var clampedDot = Math.Clamp(dotProduct, -1, 1);

        var angleInRadians = MathF.Acos(clampedDot);
        return angleInRadians * (180 / MathF.PI);
    }
}
