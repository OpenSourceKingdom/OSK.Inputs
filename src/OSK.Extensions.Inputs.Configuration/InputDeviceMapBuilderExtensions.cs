using System;
using OSK.Extensions.Inputs.Configuration.Ports;
using OSK.Inputs.Abstractions.Devices;
using OSK.Inputs.Abstractions.Devices.Keyboards;

namespace OSK.Extensions.Inputs.Configuration;

public static class InputDeviceMapBuilderExtensions
{
    #region General

    /// <summary>
    /// Adds an input map using a strongly typed enum
    /// </summary>
    /// <typeparam name="TInput">The input enum</typeparam>
    /// <param name="builder">The builder to configure</param>
    /// <param name="input">The enum value</param>
    /// <param name="actionName">The action the input will trigger</param>
    /// <returns>The builder for chaining</returns>
    public static IInputDeviceMapBuilder WithInput<TInput>(
        this IInputDeviceMapBuilder builder, TInput input, string actionName)
        where TInput : Enum
    {
        return builder.WithInputMap(Convert.ToInt32(input), actionName);
    }

    /// <summary>
    /// Adds a passive input map using a strongly typed enum
    /// </summary>
    /// <typeparam name="TInput">The input enum</typeparam>
    /// <param name="builder">The builder to configure</param>
    /// <param name="input">The enum value</param>
    /// <returns>The builder for chaining</returns>
    public static IInputDeviceMapBuilder WithPassiveInput<TInput>(
        this IInputDeviceMapBuilder builder, TInput input)
        where TInput : Enum
    {
        return builder.WithPassiveInput(Convert.ToInt32(input));
    }

    #endregion

    #region Keyboard

    /// <summary>
    /// Adds a keyboard combination virtual input
    /// </summary>
    /// <param name="builder">The builder to configure</param>
    /// <param name="actionName">The action the input will trigger</param>
    /// <param name="keyboardKeys">The various keys the combination uses</param>
    /// <returns>The builder for chaining</returns>
    public static IInputDeviceMapBuilder WithKeyboardCombination(this IInputDeviceMapBuilder builder,
        string actionName, params KeyboardInput[] keyboardKeys)
        => builder.WithVirtualInput(new KeyboardCombination(keyboardKeys), actionName);

    #endregion
}
