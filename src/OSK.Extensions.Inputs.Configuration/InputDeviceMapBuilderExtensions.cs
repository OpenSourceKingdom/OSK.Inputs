using System;
using System.Linq.Expressions;
using OSK.Extensions.Inputs.Configuration.Ports;
using OSK.Inputs.Abstractions.Inputs;

namespace OSK.Extensions.Inputs.Configuration;

public static class InputDeviceMapBuilderExtensions
{
    extension(IInputDeviceMapBuilder builder)
    {
        public IInputDeviceMapBuilder WithInput<TInput, TService>(TInput input, Expression<Func<TService, object?>> methodPath)
            where TInput : Input
            => builder.WithInputMap(input.Id, GetMethodName(methodPath));

        public IInputDeviceMapBuilder WithInput<TInput>(TInput input, string actionKey)
            where TInput : Input
            => builder.WithInputMap(input.Id, actionKey);

        #region Helpers

        private static string GetMethodName<T>(Expression<Func<T, object?>> expression)
        {
            Expression body = expression.Body;
            if (body is UnaryExpression unary)
            {
                body = unary.Operand;
            }

            if (body is MethodCallExpression method)
            {
                return method.Method.Name;
            }

            throw new ArgumentException("Expression is not a member access", nameof(expression));
        }

        #endregion
    }
}
