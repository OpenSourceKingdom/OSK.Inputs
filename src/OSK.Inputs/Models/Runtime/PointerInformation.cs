using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace OSK.Inputs.Models.Runtime;
public class PointerInformation(int pointerId, Vector2[] pointerPositions)
{
    public const int DefaultPointerId = 999;

    public int PointerId => pointerId;

    public Vector2 CurrentPosition => pointerPositions[^1];

    public Vector2 PreviousPosition => pointerPositions.Length > 1
        ? pointerPositions[^2]
        : CurrentPosition;

    public Vector2 Delta { get; } = pointerPositions.Length <= 1 
        ? Vector2.Zero 
        : pointerPositions[^1] - pointerPositions[^2];

    public Vector2[] PointerPositions => pointerPositions;
}
