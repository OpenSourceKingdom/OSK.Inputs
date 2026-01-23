using System;
using System.Linq;

namespace OSK.Inputs.Abstractions.Devices.Mice;

/// <summary>
/// Defines a specification for mice
/// </summary>
public class GenericMouse: MouseDeviceSpecification
{
    #region InputDeviceSpecification Overrides

    public override InputDeviceFamily DeviceFamily => InputDeviceFamily.GenericMouse;

    protected override MouseInput[] Inputs { get; } 
        = [.. Enum.GetValues(typeof(MouseInput)).Cast<MouseInput>()];

    #endregion
}
