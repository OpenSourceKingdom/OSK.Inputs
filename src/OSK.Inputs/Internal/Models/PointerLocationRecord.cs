using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace OSK.Inputs.Internal.Models;

internal readonly struct PointerLocationRecord(Vector2 position, TimeSpan time)
{
    public Vector2 Position => position;

    public TimeSpan Time => time;
}
