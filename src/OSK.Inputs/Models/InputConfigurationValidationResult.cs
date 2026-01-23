using System;
using System.Linq.Expressions;
using OSK.Inputs.Abstractions.Configuration;

namespace OSK.Inputs.Models;

/// <summary>
/// A result from validating an input configuration. This is used to describe validation issues with various parts of an <see cref="InputSystemConfiguration"/>
/// </summary>
public class InputConfigurationValidationResult
{
    #region Static

    /// <summary>
    /// Creates a validation result that represents a successful validation.
    /// </summary>
    /// <returns></returns>
    public static InputConfigurationValidationResult Success()
        => new() 
        { 
            ConfigurationType = InputConfigurationType.InputSystem, 
            TargetName = "Configuration",
            Message = "Ok", 
            Result = InputConfigurationValidation.Ok 
        };

    /// <summary>
    /// Create a validation result that is specific to the input system configuration, targeting a specific property that is on the <see cref="InputSystemConfiguration"/>/>
    /// </summary>
    /// <param name="propertyPath">The path to the property that will be the target of the validation result</param>
    /// <param name="validation">The specific validation to set for the target</param>
    /// <param name="message">A unique message that describes the reason for the validation</param>
    /// <returns>A validation result tied to the input system property</returns>
    public static InputConfigurationValidationResult ForInputSystem(Expression<Func<InputSystemConfiguration, object?>> propertyPath,
        InputConfigurationValidation validation, string? message = null)
        => ForConfiguration(InputConfigurationType.InputSystem, propertyPath, validation, message);

    /// <summary>
    /// Create a validation result that is specific to the input definition, targeting a specific property that is on the <see cref="InputDefinition"/>/>
    /// </summary>
    /// <param name="propertyPath">The path to the property that will be the target of the validation result</param>
    /// <param name="validation">The specific validation to set for the target</param>
    /// <param name="message">A unique message that describes the reason for the validation</param>
    /// <returns>A validation result tied to the input definition property</returns>
    public static InputConfigurationValidationResult ForDefinition(Expression<Func<InputDefinition, object?>> propertyPath, 
        InputConfigurationValidation validation, string? message = null)
        => ForConfiguration(InputConfigurationType.Definition, propertyPath, validation, message);

    /// <summary>
    /// Create a validation result that is specific to the input action, targeting a specific property that is on the <see cref="InputAction"/>/>
    /// </summary>
    /// <param name="propertyPath">The path to the property that will be the target of the validation result</param>
    /// <param name="validation">The specific validation to set for the target</param>
    /// <param name="message">A unique message that describes the reason for the validation</param>
    /// <returns>A validation result tied to the input action property</returns>
    public static InputConfigurationValidationResult ForInputAction(Expression<Func<InputAction, object?>> propertyPath,
        InputConfigurationValidation validation, string? message = null)
        => ForConfiguration(InputConfigurationType.InputAction, propertyPath, validation, message);

    /// <summary>
    /// Create a validation result that is specific to the input scheme, targeting a specific property that is on the <see cref="InputScheme"/>/>
    /// </summary>
    /// <param name="propertyPath">The path to the property that will be the target of the validation result</param>
    /// <param name="validation">The specific validation to set for the target</param>
    /// <param name="message">A unique message that describes the reason for the validation</param>
    /// <returns>A validation result tied to the input scheme property</returns>
    public static InputConfigurationValidationResult ForScheme(Expression<Func<InputScheme, object?>> propertyPath, 
        InputConfigurationValidation validation, string? message = null)
        => ForConfiguration(InputConfigurationType.Scheme, propertyPath, validation, message);

    /// <summary>
    /// Create a validation result that is specific to the device input map, targeting a specific property that is on the <see cref="DeviceInputMap"/>/>
    /// </summary>
    /// <param name="propertyPath">The path to the property that will be the target of the validation result</param>
    /// <param name="validation">The specific validation to set for the target</param>
    /// <param name="message">A unique message that describes the reason for the validation</param>
    /// <returns>A validation result tied to the device input map property</returns>
    public static InputConfigurationValidationResult ForDeviceMap(Expression<Func<DeviceInputMap, object?>> propertyPath,
        InputConfigurationValidation validation, string? message = null)
        => ForConfiguration(InputConfigurationType.DeviceMap, propertyPath, validation, message);

    /// <summary>
    /// Create a validation result that is specific to the join policy, targeting a specific property that is on the <see cref="InputSystemJoinPolicy"/>/>
    /// </summary>
    /// <param name="propertyPath">The path to the property that will be the target of the validation result</param>
    /// <param name="validation">The specific validation to set for the target</param>
    /// <param name="message">A unique message that describes the reason for the validation</param>
    /// <returns>A validation result tied to the join policy property</returns>
    public static InputConfigurationValidationResult ForJoinPolicy(Expression<Func<InputSystemJoinPolicy, object?>> propertyPath,
        InputConfigurationValidation validation, string? message = null)
        => ForConfiguration(InputConfigurationType.JoinPolicy, propertyPath, validation, message);

    /// <summary>
    /// Create a validation result that is specific to the input processor configuration, targeting a specific property that is on the <see cref="InputProcessorConfiguration"/>/>
    /// </summary>
    /// <param name="propertyPath">The path to the property that will be the target of the validation result</param>
    /// <param name="validation">The specific validation to set for the target</param>
    /// <param name="message">A unique message that describes the reason for the validation</param>
    /// <returns>A validation result tied to the input processor configuration</returns>
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

    /// <summary>
    /// Indicates if the result represents a valid response, if false then there was a validation issue
    /// </summary>
    public bool IsValid => Result is InputConfigurationValidation.Ok;

    /// <summary>
    /// The type of configuration that the validation result is applied to
    /// </summary>
    public required InputConfigurationType ConfigurationType { get; set; }

    /// <summary>
    /// The target of the validation
    /// </summary>
    public required string TargetName { get; set; }

    /// <summary>
    /// The validation result for the target
    /// </summary>
    public required InputConfigurationValidation Result { get; set; }

    /// <summary>
    /// A unique message that describes the reason for the validation result
    /// </summary>
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
