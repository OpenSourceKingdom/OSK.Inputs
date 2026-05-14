using System;
using OSK.Extensions.Inputs.Configuration.Ports;
using OSK.Inputs.Abstractions.Inputs;

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
    /// Adds an input stream using a strongly typed enum
    /// </summary>
    /// <typeparam name="TInputStream">The type of input stream object</typeparam>
    /// <typeparam name="TInput">The input enum</typeparam>
    /// <param name="builder">The builder to configure</param>
    /// <param name="input">The enum value</param>
    /// <returns>The builder for chaining</returns>
    public static IInputDeviceMapBuilder WithInputStream<TInputStream, TInput>(
        this IInputDeviceMapBuilder builder, TInput input)
        where TInputStream : InputStream
        where TInput : Enum
    {
        return builder.WithInputStream<TInputStream>(Convert.ToInt32(input));
    }

    #endregion
}
