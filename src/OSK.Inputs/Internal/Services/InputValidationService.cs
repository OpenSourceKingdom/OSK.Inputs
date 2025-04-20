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
    public const string InputDeviceError = "_inputDevice";
    public const string InputActionError = "_inputAction";
    public const string InputActionMapError = "_inputMap";
    public const string InputControllerError = "_controllerConfiguration";
        
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

        var inputDeviceValidationContext = ValidateInputDevices(inputSystemConfiguration.SupportedInputDevices);
        if (inputDeviceValidationContext.Errors.Any())
        {
            return inputDeviceValidationContext;
        }

        var inputControllersValidationContext = ValidateInputControllerConfigurations(inputSystemConfiguration.InputControllers,
            inputSystemConfiguration.SupportedInputDevices);
        if (inputControllersValidationContext.Errors.Any())
        {
            return inputControllersValidationContext;
        }

        var inputDefinitionsValidationContext = ValidateInputDefinitions(inputSystemConfiguration.InputDefinitions, inputSystemConfiguration.InputControllers, inputSystemConfiguration.SupportedInputDevices);
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

        return ValidateInputScheme(inputScheme, true, inputSystemConfiguration.InputDefinitions, inputSystemConfiguration.InputControllers, 
            inputSystemConfiguration.SupportedInputDevices);        
    }

    #endregion

    #region Helpers

    private InputValidationContext ValidateInputDefinitions(IReadOnlyCollection<InputDefinition> inputDefinitions,
        IReadOnlyCollection<InputControllerConfiguration> controllerConfigurations,
        IReadOnlyCollection<IInputDeviceConfiguration> supportedInputDeviceConfigurations)
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
            var context = ValidateInputDefinition(inputDefinition, controllerConfigurations, supportedInputDeviceConfigurations);
            if (context.Errors.Any())
            {
                return context;
            }
        }

        return InputValidationContext.Success;
    }

    private InputValidationContext ValidateInputDefinition(InputDefinition inputDefinition, IEnumerable<InputControllerConfiguration> controllerConfigurations, 
        IEnumerable<IInputDeviceConfiguration> supportedInputDeviceConfigurations)
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
            var errorMessage = string.Join($"{Environment.NewLine}", invalidActionKeys.Select(key => $"{key}"));
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
            var inputSchemeValidationContext = ValidateInputScheme(inputScheme, false, inputDefinitions, controllerConfigurations, 
                supportedInputDeviceConfigurations);
            if (inputSchemeValidationContext.Errors.Any())
            {
                return inputSchemeValidationContext;
            }
        }

        return InputValidationContext.Success;
    }

    private InputValidationContext ValidateInputAction(InputAction inputAction)
    {
        if (inputAction.ActionExecutor is null)
        {
            return InputValidationContext.Error(InputActionError, ValidationError_InvalidData, $"Input action {inputAction.ActionKey} does not have a valid action executor and is unusable.");
        }

        return InputValidationContext.Success;
    }

    private InputValidationContext ValidateInputControllerConfigurations(IReadOnlyCollection<InputControllerConfiguration> controllerConfigurations,
        IReadOnlyCollection<IInputDeviceConfiguration> inputDevices)
    {
        var invalidControllerConfigurations = controllerConfigurations.GroupBy(controllerConfig => controllerConfig.ControllerName)
            .Select(controllgerConfigGroup =>
            {
                return new
                {
                    ControllerName = controllgerConfigGroup.Key,
                    IsDuplicate = controllgerConfigGroup.Count() > 1,
                    IsEmpty = string.IsNullOrEmpty(controllgerConfigGroup.Key)
                };
            });

        var invalidNames = invalidControllerConfigurations.Where(group => group.IsEmpty);
        if (invalidNames.Any())
        {
            return InputValidationContext.Error(InputControllerError, ValidationError_MissingIdentifier, "One or more controller configurations had an empty name.");
        }

        var duplicateNames = invalidControllerConfigurations.Where(group => group.IsDuplicate);
        if (duplicateNames.Any())
        {
            var error = string.Join(Environment.NewLine, duplicateNames.Select(group => group.ControllerName));
            return InputValidationContext.Error(InputControllerError, ValidationError_DuplicateIdentifier, $"The following input controllers were invalid because the names were already added to the input system.{Environment.NewLine}{error}");
        }

        var invalidDeviceConfigurations = controllerConfigurations.Select(controllerConfiguration => new
        {
            ConfigurationName = controllerConfiguration.ControllerName,
            InvalidDevices = controllerConfiguration.DeviceNames.Where(deviceName => !inputDevices.Any(device => device.DeviceName == deviceName))
        });

        if (invalidDeviceConfigurations.Any())
        {
            var error = string.Join(Environment.NewLine, invalidDeviceConfigurations.Select(config => $"Input controller {config.ConfigurationName} had one or more unsupported device names: {string.Join(", ", config.InvalidDevices)}"));
            return InputValidationContext.Error(InputControllerError, ValidationError_InvalidData, $"One or more input controllers had unsupported device names:{Environment.NewLine}{error}");
        }

        var duplicateDeviceConfigurations = controllerConfigurations.SelectMany(config => config.DeviceNames).GroupBy(deviceName => deviceName)
            .Where(deviceGroup => deviceGroup.Count() > 1);
        if (duplicateDeviceConfigurations.Any())
        {
            var error = string.Join(",", duplicateDeviceConfigurations.Select(configGroup => configGroup.Key));
            return InputValidationContext.Error(InputControllerError, ValidationError_DuplicateIdentifier, $"The following device names were duplicated among controllers: {error}");
        }

        return InputValidationContext.Success;
    }

    private InputValidationContext ValidateInputDevices(IReadOnlyCollection<IInputDeviceConfiguration> supportedDeviceConfigurations)
    {
        if (supportedDeviceConfigurations is null || !supportedDeviceConfigurations.Any())
        {
            return InputValidationContext.Error(InputDeviceError, ValidationError_CollectionMissingData, "Input System configuration has no input device configurations and is unusable.");
        }
        if (supportedDeviceConfigurations.Any(deviceConfiguration => string.IsNullOrWhiteSpace(deviceConfiguration.DeviceName.Name)))
        {
            return InputValidationContext.Error(InputDeviceError, ValidationError_MissingIdentifier, "One or more input devices had an empty name.");
        }

        var duplicateDeviceNames = supportedDeviceConfigurations.GroupBy(device => device.DeviceName).Where(group => group.Count() > 1);
        if (duplicateDeviceNames.Any())
        {
            var error = string.Join(", ", duplicateDeviceNames.Select(g => g.Key));
            return InputValidationContext.Error(InputDeviceError, ValidationError_DuplicateIdentifier, $"One or more input devices share the same name: {error}");
        }

        foreach (var deviceConfiguration in supportedDeviceConfigurations)
        {
            var validationContext = ValidateInputDevice(deviceConfiguration);
            if (validationContext.Errors.Any())
            {
                return validationContext;
            }
        }

        return InputValidationContext.Success;
    }

    private InputValidationContext ValidateInputDevice(IInputDeviceConfiguration deviceConfiguration)
    {
        var context = new InputValidationContext(InputDeviceError);

        if (deviceConfiguration.InputReaderType is null)
        {
            context.AddErrors(ValidationError_InvalidData, $"The {deviceConfiguration.DeviceName} device had a null input reader type and is unusable.");
        }
        else if (!typeof(IInputReader).IsAssignableFrom(deviceConfiguration.InputReaderType))
        {
            context.AddErrors(ValidationError_InvalidData,
                $"The input device {deviceConfiguration.DeviceName}'s input reader type, {deviceConfiguration.InputReaderType.FullName}, does not implement the {nameof(IInputReader)} interface.");
        }

        if (deviceConfiguration.Inputs is null || !deviceConfiguration.Inputs.Any())
        {
            context.AddErrors(ValidationError_CollectionMissingData, $"The {deviceConfiguration.DeviceName} has no inputs and is unusable.");
        }
        else if (deviceConfiguration.Inputs.Any(input => string.IsNullOrWhiteSpace(input.Name)))
        {
            context.AddErrors(ValidationError_InvalidData, $"The input device {deviceConfiguration.DeviceName} bas inputs with empty names.");
        }

        return context;
    }

    private InputValidationContext ValidateInputScheme(InputScheme inputScheme, bool isNewScheme, IEnumerable<InputDefinition> inputDefinitions,
        IEnumerable<InputControllerConfiguration> controllerConfigurations, IEnumerable<IInputDeviceConfiguration> supportedDevices)
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

        if (string.IsNullOrWhiteSpace(inputScheme.ControllerId))
        {
            return InputValidationContext.Error(InputSchemeError, ValidationError_InvalidData, "Input scheme's controller id can not be empty.");
        }
        if (controllerConfigurations.FirstOrDefaultByString(controller => controller.ControllerName, inputScheme.ControllerId) is null)
        {
            return InputValidationContext.Error(InputSchemeError, ValidationError_InvalidData, "Input controller id is not listed as a supported controller configuration.");
        }

        var duplicateDeviceNames = inputScheme.DeviceActionMaps.GroupBy(device => device.DeviceName)
            .Where(deviceGroup => deviceGroup.Count() > 1);
        if (duplicateDeviceNames.Any())
        {
            var error = string.Join(", ", duplicateDeviceNames);
            return InputValidationContext.Error(InputSchemeError, ValidationError_DuplicateIdentifier, $"The {inputScheme.Name} for input definition {inputScheme.InputDefinitionName} had the following duplicate input devices and is invalid: {error}");
        }

        var invalidDevices = inputScheme.DeviceActionMaps
            .Where(inputSchemeDevice => supportedDevices.FirstOrDefaultByString(supportedDevice => supportedDevice.DeviceName.Name, inputSchemeDevice.DeviceName.Name) is null);
        if (invalidDevices.Any())
        {
            var error = string.Join(", ", invalidDevices.Select(device => device.DeviceName));

            return InputValidationContext.Error(InputSchemeError, ValidationError_InvalidData,
                $"The input system was not configured to support the following input devices that the {inputScheme.Name} on definition {inputScheme.InputDefinitionName} is using: {error}{Environment.NewLine}Consider adding support to the DI container.");
        }

        if (string.IsNullOrWhiteSpace(inputScheme.Name))
        {
            return InputValidationContext.Error(InputSchemeError, ValidationError_MissingIdentifier, "Input scheme name can not be empty.");
        }
        if (isNewScheme && inputDefinition.InputSchemes.AnyByString(scheme => scheme.Name, inputScheme.Name))
        {
            return InputValidationContext.Error(InputSchemeError, ValidationError_DuplicateIdentifier, $"Input scheme with name {inputScheme.Name} has already been added to the input definition {inputDefinition.Name}.");
        }

        var actionMapValidationContext = ValidateInputSchemeActionMaps(inputScheme);
        if (actionMapValidationContext.Errors.Any())
        {
            return actionMapValidationContext;
        }

        var currentActionKeys = new HashSet<string>(inputScheme.DeviceActionMaps.SelectMany(deviceMaps => deviceMaps.InputActionMaps).Select(inputMap => inputMap.ActionKey));
        var missingInputActions = inputDefinition.InputActions.Where(inputAction => !currentActionKeys.Contains(inputAction.ActionKey));
        if (missingInputActions.Any())
        {
            var error = string.Join($"{Environment.NewLine}", missingInputActions.Select(action => action.ActionKey));
            return InputValidationContext.Error(InputSchemeError, ValidationError_CollectionMissingData, $"The input scheme {inputScheme.Name} is missing the following action keys:{Environment.NewLine}{error}");
        }

        return InputValidationContext.Success;
    }

    private InputValidationContext ValidateInputSchemeActionMaps(InputScheme scheme)
    {
        var context = new InputValidationContext(InputActionMapError);
        var invalidDeviceMaps = scheme.DeviceActionMaps.Select(deviceMap =>
        {
            var invalidInputIds = deviceMap.InputActionMaps.GroupBy(map => map.InputId)
            .Select(group => new
            {
                InputId = group.Key,
                IsDuplicate = group.Count() > 1
            })
            .Where(group => group.IsDuplicate);

            var invalidInputActionKeys = deviceMap.InputActionMaps.GroupBy(map => map.ActionKey, StringComparer.OrdinalIgnoreCase)
            .Select(group => new
            {
                ActionKey = group.Key,
                IsEmptyName = string.IsNullOrWhiteSpace(group.Key),
                IsDuplicateName = group.Count() > 1
            })
            .Where(group => group.IsDuplicateName || group.IsEmptyName);

            return new
            {
                deviceMap.DeviceName,
                InvalidInputIds = invalidInputIds,
                InvalidInputActionKeys = invalidInputActionKeys
            };
        })
        .Where(deviceMapValidation => deviceMapValidation.InvalidInputIds.Any() || deviceMapValidation.InvalidInputActionKeys.Any())
        .ToArray();

        var invalidIds = invalidDeviceMaps.Where(deviceMapValidation => deviceMapValidation.InvalidInputIds.Any())
            .Select(mapValidation =>
            {
                var error = string.Join(", ", mapValidation.InvalidInputIds.Select(input => input.InputId));
                return $"The input device {mapValidation.DeviceName} had an invalid mapping configuration. The following input ids were duplicated: {error}";
            });
        if (invalidIds.Any())
        {
            var errorMessage = string.Join($"{Environment.NewLine}", invalidIds);
            context.AddErrors(ValidationError_InvalidData, $"The input scheme {scheme.Name} on input definition {scheme.InputDefinitionName} had an invalid mapping configuration:{Environment.NewLine}{errorMessage}");
        }

        var invalidActionKeys = invalidDeviceMaps.Where(deviceMapValidation => deviceMapValidation.InvalidInputActionKeys.Any())
            .Select(deviceMapValidation =>
            {
                var duplicateActionKeys = string.Join(", ", deviceMapValidation.InvalidInputActionKeys.Select(group => group.ActionKey));
                var hasEmptyName = deviceMapValidation.InvalidInputActionKeys.Any(group => group.IsEmptyName);

                var error = $"The input device {deviceMapValidation.DeviceName} had an invalid action map configuration.";
                if (hasEmptyName)
                {
                    error += " There was one or more action keys that had an empty name.";
                }
                if (duplicateActionKeys.Any())
                {
                    error += $" The following action keys were duplicated on the map: {duplicateActionKeys}";
                }

                return error;
            });
        if (invalidActionKeys.Any())
        {
            var errorMessage = string.Join($"{Environment.NewLine}", invalidActionKeys.Select(key => $"{key}"));
            context.AddErrors(ValidationError_InvalidData, $"The input scheme {scheme.Name} on input definition {scheme.InputDefinitionName} had an invalid mapping configuration.{Environment.NewLine}{errorMessage}");
        }

        return context;
    }

    #endregion
}
