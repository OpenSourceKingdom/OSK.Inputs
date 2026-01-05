using System;
using System.Linq.Expressions;
using OSK.Extensions.Inputs.Configuration.Ports;
using OSK.Inputs.Abstractions.Devices;
using OSK.Inputs.Abstractions.Inputs;

namespace OSK.Extensions.Inputs.Configuration;

public static class InputDeviceMapBuilderExtensions
{
    public static IInputDeviceMapBuilder<TDeviceSpecification, TInput> WithInput<TDeviceSpecification, TInput, TService>(
        this IInputDeviceMapBuilder<TDeviceSpecification, TInput> builder, TInput input, Expression<Func<TService, object?>> methodPath)
        where TInput : Enum
        where TDeviceSpecification : InputDeviceSpecification<TInput>, new()
    {
        return builder.WithInputMap(input, GetMethodName(methodPath));
    }

    public static IInputDeviceMapBuilder<TDeviceSpecification, TInput> WithInput<TDeviceSpecification, TInput>(
        this IInputDeviceMapBuilder<TDeviceSpecification, TInput> builder, TInput input, string actionKey)
        where TInput : Enum
        where TDeviceSpecification : InputDeviceSpecification<TInput>, new()
    {
        return builder.WithInputMap(input, actionKey);
    }

    #region Helpers

    private static string GetMethodName<T>(Expression<Func<T, object?>> expression)
    {
        Expression body = expression.Body;

        // Handle boxing/unboxing if the method returns a value type cast to object
        if (body is UnaryExpression unary)
        {
            body = unary.Operand;
        }

        if (body is MethodCallExpression method)
        {
            return method.Method.Name;
        }

        // Also handle MemberExpression in case they pass a property that acts as an action
        if (body is MemberExpression member)
        {
            return member.Member.Name;
        }

        throw new ArgumentException("Expression must be a method call (e.g., s => s.Jump())", nameof(expression));
    }

    #endregion
}
