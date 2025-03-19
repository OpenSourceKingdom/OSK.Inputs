using System;

namespace OSK.Inputs.Models.Runtime;
public struct InputControllerIdentifier(int controllerId, string controllerName)
{
    public int ControllerId => controllerId;

    public string ControllerName => controllerName;

    #region Operators

    public override bool Equals(object? obj)
    {
        return obj is InputControllerIdentifier identifier &&
               ControllerId == identifier.ControllerId &&
               ControllerName.Equals(identifier.ControllerName, StringComparison.Ordinal);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(ControllerId, ControllerName);
    }

    public static bool operator ==(InputControllerIdentifier left, InputControllerIdentifier right)
    {
        return left.ControllerId == right.ControllerId && left.ControllerName.Equals(right.ControllerName, StringComparison.Ordinal);
    }

    public static bool operator !=(InputControllerIdentifier left, InputControllerIdentifier right)
    {
        return left.ControllerId != right.ControllerId || !left.ControllerName.Equals(right.ControllerName, StringComparison.Ordinal);
    }

    #endregion
}
