using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace OSK.Inputs.Abstractions.Runtime;

public readonly struct PointerMotion(Vector2 velocity, Vector2 acceleration)
{
    public Vector2 Velocity => velocity;

    public Vector2 Acceleration => acceleration;
}
