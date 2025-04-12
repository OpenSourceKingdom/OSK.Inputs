using System;

namespace OSK.Inputs.Models.Configuration;
public struct InputDeviceName(string? name): IEquatable<InputDeviceName>
{
    public string Name => name ?? string.Empty;

    public bool Equals(InputDeviceName other)
    {
        return Name == other.Name;
    }

    #region Operators

    public override string ToString()
    {
        return Name;
    }

    public override bool Equals(object obj)
    {
        return obj is InputDeviceName other && Equals(other);
    }

    public override int GetHashCode()
    {
        return Name is null ? 0 : Name.GetHashCode();
    }

    public static bool operator ==(InputDeviceName left, InputDeviceName right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(InputDeviceName left, InputDeviceName right)
    {
        return !(left == right);
    }

    #endregion
}
