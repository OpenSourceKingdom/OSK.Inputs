using System;
using OSK.Inputs.Internal;
using OSK.Inputs.Models.Inputs;
using OSK.Inputs.Ports;

namespace OSK.Inputs;

public static class InputControllerBuilderExtensions
{
    #region Input Receivers

    public static IInputControllerBuilder AddInputReceiver<TInputReceiver, TInput>(this IInputControllerBuilder builder, string name) 
        where TInputReceiver : IInputReceiver
        where TInput: IInput
    {
        builder.AddInputReceiver(new InputReceiverDescriptor(name, typeof(TInputReceiver), input => input is IInput));

        return builder;
    }   

    public static IInputControllerBuilder AddInputReceiver<TInputReceiver>(this IInputControllerBuilder builder, string name, Func<IInput, bool> validator)
        where TInputReceiver : IInputReceiver
    {
        builder.AddInputReceiver(new InputReceiverDescriptor(name, typeof(TInputReceiver), validator));

        return builder;
    }

    #endregion
}
