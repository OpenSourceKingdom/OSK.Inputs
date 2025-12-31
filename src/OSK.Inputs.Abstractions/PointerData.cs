using System.Collections.Generic;
using System.Numerics;

namespace OSK.Inputs.Abstractions;

public readonly struct PointerData(int id, InputDeviceIdentity deviceIdentity, Vector2 position, PointerMotion motion)
{
    #region Variables

    public int Id => id;

    public InputDeviceIdentity DeviceIdentity => deviceIdentity;

    public Vector2 Position => position;

    public PointerMotion Motion => motion;

    #endregion
}
