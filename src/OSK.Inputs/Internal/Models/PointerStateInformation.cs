using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using OSK.Inputs.Abstractions.Runtime;

namespace OSK.Inputs.Internal.Models;

internal readonly struct PointerStateInformation(Vector2 startPosition, Vector2 currentPosition, PointerMotion motion)
{
    public Vector2 StartPosition => startPosition;
    public Vector2 CurrentPosition => currentPosition;
    public PointerMotion Motion => motion;
}
