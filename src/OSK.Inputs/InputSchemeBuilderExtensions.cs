using OSK.Inputs.Models.Inputs;
using OSK.Inputs.Ports;

namespace OSK.Inputs;

public static class InputSchemeBuilderExtensions
{
    #region Assign Input

    public static IInputSchemeBuilder AssignStartAction(this IInputSchemeBuilder builder, string actionKey, IInput input)
        => builder.AssignInput(actionKey, input, InputPhase.Start);

    public static IInputSchemeBuilder AssignHoldAction(this IInputSchemeBuilder builder, string actionKey, IInput input)
        => builder.AssignInput(actionKey, input, InputPhase.Hold);

    public static IInputSchemeBuilder AssignTranslationAction(this IInputSchemeBuilder builder, string actionKey, IInput input)
        => builder.AssignInput(actionKey, input, InputPhase.Translation);

    public static IInputSchemeBuilder AssignEndAction(this IInputSchemeBuilder builder, string actionKey, IInput input)
        => builder.AssignInput(actionKey, input, InputPhase.End);

    #endregion
}
