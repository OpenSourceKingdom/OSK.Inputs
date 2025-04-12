using System;
using System.Collections.Generic;
using System.Linq;
using OSK.Inputs.Models.Configuration;
using OSK.Inputs.Ports;

namespace OSK.Inputs.Internal.Services;

internal class InputValidationService : IInputValidationService
{
    #region Variables

    public const string InputSystemConfigurationError = "_inputSysConfiguration";
    public const string InputDefinitionError = "_inputDefinition";
    public const string InputSchemeError = "_inputScheme";
    public const string InputControllerError = "_inputController";
    public const string InputActionError = "_inputAction";
    public const string InputActionMapError = "_inputMap";
        
    /// <summary>
    /// Represents a child object not referencing the same id as the parent object it is attached to.
    /// </summary>
    public const int ValidationError_MismatchedTenant = 400;

    /// <summary>
    /// The identifying property for an object is null or empty
    /// </summary>
    public const int ValidationError_MissingIdentifier = 401;

    /// <summary>
    /// The identifier for an object was found on another object associated to the parent
    /// </summary>
    public const int ValidationError_DuplicateIdentifier = 402;

    /// <summary>
    /// The data for a collection property is null or empty
    /// </summary>
    public const int ValidationError_CollectionMissingData = 403;

    /// <summary>
    /// Data for a property was in an invalid state
    /// </summary>
    public const int ValidationError_InvalidData = 405;

    #endregion

    #region IInputValidationService

    public InputValidationContext ValidateInputSystemConfiguration(InputSystemConfiguration inputSystemConfiguration)
    {
        if (inputSystemConfiguration is null)
        {
            throw new ArgumentNullException(nameof(inputSystemConfiguration));
        }

        var inputControllersValidationContext = ValidateInputControllers(inputSystemConfiguration.SupportedInputControllers);
        if (inputControllersValidationContext.Errors.Any())
        {
            return inputControllersValidationContext;
        }

        var inputDefinitionsValidationContext = ValidateInputDefinitions(inputSystemConfiguration.InputDefinitions, inputSystemConfiguration.SupportedInputControllers);
        if (inputDefinitionsValidationContext.Errors.Any())
        {
            return inputDefinitionsValidationContext;
        }

        if (inputSystemConfiguration.MaxLocalUsers < 1)
        {
            return InputValidationContext.Error(InputSystemConfigurationError, ValidationError_InvalidData, "Max local users can not be less than 1.");
        }

        return InputValidationContext.Success;
    }

    public InputValidationContext ValidateCustomInputScheme(InputSystemConfiguration inputSystemConfiguration, InputScheme inputScheme)
    {
        if (inputSystemConfiguration is null)
        {
            throw new ArgumentNullException(nameof(inputSystemConfiguration));
        }
        if (inputScheme is null)
        {
            throw new ArgumentNullException(nameof(inputScheme));
        }

        return ValidateInputScheme(inputScheme, true, inputSystemConfiguration.InputDefinitions, inputSystemConfiguration.SupportedInputControllers);        
    }

    #endregion

    #region Helpers

    private InputValidationContext ValidateInputDefinitions(IReadOnlyCollection<InputDefinition> inputDefinitions,
        IReadOnlyCollection<IInputDeviceConfiguration> supportedInputControllerConfigurations)
    {
        if (inputDefinitions is null || !inputDefinitions.Any())
        {
            return InputValidationContext.Error(InputDefinitionError, ValidationError_CollectionMissingData, "Input System configuration has no input definitions and is unusable.");
        }

        if (inputDefinitions.Any(definition => string.IsNullOrWhiteSpace(definition.Name)))
        {
            return InputValidationContext.Error(InputDefinitionError, ValidationError_MissingIdentifier, "One or more input definitions had an empty name.");
        }

        var duplicateDefinitionNames = inputDefinitions.GroupBy(definition => definition.Name).Where(group => group.Count() > 1);
        if (duplicateDefinitionNames.Any())
        {
            var error = string.Join(", ", duplicateDefinitionNames.Select(g => g.Key));
            return InputValidationContext.Error(InputDefinitionError, ValidationError_DuplicateIdentifier, $"One or more input definitions share the same name: {error}");
        }

        foreach (var inputDefinition in inputDefinitions)
        {
            var context = ValidateInputDefinition(inputDefinition, supportedInputControllerConfigurations);
            if (context.Errors.Any())
            {
                return context;
            }
        }

        return InputValidationContext.Success;
    }

    private InputValidationContext ValidateInputDefinition(InputDefinition inputDefinition, IEnumerable<IInputDeviceConfiguration> supportedInputControllerConfigurations)
    {
        if (!inputDefinition.InputActions.Any())
        {
            return InputValidationContext.Error(InputDefinitionError, ValidationError_CollectionMissingData, $"Input definition {inputDefinition.Name} has no actions and can not be used.");
        }

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
            var errorMessage = string.Join("\n", invalidActionKeys.Select(key => $"{key}"));
            return InputValidationContext.Error(InputDefinitionError, ValidationError_InvalidData, errorMessage);
        }

        foreach (var action in inputDefinition.InputActions)
        {
            var inputActionValidation = ValidateInputAction(action);
            if (inputActionValidation.Errors.Any())
            {
                return inputActionValidation;
            }
        }

        InputDefinition[] inputDefinitions = [inputDefinition];
        foreach (var inputScheme in inputDefinition.InputSchemes)
        {
            var inputSchemeValidationContext = ValidateInputScheme(inputScheme, false, inputDefinitions, supportedInputControllerConfigurations);
            if (inputSchemeValidationContext.Errors.Any())
            {
                return inputSchemeValidationContext;
            }
        }

        return InputValidationContext.Success;
    }

    private InputValidationContext ValidateInputAction(InputAction inputAction)
    {
        return InputValidationContext.Success;
    }

    private InputValidationContext ValidateInputControllers(IReadOnlyCollection<IInputDeviceConfiguration> supportedControllerConfigurations)
    {
        if (supportedControllerConfigurations is null || !supportedControllerConfigurations.Any())
        {
            return InputValidationContext.Error(InputControllerError, ValidationError_CollectionMissingData, "Input System configuration has no input controller configurations and is unusable.");
        }
        if (supportedControllerConfigurations.Any(controllerConfiguration => string.IsNullOrWhiteSpace(controllerConfiguration.ControllerName.Name)))
        {
            return InputValidationContext.Error(InputControllerError, ValidationError_MissingIdentifier, "One or more input controllers had an empty name.");
        }

        var duplicateControllerNames = supportedControllerConfigurations.GroupBy(controller => controller.ControllerName).Where(group => group.Count() > 1);
        if (duplicateControllerNames.Any())
        {
            var error = string.Join(", ", duplicateControllerNames.Select(g => g.Key));
            return InputValidationContext.Error(InputControllerError, ValidationError_DuplicateIdentifier, $"One or more input controllers share the same name: {error}");
        }

        foreach (var controllerConfiguration in supportedControllerConfigurations)
        {
            var validationContext = ValidateInputController(controllerConfiguration);
            if (validationContext.Errors.Any())
            {
                return validationContext;
            }
        }

        return InputValidationContext.Success;
    }

    private InputValidationContext ValidateInputController(IInputDeviceConfiguration controllerConfiguration)
    {
        var context = new InputValidationContext(InputControllerError);

        if (controllerConfiguration.InputReaderType is null)
        {
            context.AddErrors(ValidationError_InvalidData, $"The {controllerConfiguration.ControllerName} controller had a null input reader type and is unusable.");
        }
        else if (!typeof(IInputReader).IsAssignableFrom(controllerConfiguration.InputReaderType))
        {
            context.AddErrors(ValidationError_InvalidData,
                $"The input controller {controllerConfiguration.ControllerName}'s input reader type, {controllerConfiguration.InputReaderType.FullName}, does not implement the {nameof(IInputReader)} interface.");
        }

        if (controllerConfiguration.Inputs is null || !controllerConfiguration.Inputs.Any())
        {
            context.AddErrors(ValidationError_CollectionMissingData, $"The {controllerConfiguration.ControllerName} has no inputs and is unusable.");
        }
        else if (controllerConfiguration.Inputs.Any(input => string.IsNullOrWhiteSpace(input.Name)))
        {
            context.AddErrors(ValidationError_InvalidData, $"The input controller {controllerConfiguration.ControllerName} bas inputs with empty names.");
        }

        return context;
    }

    private InputValidationContext ValidateInputScheme(InputScheme inputScheme, bool isNewScheme, IEnumerable<InputDefinition> inputDefinitions, 
        IEnumerable<IInputDeviceConfiguration> supportedControllers)
    {
        if (string.IsNullOrWhiteSpace(inputScheme.InputDefinitionName))
        {
            return InputValidationContext.Error(InputSchemeError, ValidationError_InvalidData, "Input scheme's definition name can not be empty.");
        }

        var inputDefinition = inputDefinitions.FirstOrDefaultByString(definition => definition.Name, inputScheme.InputDefinitionName);
        if (inputDefinition is null)
        {
            return InputValidationContext.Error(InputSchemeError, ValidationError_MismatchedTenant, $"The Input Scheme references a definition {inputScheme.InputDefinitionName} that was not added to the input system.");
        }

        if (string.IsNullOrWhiteSpace(inputScheme.ControllerName.Name))
        {
            return InputValidationContext.Error(InputSchemeError, ValidationError_InvalidData, "Controller name can not be empty.");
        }

        var inputControllerConfiguration = supportedControllers.FirstOrDefaultByString(controller => controller.ControllerName.Name, inputScheme.ControllerName.Name);
        if (inputControllerConfiguration is null)
        {
            return InputValidationContext.Error(InputSchemeError, ValidationError_InvalidData,
                $"Input definition {inputDefinition.Name} does not support the {inputScheme.ControllerName} controller");
        }

        if (string.IsNullOrWhiteSpace(inputScheme.SchemeName))
        {
            return InputValidationContext.Error(InputSchemeError, ValidationError_MissingIdentifier, "Input scheme name can not be empty.");
        }

        if (isNewScheme && inputDefinition.InputSchemes.AnyByString(scheme => scheme.ControllerName.Name, inputScheme.ControllerName.Name))
        {
            return InputValidationContext.Error(InputSchemeError, ValidationError_DuplicateIdentifier, $"Input scheme with name {inputScheme.SchemeName} has already been added to the input definition {inputDefinition.Name}.");
        }

        var actionMapValidationContext = ValidateInpurSchemeActionMaps(inputScheme);
        if (actionMapValidationContext.Errors.Any())
        {
            return actionMapValidationContext;
        }

        var currentActionKeys = new HashSet<string>(inputScheme.InputActionMaps.Select(inputMap => inputMap.ActionKey));
        var missingInputActions = inputDefinition.InputActions.Where(inputAction => !currentActionKeys.Contains(inputAction.ActionKey));
        if (missingInputActions.Any())
        {
            var error = string.Join("\n", missingInputActions.Select(action => action.ActionKey));
            return InputValidationContext.Error(InputSchemeError, ValidationError_CollectionMissingData, $"The input scheme {inputScheme.SchemeName} is missing the following action keys:\n{error}");
        }

        return InputValidationContext.Success;
    }

    private InputValidationContext ValidateInpurSchemeActionMaps(InputScheme scheme)
    {
        var context = new InputValidationContext(InputActionMapError);
        var invalidInputKeys = scheme.InputActionMaps.GroupBy(map => map.InputKey)
            .Select(group => new
            {
                InputKey = group.Key,
                IsDuplicate = group.Count() > 1
            })
            .Where(group => group.IsDuplicate)
            .Select(group =>
            {
                return $"The input id {group.InputKey} already exists on the input scheme {scheme.SchemeName}";
            });
        if (invalidInputKeys.Any())
        {
            var errorMessage = string.Join("\n", invalidInputKeys.Select(key => $"{key}"));
            context.AddErrors(ValidationError_InvalidData, errorMessage);
        }

        var invalidActionKeys = scheme.InputActionMaps.GroupBy(map => map.ActionKey, StringComparer.OrdinalIgnoreCase)            
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
                    ? $"An input key for input scheme  {scheme.SchemeName} had an empty name"
                    : $"The input action key {group.ActionKey} already exists on the input scheme {scheme.SchemeName}";
            });
        if (invalidActionKeys.Any())
        {
            var errorMessage = string.Join("\n", invalidActionKeys.Select(key => $"{key}"));
            context.AddErrors(ValidationError_InvalidData, errorMessage);
        }

        return context;
    }

    #endregion
}
