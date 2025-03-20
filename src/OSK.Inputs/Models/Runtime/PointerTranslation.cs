using System.Numerics;

namespace OSK.Inputs.Models.Runtime;
public class PointerTranslation(Vector2 start, Vector2? end)
{
    public Vector2 Start => start;

    public Vector2? End => end;

    public Vector2 Delta { get; } = end.HasValue ? end.Value - start : Vector2.Zero;
}
