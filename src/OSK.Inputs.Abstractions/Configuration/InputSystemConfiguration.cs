using System;
using System.Collections.Generic;
using System.Linq;
using OSK.Inputs.Abstractions.Devices;
using OSK.Inputs.Abstractions.Inputs;

namespace OSK.Inputs.Abstractions.Configuration;

/// <summary>
/// The very necessary configuration utilized with the input system. This is the 'source of truth' for all interactions 
/// and decisions made by the input system
/// </summary>
/// <param name="deviceSpecifications">The devices the input system is able to support</param>
/// <param name="definitions">The input definitions the input system will use to map inputs and actions</param>
/// <param name="processorConfiguration">The configuration for the internal input processing</param>
/// <param name="joinPolicy">The policy the input system uses for new users, devices, and the like</param>
public class InputSystemConfiguration(IEnumerable<InputDeviceSpecification> deviceSpecifications, IEnumerable<InputDefinition> definitions,
    InputProcessorConfiguration processorConfiguration, InputSystemJoinPolicy joinPolicy)
{
    #region Variables

    private readonly Dictionary<InputDeviceFamily, InputDeviceSpecification> _deviceSpecificationLookup 
        = deviceSpecifications?.ToDictionary(specification => specification.DeviceFamily) ?? [];
    private readonly Dictionary<string, InputDefinition> _inputDefinitionLookup 
        = definitions?.Where(definition => definition?.Name is not null).ToDictionary(definition => definition.Name, StringComparer.OrdinalIgnoreCase) ?? [];

    #endregion

    #region Api

    /// <summary>
    /// The configuration options for the <see cref="IInputProcessor"/> to use
    /// </summary>
    public InputProcessorConfiguration ProcessorConfiguration => processorConfiguration;

    /// <summary>
    /// The policy that determines new user, device, etc. behaviors when interacting with the input system
    /// </summary>
    public InputSystemJoinPolicy JoinPolicy => joinPolicy;

    /// <summary>
    /// The collection of supported input device combinations for built in and custom schemes to use.
    /// </summary>
    public IReadOnlyCollection<InputDeviceCombination> SupportedDeviceCombinations { get; }
        = GetUniqueCombinations(definitions ?? []);

    /// <summary>
    /// The collection of input definitions that are available for users
    /// </summary>
    public IReadOnlyCollection<InputDefinition> Definitions
        => _inputDefinitionLookup.Values;

    /// <summary>
    /// Attempts to get a device specification by the device identity
    /// </summary>
    /// <param name="deviceFamily">The identity of the device to get a specification for</param>
    /// <returns>The specific specification for the device identity if it is supported, otherwise null</returns>
    public InputDeviceSpecification? GetDeviceSpecification(InputDeviceFamily deviceFamily)
        => _deviceSpecificationLookup.TryGetValue(deviceFamily, out var specification)
            ? specification
            : null;
    
    /// <summary>
    /// Attempts to get the definition from a name
    /// </summary>
    /// <param name="definitionName">The name of the definition to get</param>
    /// <returns>The definition if the name matches an existing definition in the input system, otherwise null</returns>
    public InputDefinition? GetDefinition(string definitionName)
        => !string.IsNullOrWhiteSpace(definitionName) && _inputDefinitionLookup.TryGetValue(definitionName, out var definition)
            ? definition
            : null;

    /// <summary>
    /// Attempts to get an action map for the provided definition and scheme that can be used to trigger configured actions at runtime
    /// </summary>
    /// <param name="inputDefinitionName">The definition name that references one configured with the input system</param>
    /// <param name="schemeName">A scheme name that should be configured with the associated input definition</param>
    /// <returns>An action map that combines the input scheme input maps and input definition's actions, if the names match existing configured items, otherwise null</returns>
    public InputSchemeActionMap? GetSchemeMap(string inputDefinitionName, string schemeName)
    {
        var definition = GetDefinition(inputDefinitionName);
        if (definition is null)
        {
            return null;
        }

        var scheme = definition.GetScheme(schemeName);
        if (scheme is null)
        {
            return null;
        }

        var deviceMaps = scheme.DeviceMaps.Where(deviceMap => _deviceSpecificationLookup.TryGetValue(deviceMap.DeviceFamily, out _))
                .Select(deviceMap =>
                {
                    var actionMaps = _deviceSpecificationLookup[deviceMap.DeviceFamily].GetInputs().Select(input =>
                    {
                        var inputMap = deviceMap.GetInputMap(input.Id);
                        var action = inputMap is null
                            ? null
                            : definition.GetAction(inputMap.Value.ActionName);

                    return inputMap is null || action is null
                            ? null
                            : GetActionMap(input, inputMap.Value, action);
                    }).Where(inputMap => inputMap is not null).Cast<InputActionMap>() ?? [];

                    return new DeviceSchemeActionMap(deviceMap.DeviceFamily, actionMaps);
                });

        return new InputSchemeActionMap(deviceMaps);
    }

    /// <summary>
    /// Attempts to apply a set of custom configured schemes to the configuration
    /// </summary>
    /// <param name="customSchemes">The list of schemes to apply</param>
    public void ApplyCustomInputSchemes(IEnumerable<CustomInputScheme> customSchemes)
    {
        if (customSchemes is null)
        {
            return;
        }

        foreach (var customScheme in customSchemes)
        {
            var inputDefinition = GetDefinition(customScheme.DefinitionName);
            if (inputDefinition is null)
            {
                continue;
            }

            inputDefinition.ApplyCustomScheme(customScheme);
        }
    } 

    /// <summary>
    /// Resets the input configuration by removing any additional applied settings (like custom schemes) that were set
    /// during runtime. This set the configuration back to the state it was when the configuration was created.
    /// </summary>
    public void Reset()
    {
        foreach (var definition in _inputDefinitionLookup.Values)
        {
            definition.ResetDefinition();
        }
    }

    #endregion

    #region Helpers

    private InputActionMap GetActionMap(IInput input, InputMap map, InputAction action)
    {
        return new InputActionMap()
        {
            Input = input,
            Action = action,
            LinkedInputIds = input is DeviceCombinationInput combinationInput
                ? [.. combinationInput.DeviceInputs.Select(i => i.Id)]
                : []
        };
    }

    /// <summary>
    /// Determines the list of device combinations the configuration supports, based on the input schemes provided
    /// </summary>
    /// <param name="definitions">The input definitions the configuration will use</param>
    /// <returns>The unique list of device combinations supported by the schemes</returns>
    private static IReadOnlyCollection<InputDeviceCombination> GetUniqueCombinations(IEnumerable<InputDefinition> definitions)
    {
        // Make sure to create combinations from non custom schemes, otherwise we might potentially
        // support a scheme that was not intended by a developer
        var controllerSchemeGroups = definitions.SelectMany(definition => definition.Schemes)
            .Where(scheme => !scheme.IsCustom)
            .Select(scheme => new
            {
                ControllerName = string.Join(".", scheme.DeviceMaps.Select(map => map.DeviceFamily)),
                Devices = scheme.DeviceMaps.Select(map => map.DeviceFamily)
            })
            .GroupBy(controllerScheme => controllerScheme.ControllerName);

        var controllers = new Dictionary<string, InputDeviceCombination>();
        foreach (var controllerSchemeGroup in controllerSchemeGroups)
        {
            var scheme = controllerSchemeGroup.First();

            var deviceList = scheme.Devices.ToArray();

            // Attempts to create a display name for the combination that is more readable for a combination:
            // "Keyboard", "Keyboard and Mouse", etc.
            var displayName = deviceList.Length switch
            {
                0 => string.Empty,
                1 => deviceList[0].Name,
                2 => $"{deviceList[0]} and {deviceList[1]}",
                _ => $"{string.Join(", ", deviceList.Take(deviceList.Length - 1))}, and {deviceList[^1]}"
            };

            controllers[controllerSchemeGroup.Key] = new InputDeviceCombination(displayName, deviceList);
        }

        return controllers.Values;
    }

    #endregion
}
