namespace OSK.Inputs.Abstractions.Devices.Mice;

/// <summary>
/// Defines a specification for mice
/// </summary>
public class Mouse: MouseDeviceSpecification
{
    #region InputDeviceSpecification Overrides

    public override InputDeviceFamily DeviceFamily => InputDeviceFamily.Mice;

    protected override MouseInput[] Inputs { get; } 
        = [ MouseInput.LeftClick, MouseInput.RightClick, MouseInput.ScrollWheelClick, MouseInput.ScrollWheel, MouseInput.MouseMovement ];

    #endregion
}
