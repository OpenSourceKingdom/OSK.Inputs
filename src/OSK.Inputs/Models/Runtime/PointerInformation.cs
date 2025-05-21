using System.Numerics;

namespace OSK.Inputs.Models.Runtime;
public class PointerInformation(int pointerId, Vector2[] pointerPositions)
{
    public const int DefaultPointerId = 999;

    public static PointerInformation Default { get; } = new PointerInformation(DefaultPointerId, []);

    public int PointerId => pointerId;

    public Vector2 CurrentPosition => pointerPositions.Length == 1
        ? pointerPositions[^1]
        : Vector2.Zero;

    public Vector2 PreviousPosition => pointerPositions.Length > 1
        ? pointerPositions[^2]
        : CurrentPosition;

    public Vector2 Delta { get; } = pointerPositions.Length <= 1 
        ? Vector2.Zero 
        : pointerPositions[^1] - pointerPositions[^2];

    public Vector2[] PointerPositions => pointerPositions;
}
