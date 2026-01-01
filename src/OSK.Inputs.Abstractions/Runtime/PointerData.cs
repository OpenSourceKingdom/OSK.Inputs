using System.Numerics;
using OSK.Inputs.Abstractions.Configuration;

namespace OSK.Inputs.Abstractions.Runtime;

public readonly struct PointerData(int id, InputDeviceIdentity deviceIdentity, Vector2 position, PointerMotion motion)
{
    #region Variables

    public int Id => id;

    public InputDeviceIdentity DeviceIdentity => deviceIdentity;

    public Vector2 Position => position;

    public PointerMotion Motion => motion;

    #endregion
}
