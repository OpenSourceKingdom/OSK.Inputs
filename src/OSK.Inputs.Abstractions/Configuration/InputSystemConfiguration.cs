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
    /// 
    /// <returns>An action map that combines the input scheme input maps and input definition's actions, if the names match existing configured items, otherwise null</returns>
    public InputSchemeActionMap? GetSchemeMap(string definitionName, string combinationId, string schemeName)
    {
        var definition = GetDefinition(definitionName);
        if (definition is null)
        {
            return null;
        }

        var scheme = definition.GetScheme(combinationId, schemeName);
        if (scheme is null)
        {
            return null;
        }

        var deviceMaps = scheme.DeviceMaps.Where(deviceMap => _deviceSpecificationLookup.TryGetValue(deviceMap.DeviceFamily, out _))
                .Select(deviceMap =>
                {
                    var virtualInputMaps = deviceMap.VirtualMaps.Select(virtualMap =>
                    {
                        if (virtualMap.Input.GetLinkedInputs().OfType<DeviceInput>().Any(input => deviceMap.GetInputMap(input.Id) is null))
                        {
                            return null;
                        }

                        var action = definition.GetAction(virtualMap.ActionName);

                        return action is null
                            ? null
                            : new InputActionMap()
                            {
                                Action = action,
                                Input = virtualMap.Input
                            };
                    }).Where(inputMap => inputMap is not null).Cast<InputActionMap>();

                    var activeDeviceInputMaps = _deviceSpecificationLookup[deviceMap.DeviceFamily].GetInputs().Select(input =>
                    {
                        var inputMap = deviceMap.GetInputMap(input.Id);
                        var action = inputMap is null || inputMap.Value.IsPassive
                            ? null
                            : definition.GetAction(inputMap.Value.ActionName);
                        return inputMap is null || (action is null && !inputMap.Value.IsPassive)
                            ? null
                            : new InputActionMap()
                            {
                                Input = input,
                                Action = action
                            };
                    }).Where(inputMap => inputMap is not null).Cast<InputActionMap>();

                    // Make passive inputs of any device input that a virtual map needs that isn't already used for an active input map
                    // This is so we can track the data for it even if it doesn't directly trigger an action - the virtual map does
                    var passiveDeviceInputs = virtualInputMaps.SelectMany(virtualMap =>
                    {
                        var deviceInputs = ((VirtualInput)virtualMap.Input).GetLinkedInputs().OfType<DeviceInput>();

                        return deviceInputs.Where(input => !activeDeviceInputMaps.Any(map => ((DeviceInput)map.Input).Id == input.Id))
                            .Select(missingInput => new InputActionMap() { Action = null, Input = missingInput });
                    });

                    return new DeviceSchemeActionMap(deviceMap.DeviceFamily, activeDeviceInputMaps.Concat(passiveDeviceInputs).Concat(virtualInputMaps));
                });

        return new(definitionName, schemeName, deviceMaps);
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

    /// <summary>
    /// Determines the list of device combinations the configuration supports, based on the input schemes provided
    /// </summary>
    /// <param name="definitions">The input definitions the configuration will use</param>
    /// <returns>The unique list of device combinations supported by the schemes</returns>
    private static IReadOnlyCollection<InputDeviceCombination> GetUniqueCombinations(IEnumerable<InputDefinition> definitions)
    {
        // Make sure to create combinations from non custom schemes, otherwise we might potentially
        // support a scheme that was not intended by a developer
        var schemeCombinations = definitions.SelectMany(definition => definition.Schemes)
            .Where(scheme => !scheme.IsCustom)
            .Select(scheme => new
            {
                CombinationId = InputDeviceCombination.GetCombinationId(scheme.GetDeviceFamilies()),
                Devices = scheme.DeviceMaps.Select(map => map.DeviceFamily)
            })
            .GroupBy(combination => combination.CombinationId);

        var deviceCombinations = new Dictionary<string, InputDeviceCombination>();
        foreach (var combination in schemeCombinations)
        {
            var scheme = combination.First();
            deviceCombinations[combination.Key] = new InputDeviceCombination(scheme.Devices);
        }

        return deviceCombinations.Values;
    }

    #endregion
}
