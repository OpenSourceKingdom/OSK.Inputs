using System.Collections.Generic;
using System.Numerics;

namespace OSK.Inputs.Models.Runtime;
public class PointerInformation(int pointerId, Vector2 start, Vector2? end = null, IEnumerable<Vector2>? previousDeltas = null)
{
    public Vector2 Start => start;

    public Vector2 End => end ?? start;

    public Vector2 Delta { get; } = end.HasValue ? end.Value - start : Vector2.Zero;

    public IEnumerable<Vector2> PreviousDeltas { get; } = previousDeltas ?? [];
}
