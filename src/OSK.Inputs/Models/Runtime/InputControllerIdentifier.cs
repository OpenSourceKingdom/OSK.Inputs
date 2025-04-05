using System;
using OSK.Inputs.Models.Configuration;

namespace OSK.Inputs.Models.Runtime;
public struct InputControllerIdentifier(int controllerId, InputControllerName controllerName)
{
    public int ControllerId => controllerId;

    public InputControllerName ControllerName => controllerName;

    #region Operators

    public override bool Equals(object? obj)
    {
        return obj is InputControllerIdentifier identifier &&
               ControllerId == identifier.ControllerId &&
               ControllerName == identifier.ControllerName;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(ControllerId, ControllerName);
    }

    public static bool operator ==(InputControllerIdentifier left, InputControllerIdentifier right)
    {
        return left.ControllerId == right.ControllerId && left.ControllerName == right.ControllerName;
    }

    public static bool operator !=(InputControllerIdentifier left, InputControllerIdentifier right)
    {
        return left.ControllerId != right.ControllerId || left.ControllerName != right.ControllerName;
    }

    #endregion
}
