using OSK.Inputs.Models.Inputs;
using OSK.Inputs.Ports;

namespace OSK.Inputs;

public static class InputDeviceActionBuilderExtensions
{
    #region Assign Input

    public static IInputDeviceActionBuilder<TInput> AssignStartAction<TInput>(this IInputDeviceActionBuilder<TInput> builder, 
        TInput input, string actionKey)
        where TInput : IInput
        => builder.AssignInput(input, [InputPhase.Start], actionKey);

    public static IInputDeviceActionBuilder<TInput> AssignActiveAction<TInput>(this IInputDeviceActionBuilder<TInput> builder,
        TInput input, string actionKey)
        where TInput: IInput
        => builder.AssignInput(input, [InputPhase.Active], actionKey);

    public static IInputDeviceActionBuilder<TInput> AssignEndAction<TInput>(this IInputDeviceActionBuilder<TInput> builder, 
        TInput input, string actionKey) 
        where TInput : IInput
        => builder.AssignInput(input, [InputPhase.End], actionKey);

    public static IInputDeviceActionBuilder<TInput> AssignMultiPhaseAction<TInput>(this IInputDeviceActionBuilder<TInput> builder,
        TInput input, string actionKey, params InputPhase[] triggerPhases)
        where TInput : IInput
        => builder.AssignInput(input, triggerPhases, actionKey);

    #endregion
}
