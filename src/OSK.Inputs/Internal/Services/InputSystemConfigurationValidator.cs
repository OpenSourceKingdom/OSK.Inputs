using System;
using System.Linq;
using OSK.Inputs.Abstractions.Configuration;
using OSK.Inputs.Models;
using OSK.Inputs.Ports;

namespace OSK.Inputs.Internal.Services;

internal class InputSystemConfigurationValidator : IInputSystemConfigurationValidator
{
    #region IInputSystemConfigurationValidator

    public InputConfigurationValidationResult Validate(InputSystemConfiguration configuration)
    {
        if (configuration is null)
        {
            throw new ArgumentNullException(nameof(configuration));
        }

        var validation = ValidateDefinitions(configuration);
        if (!validation.IsValid)
        {
            return validation;
        }

        validation = ValidateProcessorConfiguration(configuration);
        if (!validation.IsValid)
        {
            return validation;
        }

        return ValidateJoinPolicy(configuration);
    }

    public InputConfigurationValidationResult ValidateCustomScheme(InputSystemConfiguration configuration, CustomInputScheme customScheme,
        bool allowDuplicateCustomScheme)
    {
        if (configuration is null)
        {
            throw new ArgumentNullException(nameof(configuration));
        }
        if (customScheme is null)
        {
            throw new ArgumentNullException(nameof(customScheme));
        }

        if (string.IsNullOrWhiteSpace(customScheme.DefinitionName))
        {
            return InputConfigurationValidationResult.ForScheme(scheme => scheme.Name, InputConfigurationValidation.MissingData,
                "A custom scheme must have an input definition name.");
        }

        var definition = configuration.GetDefinition(customScheme.DefinitionName);
        if (definition is null)
        {
            return InputConfigurationValidationResult.ForDefinition(definition => definition.Name, InputConfigurationValidation.InvalidData,
                $"The custom scheme's definition name {customScheme.DefinitionName} does not exist and can not be used.");
        }

        if (string.IsNullOrWhiteSpace(customScheme.Name))
        {
            return InputConfigurationValidationResult.ForScheme(scheme => scheme.Name, InputConfigurationValidation.MissingData,
                "A custom scheme must have a scheme name.");
        }

        var existingScheme = definition.GetScheme(customScheme.Name);
        if (existingScheme is not null && (!existingScheme.IsCustom || (existingScheme.IsCustom && !allowDuplicateCustomScheme)))
        {
            return InputConfigurationValidationResult.ForScheme(scheme => scheme.Name, InputConfigurationValidation.DuplicateData,
                $"The custom scheme's name {customScheme.Name} already exists on input definition {definition.Name}.");
        }

        if (customScheme.DeviceMaps is null || !customScheme.DeviceMaps.Any())
        {
            return InputConfigurationValidationResult.ForScheme(scheme => scheme.DeviceMaps, InputConfigurationValidation.MissingData,
                $"The custom input scheme {customScheme.Name} on input definition {definition.Name} has no device maps.");
        }

        return ValidateInputScheme(configuration, definition, customScheme.ToInputScheme());
    }

    #endregion

    #region Helpers

    private InputConfigurationValidationResult ValidateJoinPolicy(InputSystemConfiguration configuration)
    {
        if (configuration.JoinPolicy is null)
        {
            return InputConfigurationValidationResult.ForInputSystem(inputSystem => inputSystem.JoinPolicy, InputConfigurationValidation.MissingData,
                "Join Policy must exist.");
        }

        if (configuration.JoinPolicy.MaxUsers < 0)
        {
            return InputConfigurationValidationResult.ForJoinPolicy(policy => policy.MaxUsers, InputConfigurationValidation.InvalidData,
                "Max Users must be greater than 0.");
        }

        return InputConfigurationValidationResult.Success();
    }

    private InputConfigurationValidationResult ValidateProcessorConfiguration(InputSystemConfiguration configuration)
    {
        if (configuration.ProcessorConfiguration is null)
        {
            return InputConfigurationValidationResult.ForInputSystem(inputSystem => inputSystem.ProcessorConfiguration, InputConfigurationValidation.MissingData,
                "Processor configuration must exist.");
        }

        if (configuration.ProcessorConfiguration.TapDelayTime.GetValueOrDefault() < TimeSpan.Zero)
        {
            return InputConfigurationValidationResult.ForProcessorConfiguration(processor => processor.TapDelayTime, InputConfigurationValidation.InvalidData,
                "Tap delay time can not be less than 0.");
        }

        return InputConfigurationValidationResult.Success();
    }

    private InputConfigurationValidationResult ValidateDefinitions(InputSystemConfiguration configuration)
    {
        if (configuration.Definitions is null || !configuration.Definitions.Any())
        {
            return InputConfigurationValidationResult.ForInputSystem(inputSystem => inputSystem.Definitions, InputConfigurationValidation.MissingData,
                "Input Definitions must exist.");
        }

        var definitions = configuration.Definitions;

        var invalidDefinitionNames = definitions.Select(definition => definition?.Name).Where(string.IsNullOrWhiteSpace);
        if (invalidDefinitionNames.Any())
        {
            return InputConfigurationValidationResult.ForDefinition(definition => definition.Name, InputConfigurationValidation.MissingData,
                $"There are {invalidDefinitionNames.Count()} definitions with empty names.");
        }

        // Note: test difficulty - due to how definitions are read-only and provided at construction into a dictionary (i.e duplicate keys throw),
        // it's not entirely feasible this will occur, but validation will be done to ensure if something changes that this is still caught
        var duplicateDefinitionNames = definitions.GroupBy(definition => definition.Name, StringComparer.OrdinalIgnoreCase)
            .Where(definitionGroup => definitionGroup.Count() > 1);
        if (duplicateDefinitionNames.Any())
        {
            return InputConfigurationValidationResult.ForDefinition(definition => definition.Name, InputConfigurationValidation.DuplicateData,
                $"There are {duplicateDefinitionNames.Count()} definitions with duplicate names, the names are: {string.Join(", ", duplicateDefinitionNames.Select(d => d.Key))}");
        }

        var definitionsWithoutSchemes = definitions.Where(definition => definition.Schemes is null || !definition.Schemes.Any())
                                                   .Select(definition => definition.Name);
        if (definitionsWithoutSchemes.Any())
        {
            return InputConfigurationValidationResult.ForDefinition(definition => definition.Schemes, InputConfigurationValidation.MissingData,
                $"There are {definitionsWithoutSchemes.Count()} definitions without input schemes, the names are: {string.Join(", ", definitionsWithoutSchemes)}.");
        }

        var definitionsWithoutActions = definitions.Where(definition => definition.Actions is null || !definition.Actions.Any())
                                                   .Select(definition => definition.Name);
        if (definitionsWithoutActions.Any())
        {
            return InputConfigurationValidationResult.ForDefinition(definition => definition.Actions, InputConfigurationValidation.MissingData,
                $"There are {definitionsWithoutActions.Count()} definitions without input actions, the names are: {string.Join(", ", definitionsWithoutActions)}.");
        }

        foreach (var definition in definitions)
        {
            var validation = ValidateInputActions(definition);
            if (!validation.IsValid)
            {
                return validation;
            }

            validation = ValidateInputSchemes(configuration, definition);
            if (!validation.IsValid)
            {
                return validation; 
            }
        }

        return InputConfigurationValidationResult.Success();
    }

    private InputConfigurationValidationResult ValidateInputActions(InputDefinition definition)
    {
        var actionsWithInvalidNames = definition.Actions.Where(action => string.IsNullOrWhiteSpace(action.Name));
        if (actionsWithInvalidNames.Any())
        {
            return InputConfigurationValidationResult.ForDefinition(definition => definition.Actions, InputConfigurationValidation.MissingData,
                $"There are {actionsWithInvalidNames.Count()} actions with empty names on input definition {definition.Name}.");
        }

        // Note: test difficulty - due to how actions are read-only and provided at construction into a dictionary (i.e duplicate keys throw),
        // it's not entirely feasible this will occur, but validation will be done to ensure if something changes that this is still caught
        var duplicateActionNames = definition.Actions.GroupBy(action => action.Name, StringComparer.OrdinalIgnoreCase)
            .Where(actionGroup => actionGroup.Count() > 1)
            .Select(actionGroup => actionGroup.Key);
        if (duplicateActionNames.Any())
        {
            return InputConfigurationValidationResult.ForDefinition(definition => definition.Actions, InputConfigurationValidation.DuplicateData,
                $"There are {duplicateActionNames.Count()} actions with the same name on input definition {definition.Name}, the names are: {string.Join(", ", duplicateActionNames)}.");
        }

        foreach (var action in definition.Actions)
        {
            if (!action.TriggerPhases.Any())
            {
                return InputConfigurationValidationResult.ForInputAction(action => action.TriggerPhases, InputConfigurationValidation.MissingData,
                    $"There are no input trigger phases for action {action.Name} on input definition {definition.Name}.");
            }
            if (action.ActionExecutor is null)
            {
                return InputConfigurationValidationResult.ForInputAction(action => action.ActionExecutor, InputConfigurationValidation.MissingData,
                    $"There is not action executor for action {action.Name} for definition {definition.Name}.");
            }
        }

        return InputConfigurationValidationResult.Success();
    }

    private InputConfigurationValidationResult ValidateInputSchemes(InputSystemConfiguration configuration, InputDefinition definition)
    {
        var schemesWithInvalidNames = definition.Schemes.Where(scheme => string.IsNullOrWhiteSpace(scheme.Name));
        if (schemesWithInvalidNames.Any())
        {
            return InputConfigurationValidationResult.ForScheme(scheme => scheme.Name, InputConfigurationValidation.MissingData,
                $"There are {schemesWithInvalidNames.Count()} schemes with empty names on input definition {definition.Name}.");
        }

        // Note: test difficulty - due to how schemes are read-only and provided at construction into a dictionary (i.e duplicate keys throw),
        // it's not entirely feasible this will occur, but validation will be done to ensure if something changes that this is still caught
        var duplicateSchemeNames = definition.Schemes.GroupBy(scheme => scheme.Name, StringComparer.OrdinalIgnoreCase)
           .Where(schemeGroup => schemeGroup.Count() > 1)
           .Select(schemeGroup => schemeGroup.Key);
        if (duplicateSchemeNames.Any())
        {
            return InputConfigurationValidationResult.ForDefinition(definition => definition.Schemes, InputConfigurationValidation.DuplicateData,
                $"There are {duplicateSchemeNames.Count()} schemes with the same name on input definition {definition.Name}, the names are: {string.Join(", ", duplicateSchemeNames)}.");
        }

        var schemesMissingDeviceMaps = definition.Schemes.Where(scheme => scheme.DeviceMaps is null || !scheme.DeviceMaps.Any());
        if (schemesMissingDeviceMaps.Any())
        {
            return InputConfigurationValidationResult.ForScheme(scheme => scheme.DeviceMaps, InputConfigurationValidation.MissingData,
                $"There are {schemesMissingDeviceMaps.Count()} schemes on input definition {definition.Name} that have no device maps, the names are: {string.Join(", ", schemesMissingDeviceMaps.Select(scheme => scheme.Name))}.");
        }

        foreach (var scheme in definition.Schemes)
        {
            var validation = ValidateInputScheme(configuration, definition, scheme);
            if (!validation.IsValid)
            {
                return validation;
            }
        }

        return InputConfigurationValidationResult.Success();
    }

    private InputConfigurationValidationResult ValidateInputScheme(InputSystemConfiguration configuration, InputDefinition definition,
        InputScheme scheme)
    {
        // Note: test difficulty - due to how schemes are read-only and provided at construction into a dictionary (i.e duplicate keys throw),
        // it's not entirely feasible this will occur, but validation will be done to ensure if something changes that this is still caught
        var duplicateDeviceMaps = scheme.DeviceMaps.GroupBy(deviceMap => deviceMap.DeviceIdentity)
            .Where(deviceIdentityGroup => deviceIdentityGroup.Count() > 1)
            .Select(deviceIdentityGroup => deviceIdentityGroup.Key);
        if (duplicateDeviceMaps.Any())
        {
            return InputConfigurationValidationResult.ForScheme(scheme => scheme.DeviceMaps, InputConfigurationValidation.DuplicateData,
                $"There are {duplicateDeviceMaps.Count()} device maps on input scheme {scheme.Name} with input definition {definition.Name} with the same device identity, the device maps are: {string.Join(", ", duplicateDeviceMaps)}.");
        }

        var deviceMapsMissingInputMaps = scheme.DeviceMaps.Where(deviceMap => deviceMap.InputMaps is null || !deviceMap.InputMaps.Any())
            .Select(deviceMap => deviceMap.DeviceIdentity);
        if (deviceMapsMissingInputMaps.Any())
        {
            return InputConfigurationValidationResult.ForScheme(scheme => scheme.DeviceMaps, InputConfigurationValidation.MissingData,
                $"There are {deviceMapsMissingInputMaps.Count()} device maps that are missing input maps for scheme {scheme.Name} on input definition {definition.Name}, the device maps are: {string.Join(", ", deviceMapsMissingInputMaps)}.");
        }

        foreach (var deviceMap in scheme.DeviceMaps)
        {
            var deviceMapValidation = ValidateDeviceMap(configuration, definition, scheme, deviceMap);
            if (!deviceMapValidation.IsValid)
            {
                return deviceMapValidation;
            }
        }

        var duplicateActions = scheme.DeviceMaps.SelectMany(map => map.InputMaps)
            .GroupBy(map => map.ActionName)
            .Where(actionMapGroup => actionMapGroup.Count() > 1)
            .Select(actionMapGroup => actionMapGroup.Key);
        if (duplicateActions.Any())
        {
            return InputConfigurationValidationResult.ForScheme(scheme => scheme.DeviceMaps, InputConfigurationValidation.DuplicateData,
                $"There are {duplicateActions.Count()} input maps that share the same action name across the device maps for scheme {scheme.Name} on input definition {definition.Name}, the action names are: {string.Join(", ", duplicateActions)}");
        }

        return InputConfigurationValidationResult.Success();
    }

    private InputConfigurationValidationResult ValidateDeviceMap(InputSystemConfiguration configuration, InputDefinition definition, 
        InputScheme scheme, DeviceInputMap deviceMap)
    {
        var deviceSpecification = configuration.GetDeviceSpecification(deviceMap.DeviceIdentity);
        if (deviceSpecification is null)
        {
            return InputConfigurationValidationResult.ForDeviceMap(map => map.DeviceIdentity, InputConfigurationValidation.InvalidData,
                $"The input scheme {scheme.Name} on inp" +
                $"t defintion {definition.Name} uses a device identity that is not configured for the input system, the device is: {deviceMap.DeviceIdentity}.");
        }

        var validInputIdLookup = deviceSpecification.Inputs.ToDictionary(input => input.Id);
        var invalidInputIds = deviceMap.InputMaps.Where(map => !validInputIdLookup.TryGetValue(map.InputId, out _))
            .Select(map => map.InputId);
        if (invalidInputIds.Any())
        {
            return InputConfigurationValidationResult.ForDeviceMap(map => map.InputMaps, InputConfigurationValidation.InvalidData,
                $"There are {invalidInputIds.Count()} input maps that use input ids that don't exist for the device map {deviceMap.DeviceIdentity} with scheme {scheme.Name} on input definition {definition.Name}, the invalid ids are: {string.Join(",", invalidInputIds.Distinct())}.");
        }

        var duplicateInputIds = deviceMap.InputMaps.GroupBy(map => map.InputId)
            .Where(mapGroup => mapGroup.Count() > 1)
            .Select(mapGroup => mapGroup.Key);
        if (duplicateInputIds.Any()) 
        {
            return InputConfigurationValidationResult.ForDeviceMap(map => map.InputMaps, InputConfigurationValidation.DuplicateData,
                $"There are {duplicateInputIds.Count()} input ids for the device map {deviceMap.DeviceIdentity} with scheme {scheme.Name} on input definition {definition.Name}, the duplicate ids are: {string.Join(", ", duplicateInputIds)}.");
        }

        var inputsMissingActionNames = deviceMap.InputMaps.Where(map => string.IsNullOrWhiteSpace(map.ActionName))
            .Select(map => map.InputId);
        if (inputsMissingActionNames.Any()) 
        {
            return InputConfigurationValidationResult.ForDeviceMap(map => map.InputMaps, InputConfigurationValidation.MissingData,
                $"There are {inputsMissingActionNames.Count()} input maps missing action names for device map {deviceMap.DeviceIdentity} with scheme {scheme.Name} on input definition {definition.Name}, the input map ids are: {string.Join(", ", inputsMissingActionNames)}.");
        }

        var invalidActionNames = deviceMap.InputMaps.Where(map => definition.GetAction(map.ActionName) is null);
        if (invalidActionNames.Any()) 
        {
            return InputConfigurationValidationResult.ForDeviceMap(map => map.InputMaps, InputConfigurationValidation.InvalidData,
                $"There are {invalidActionNames.Count()} input maps with action names that don't exist for device map {deviceMap.DeviceIdentity} with scheme {scheme.Name} on input definition {definition.Name}, the invalid action names are: {string.Join(", ", invalidActionNames.Select(map => map.ActionName).Distinct())}.");
        }

        var duplicateActionNames = deviceMap.InputMaps.GroupBy(map => map.ActionName)
            .Where(mapGroup => mapGroup.Count() > 1)
            .Select(mapGroup => mapGroup.Key);
        if (duplicateActionNames.Any())
        {
            return InputConfigurationValidationResult.ForDeviceMap(map => map.InputMaps, InputConfigurationValidation.DuplicateData,
                $"There are {duplicateActionNames.Count()} duplicate action names for device map {deviceMap.DeviceIdentity} with scheme {scheme.Name} on input defintion {definition.Name}, the action names are: {string.Join(", ", duplicateActionNames)}.");
        }

        return InputConfigurationValidationResult.Success();
    }

    #endregion
}
