using System;
using System.Numerics;

namespace OSK.Inputs.Internal.Models;

internal readonly struct PointerLocationRecord(Vector2 position, TimeSpan time)
{
    public Vector2 Position => position;

    public TimeSpan Time => time;
}
