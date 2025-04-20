using OSK.Inputs.Models.Inputs;
using OSK.Inputs.Ports;

namespace OSK.Inputs;

public static class InputDeviceActionBuilderExtensions
{
    #region Assign Input

    public static IInputDeviceActionBuilder AssignStartAction(this IInputDeviceActionBuilder builder, IInput input, string actionKey)
        => builder.AssignInput(input, InputPhase.Start, actionKey);

    public static IInputDeviceActionBuilder AssignHoldAction(this IInputDeviceActionBuilder builder, IInput input, string actionKey)
        => builder.AssignInput(input, InputPhase.Active, actionKey);
    public static IInputDeviceActionBuilder AssignTranslationAction(this IInputDeviceActionBuilder builder, IInput input, string actionKey)
        => builder.AssignInput(input, InputPhase.Translation, actionKey);

    public static IInputDeviceActionBuilder AssignEndAction(this IInputDeviceActionBuilder builder, IInput input, string actionKey)
        => builder.AssignInput(input, InputPhase.End, actionKey);

    #endregion
}
