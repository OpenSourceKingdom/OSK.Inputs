using System;
using System.Linq.Expressions;
using OSK.Inputs.Abstractions.Configuration;

namespace OSK.Inputs.Models;

public class InputConfigurationValidationResult
{
    #region Static

    public static InputConfigurationValidationResult Success()
        => new() 
        { 
            ConfigurationType = InputConfigurationType.InputSystem, 
            TargetName = "Configuration",
            Message = "Ok", 
            Result = InputConfigurationValidation.Ok 
        };

    public static InputConfigurationValidationResult ForInputSystem(Expression<Func<InputSystemConfiguration, object?>> propertyPath,
        InputConfigurationValidation validation, string? message = null)
        => ForConfiguration(InputConfigurationType.InputSystem, propertyPath, validation, message);

    public static InputConfigurationValidationResult ForDefinition(Expression<Func<InputDefinition, object?>> propertyPath, 
        InputConfigurationValidation validation, string? message = null)
        => ForConfiguration(InputConfigurationType.Definition, propertyPath, validation, message);

    public static InputConfigurationValidationResult ForInputAction(Expression<Func<InputAction, object?>> propertyPath,
        InputConfigurationValidation validation, string? message = null)
        => ForConfiguration(InputConfigurationType.InputAction, propertyPath, validation, message);

    public static InputConfigurationValidationResult ForScheme(Expression<Func<InputScheme, object?>> propertyPath, 
        InputConfigurationValidation validation, string? message = null)
        => ForConfiguration(InputConfigurationType.Scheme, propertyPath, validation, message);

    public static InputConfigurationValidationResult ForDeviceMap(Expression<Func<DeviceInputMap, object?>> propertyPath,
        InputConfigurationValidation validation, string? message = null)
        => ForConfiguration(InputConfigurationType.DeviceMap, propertyPath, validation, message);

    public static InputConfigurationValidationResult ForJoinPolicy(Expression<Func<InputSystemJoinPolicy, object?>> propertyPath,
        InputConfigurationValidation validation, string? message = null)
        => ForConfiguration(InputConfigurationType.JoinPolicy, propertyPath, validation, message);

    public static InputConfigurationValidationResult ForProcessorConfiguration(Expression<Func<InputProcessorConfiguration, object?>> propertyPath,
        InputConfigurationValidation validation, string? message = null)
        => ForConfiguration(InputConfigurationType.InputProcessor, propertyPath, validation, message);

    private static InputConfigurationValidationResult ForConfiguration<T>(InputConfigurationType configurationType,
        Expression<Func<T, object?>> expression, InputConfigurationValidation validation, string? message = null)
        => new()
        {
            ConfigurationType = configurationType,
            TargetName = GetName(expression),
            Result = validation,
            Message = message ?? string.Empty
        };

    #endregion

    #region Variables

    public bool IsValid => Result is InputConfigurationValidation.Ok;

    public required InputConfigurationType ConfigurationType { get; set; }

    public required string TargetName { get; set; }

    public required InputConfigurationValidation Result { get; set; }

    public string Message { get; set; } = string.Empty;

    #endregion

    #region Helpers

    public override string ToString()
    {
        return $"Validation Error with {ConfigurationType} Configuration for target: {TargetName}. Validation Result: {Result}{Environment.NewLine}Message: {Message}";
    }

    private static string GetName<T>(Expression<Func<T, object?>> expression)
    {
        if (expression.Body is MemberExpression memberExpression)
        {
            return memberExpression.Member.Name;
        }

        // Handle cases where the return type is a value type (int, bool, etc.)
        // which causes the expression body to be wrapped in a 'Convert' unary expression
        if (expression.Body is UnaryExpression unaryExpression &&
            unaryExpression.Operand is MemberExpression innerMember)
        {
            return innerMember.Member.Name;
        }

        throw new ArgumentException("Expression is not a member access", nameof(expression));
    }

    #endregion
}
