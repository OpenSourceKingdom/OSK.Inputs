namespace OSK.Inputs.Abstractions.Devices.Mice;

/// <summary>
/// Defines a specification for mice
/// </summary>
public class Mouse: MouseDeviceSpecification
{
    #region Variables

    /// <summary>
    /// Represents an identity for a mouse
    /// </summary>
    public static InputDeviceFamily StandardMice = new("Mouse", InputDeviceType.Mice);

    #endregion

    #region InputDeviceSpecification Overrides

    public override InputDeviceFamily DeviceFamily => StandardMice;

    protected override MouseInput[] Inputs { get; } 
        = [ MouseInput.LeftClick, MouseInput.RightClick, MouseInput.ScrollWheelClick, MouseInput.ScrollWheel, MouseInput.MouseMovement ];

    #endregion
}
