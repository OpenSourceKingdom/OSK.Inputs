using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace OSK.Inputs.Models.Configuration;
public struct InputControllerName(string? name): IEquatable<InputControllerName>
{
    public string Name => name ?? string.Empty;

    public bool Equals(InputControllerName other)
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
        return obj is InputControllerName other && Equals(other);
    }

    public override int GetHashCode()
    {
        return Name is null ? 0 : Name.GetHashCode();
    }

    public static bool operator ==(InputControllerName left, InputControllerName right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(InputControllerName left, InputControllerName right)
    {
        return !(left == right);
    }

    #endregion
}
