using OSK.Inputs.Abstractions.Configuration;

namespace OSK.Inputs.Abstractions.Devices.Mice;

/// <summary>
/// Defines a specification for mice
/// </summary>
public class Mouse: InputDeviceSpecification<IMouseInput>
{
    #region Variables

    /// <summary>
    /// Represents an identity for a mouse
    /// </summary>
    public static InputDeviceIdentity StandardMouse = new("Mouse", MouseInputs.MouseDeviceType);

    #endregion

    #region InputDeviceSpecification Overrides

    public override InputDeviceIdentity DeviceIdentity => StandardMouse;

    public override IMouseInput[] Inputs { get; } 
        = [ MouseInputs.LeftClick, MouseInputs.RightClick, MouseInputs.ScrollWheelClick, MouseInputs.ScrollWheel, MouseInputs.MouseMovement ];

    #endregion
}
