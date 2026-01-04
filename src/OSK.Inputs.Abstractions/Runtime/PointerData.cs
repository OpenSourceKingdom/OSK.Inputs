using System.Numerics;
using OSK.Inputs.Abstractions.Configuration;

namespace OSK.Inputs.Abstractions.Runtime;

/// <summary>
/// A specific pointer on a device and the related information for it
/// </summary>
/// <param name="id">The id for the pointer given by the input system</param>
/// <param name="deviceIdentity">The device the pointer is associated to (Table, PC, etc.)</param>
/// <param name="position">The position on the screen the pointer is currently located</param>
/// <param name="motion">The <see cref="PointerMotion"/> information related to this pointer</param>
public readonly struct PointerData(int id, InputDeviceIdentity deviceIdentity, Vector2 position, PointerMotion motion)
{
    #region Variables

    /// <summary>
    /// The input system id for this pointer
    /// </summary>
    public int Id => id;

    /// <summary>
    /// The device this pointer belongs to
    /// </summary>
    public InputDeviceIdentity DeviceIdentity => deviceIdentity;

    /// <summary>
    /// The position for the pointer
    /// </summary>
    public Vector2 Position => position;

    /// <summary>
    /// The motion information of the pointer that has been captured
    /// </summary>
    public PointerMotion Motion => motion;

    #endregion
}
