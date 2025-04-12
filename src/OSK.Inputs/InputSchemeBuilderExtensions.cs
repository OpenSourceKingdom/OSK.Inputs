using OSK.Inputs.Models.Inputs;
using OSK.Inputs.Ports;

namespace OSK.Inputs;

public static class InputSchemeBuilderExtensions
{
    #region Assign Input

    public static IInputSchemeBuilder AssignStartAction(this IInputSchemeBuilder builder, IInput input, string actionKey)
        => builder.AssignInput(input, InputPhase.Start, actionKey);

    public static IInputSchemeBuilder AssignHoldAction(this IInputSchemeBuilder builder, IInput input, string actionKey)
        => builder.AssignInput(input, InputPhase.Active, actionKey);

    public static IInputSchemeBuilder AssignTranslationAction(this IInputSchemeBuilder builder, IInput input, string actionKey)
        => builder.AssignInput(input, InputPhase.Translation, actionKey);

    public static IInputSchemeBuilder AssignEndAction(this IInputSchemeBuilder builder, IInput input, string actionKey)
        => builder.AssignInput(input, InputPhase.End, actionKey);

    #endregion
}
