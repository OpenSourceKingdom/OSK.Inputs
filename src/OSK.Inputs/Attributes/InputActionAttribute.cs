using System;

namespace OSK.Inputs.Attributes;

[AttributeUsage(AttributeTargets.Method)]
public class InputActionAttribute : Attribute
{
    #region Variables

    public string? ActionName { get; }

    public string? Description { get; }

    #endregion

    #region Constructors

    public InputActionAttribute(string actionName, string? description = null)
    {
        ActionName = actionName?.Trim();
        Description = description;
    }

    #endregion
}
