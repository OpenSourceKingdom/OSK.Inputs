using System.Numerics;

namespace OSK.Inputs.Models.Runtime;

/// <summary>
/// The pointer information for use with an input. This is the location of the input pointer on the screen at the time the input was triggered
/// </summary>
/// <param name="pointerId">The specific pointer id. This could vary depending on how many touches or mice are used</param>
/// <param name="pointerPositions">The set of pointer data for the current pointer id over the course of an input. This data set represents the history of travel, i.e. for swipe related data processing if needed</param>
public class PointerInformation(int pointerId, Vector2[] pointerPositions)
{
    /// <summary>
    /// A default pointer id, if specific pointer id data is not required
    /// </summary>
    public const int DefaultPointerId = 999;

    /// <summary>
    /// Default pointer information for an empty data set of pointer information
    /// </summary>
    public static PointerInformation Default { get; } = new PointerInformation(DefaultPointerId, []);

    /// <summary>
    /// The pointer id for the input. This is used to differentiate between multiple pointers, such as in touch or multi-mouse scenarios
    /// </summary>
    public int PointerId => pointerId;

    /// <summary>
    /// The current position of the pointer, especailly useful if a data set exists for pointer position hisstory
    /// </summary>
    public Vector2 CurrentPosition => pointerPositions.Length == 1
        ? pointerPositions[^1]
        : Vector2.Zero;

    /// <summary>
    /// The previous pointer position, if available, and is useful for calculating deltas or changes in position. 
    /// This will be the the value of the current position if no previous position exists.
    /// </summary>
    public Vector2 PreviousPosition => pointerPositions.Length > 1
        ? pointerPositions[^2]
        : CurrentPosition;

    /// <summary>
    /// The delta between the current and previous pointer positions. This is useful for determining movement or changes in position.
    /// </summary>
    public Vector2 Delta { get; } = pointerPositions.Length <= 1 
        ? Vector2.Zero 
        : pointerPositions[^1] - pointerPositions[^2];

    /// <summary>
    /// The list of pointer positions for the current pointer id. This is useful for tracking the history of pointer movements, such as for swipe gestures or other input patterns.
    /// </summary>
    public Vector2[] PointerPositions => pointerPositions;
}
