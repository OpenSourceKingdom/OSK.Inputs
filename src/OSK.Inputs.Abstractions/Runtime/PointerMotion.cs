using System.Numerics;

namespace OSK.Inputs.Abstractions.Runtime;

/// <summary>
/// Details the motion aspects of a pointer on a device. A pointer can be associated with a mouse, touch, or other device.
/// </summary>
/// <param name="velocity">The velocity the mouse moved</param>
/// <param name="acceleration">The acceleration of the mouse</param>
public readonly struct PointerMotion(Vector2 velocity, Vector2 acceleration)
{
    /// <summary>
    /// The velocity of the pointer. Velocities of zero indicate no movement.
    /// </summary>
    public Vector2 Velocity => velocity;

    /// <summary>
    /// The acceleration the pointer made
    /// </summary>
    public Vector2 Acceleration => acceleration;
}
