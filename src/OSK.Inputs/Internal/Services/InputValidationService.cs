using System;
using System.Collections.Generic;
using System.Linq;
using OSK.Inputs.Models;
using OSK.Inputs.Models.Configuration;
using OSK.Inputs.Ports;

namespace OSK.Inputs.Internal.Services;

internal class InputValidationService : IInputValidationService
{
    #region Variables

    public const string InputDefinitionError = "_inputDefinition";
    public const string InputSchemeError = "_inputScheme";
    public const string InputReceiverError = "_inputReceiver";
    public const string InputControllerError = "_inputController";
    public const string InputReceiverDescriptorError = "_inputDescriptorError";
    public const string InputActionError = "_inputAction";
    public const string InputActionMapError = "_inputMap";
        
    public const int ValidationError_MismatchedTenant = 400;
    public const int ValidationError_MissingIdentifier = 401;
    public const int ValidationError_DuplicateIdentifier = 402;
    public const int ValidationError_MissingData = 403;
    public const int ValidationError_MissingData2 = 404;
    public const int ValidationError_InvalidData = 405;
    public const int ValidationError_InvalidData2 = 406;

    #endregion

    #region IInputValidationService

    public InputValidationContext ValidateInputDefinition(InputDefinition inputDefinition)
    {
        if (inputDefinition is null)
        {
            throw new ArgumentNullException(nameof(inputDefinition));
        }
        
        var validationContext = new InputValidationContext();
        if (string.IsNullOrWhiteSpace(inputDefinition.Name))
        {
            validationContext.AddError(InputDefinitionError, ValidationError_MissingIdentifier, "Input definition name can not be empty.");
        }

        if (!inputDefinition.InputActions.Any())
        {
            validationContext.AddError(InputDefinitionError, ValidationError_MissingData, $"Input definition {inputDefinition.Name} has no actions and can not be used.");
        }
        else
        {
             var invalidActionKeys = inputDefinition.InputActions
                .GroupBy(action => action.ActionKey, StringComparer.OrdinalIgnoreCase)
                .Select(group => new
                {
                    ActionKey = group.Key,
                    IsEmptyName = string.IsNullOrWhiteSpace(group.Key),
                    IsDuplicateName = group.Count() > 1
                })
                .Where(group => group.IsDuplicateName || group.IsEmptyName)
                .Select(group =>
                {
                    return group.IsEmptyName
                        ? $"An action key had an empty name for the input definition {inputDefinition.Name}"
                        : $"The action key {group.ActionKey} already exists on the input definition {inputDefinition.Name}";
                });
            if (invalidActionKeys.Any())
            {
                validationContext.AddAggregateError(InputDefinitionError, ValidationError_InvalidData, invalidActionKeys);
            }

            foreach (var action in inputDefinition.InputActions)
            {
                ValidateInputAction(validationContext, action);
            }
        }

        if (!inputDefinition.DefaultControllerConfigurations.Any())
        {
            validationContext.AddError(InputDefinitionError, ValidationError_MissingData2,
                $"Input definition {inputDefinition.Name} has no controllers and can not be used.");
        }
        else
        {
            var invalidControllerNames = inputDefinition.DefaultControllerConfigurations
                .GroupBy(controller => controller.ControllerName, StringComparer.OrdinalIgnoreCase)
                .Select(group => new
                {
                    ControllerName = group.Key,
                    IsEmptyName = string.IsNullOrWhiteSpace(group.Key),
                    IsDuplicateName = group.Count() > 1
                })
                .Where(group => group.IsDuplicateName || group.IsEmptyName)
                .Select(group =>
                {
                    return group.IsEmptyName
                        ? "An input controller had an empty name"
                        : $"The controller name {group.ControllerName} already exists on the input definition {inputDefinition.Name}";
                });

            if (invalidControllerNames.Any())
            {
                validationContext.AddAggregateError(InputDefinitionError, ValidationError_InvalidData2, invalidControllerNames);
            }

            foreach (var controller in inputDefinition.DefaultControllerConfigurations)
            {
                ValidateInputController(validationContext, inputDefinition, controller);
            }
        }

        return validationContext;
    }

    public InputValidationContext ValidateInputScheme(InputDefinition inputDefinition, InputScheme inputScheme)
    {
        if (inputDefinition is null)
        {
            throw new ArgumentNullException(nameof(inputDefinition));
        }
        if (inputScheme is null)
        {
            throw new ArgumentNullException(nameof(inputScheme));
        }

        var validationContext = new InputValidationContext();

        if (string.IsNullOrWhiteSpace(inputScheme.InputDefinitionName))
        {
            validationContext.AddError(InputSchemeError, ValidationError_MissingData, "Input scheme's definition name can not be empty.");
            return validationContext;
        }
        else if (!string.Equals(inputScheme.InputDefinitionName, inputDefinition.Name, StringComparison.Ordinal))
        {
            validationContext.AddError(InputSchemeError, ValidationError_MismatchedTenant, 
                $"Input Scheme's definition id {inputScheme.InputDefinitionName} does not match the definition it's being associated, which is {inputDefinition.Name}.");
        
            return validationContext;
        }

        ValidateInputScheme(validationContext, inputDefinition, inputScheme);        
        return validationContext;
    }

    #endregion

    #region Helpers

    private void ValidateInputScheme(InputValidationContext context, InputDefinition inputDefinition, InputScheme inputScheme)
    {
        if (string.IsNullOrWhiteSpace(inputScheme.ControllerName))
        {
            context.AddError(InputSchemeError, ValidationError_MissingData, "Controller name can not be empty.");
            return;
        }

        if (!inputDefinition.DefaultControllerConfigurations.AnyByString(controller => controller.ControllerName, inputScheme.ControllerName))
        {
            context.AddError(InputSchemeError, ValidationError_InvalidData,
                $"Input definition {inputDefinition.Name} does not support the {inputScheme.ControllerName} controller");
            return;
        }

        if (string.IsNullOrWhiteSpace(inputScheme.SchemeName))
        {
            context.AddError(InputSchemeError, ValidationError_MissingIdentifier, "Input scheme name can not be empty.");
        }

        ValidateReceiverConfigurations(context, inputDefinition, inputScheme);
    }

    private void ValidateInputAction(InputValidationContext context, InputAction inputAction)
    {
    }

    private void ValidateInputController(InputValidationContext context, InputDefinition inputDefinition, InputControllerConfiguration controller)
    {
        var invalidReceiverDescriptorNames = controller.ReceiverDescriptors
            .GroupBy(descriptor => descriptor.ReceiverName, StringComparer.OrdinalIgnoreCase)
            .Select(group => new
            {
                DescriptorName = group.Key,
                IsEmptyName = string.IsNullOrWhiteSpace(group.Key),
                IsDuplicateName = group.Count() > 1
            })
            .Where(group => group.IsDuplicateName || group.IsEmptyName)
            .Select(group =>
            {
                return group.IsEmptyName
                    ? $"An input descriptor had an empty name for controller {controller.ControllerName}"
                    : $"The input descriptor name {group.DescriptorName} already exists on the input controller {controller.ControllerName}";
            });

        if (invalidReceiverDescriptorNames.Any())
        {
            context.AddAggregateError(InputControllerError, ValidationError_InvalidData, invalidReceiverDescriptorNames);
        }

        foreach (var descriptor in controller.ReceiverDescriptors)
        {
            ValidateInputReceiverDescription(context, controller, descriptor);
        }
        foreach (var scheme in controller.InputSchemes)
        {
            ValidateInputScheme(context, inputDefinition, scheme);
        }
    }

    private void ValidateReceiverConfigurations(InputValidationContext context, InputDefinition inputDefinition, InputScheme scheme)
    {
        var invalidReceiverConfigurationNames = scheme.ReceiverConfigurations
            .GroupBy(receiver => receiver.InputReceiverName, StringComparer.OrdinalIgnoreCase)
            .Select(group => new
            {
                InputReceiverName = group.Key,
                IsEmptyName = string.IsNullOrWhiteSpace(group.Key),
                IsDuplicateName = group.Count() > 1
            })
            .Where(group => group.IsDuplicateName || group.IsEmptyName)
            .Select(group => 
            {
                return group.IsEmptyName
                    ? $"An input receiver for {group.InputReceiverName} had an empty name"
                    : $"The input receiver {group.InputReceiverName} already exists on the input definition {inputDefinition.Name} {scheme.SchemeName} scheme";
            });
        if (invalidReceiverConfigurationNames.Any())
        {
            context.AddAggregateError(InputReceiverError, ValidationError_InvalidData, invalidReceiverConfigurationNames);
        }

        foreach (var receiverConfiguration in scheme.ReceiverConfigurations)
        {
            ValidateActionMaps(context, receiverConfiguration);
        }

        var currentActionKeys = new HashSet<string>(
            scheme.ReceiverConfigurations.SelectMany(configuration => configuration.InputMaps)
            .Select(inputMap => inputMap.ActionKey));
        var missingInputActions = inputDefinition.InputActions.Where(inputAction => !currentActionKeys.Contains(inputAction.ActionKey));
        if (missingInputActions.Any())
        {
            var error = string.Join("\n", missingInputActions.Select(action => action.ActionKey));
            context.AddError(InputReceiverError, ValidationError_MissingData, $"The input scheme {scheme.SchemeName} is missing the following action keys:\n{error}");
        }
    }

    private void ValidateActionMaps(InputValidationContext context, InputReceiverConfiguration configuration)
    {
        var invalidInputKeys = configuration.InputMaps.GroupBy(map => map.InputKey, StringComparer.OrdinalIgnoreCase)
            .Select(group => new
            {
                InputKey = group.Key,
                IsEmptyName = string.IsNullOrWhiteSpace(group.Key),
                IsDuplicateName = group.Count() > 1
            })
            .Where(group => group.IsDuplicateName || group.IsEmptyName)
            .Select(group =>
            {
                return group.IsEmptyName
                    ? $"An input key for input receiver {configuration.InputReceiverName} had an empty name"
                    : $"The input key {group.InputKey} already exists on the input receiver {configuration.InputReceiverName}";
            });
        if (invalidInputKeys.Any())
        {
            var errorMessage = string.Join("\n", invalidInputKeys.Select(key => $"{key}"));
            context.AddAggregateError(InputActionMapError, ValidationError_InvalidData, invalidInputKeys);
        }

        var invalidActionKeys = configuration.InputMaps.GroupBy(map => map.ActionKey, StringComparer.OrdinalIgnoreCase)            
            .Select(group => new
            {
                ActionKey = group.Key,
                IsEmptyName = string.IsNullOrWhiteSpace(group.Key),
                IsDuplicateName = group.Count() > 1
            })
            .Where(group => group.IsDuplicateName || group.IsEmptyName)
            .Select(group =>
            {
                return group.IsEmptyName
                    ? $"An input action key for input receiver {configuration.InputReceiverName} had an empty name"
                    : $"The input action key {group.ActionKey} already exists on the input receiver {configuration.InputReceiverName}";
            });
        if (invalidActionKeys.Any())
        {
            context.AddAggregateError(InputActionMapError, ValidationError_InvalidData2, invalidInputKeys);
        }
    }

    private void ValidateInputReceiverDescription(InputValidationContext context, InputControllerConfiguration controller, IInputReceiverDescriptor description)
    {
        var invalidReceiverDescriptorTypes = controller.ReceiverDescriptors
            .Where(descriptor => !typeof(IInputSystem).IsAssignableFrom(descriptor.InputSystemType))
            .Select(descriptor => $"The input descriptor {descriptor.ReceiverName}'s receiver type, {descriptor.InputSystemType.FullName}, does not implement the {nameof(IInputSystem)} interface.");

        if (invalidReceiverDescriptorTypes.Any())
        {
            context.AddAggregateError(InputReceiverDescriptorError, ValidationError_InvalidData2 , invalidReceiverDescriptorTypes);
        }
    }

    #endregion
}
