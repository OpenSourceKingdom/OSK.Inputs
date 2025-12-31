using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace OSK.Inputs.Abstractions.Configuration;

public class InputSystemConfiguration(IEnumerable<InputDeviceSpecification> deviceDescriptors, IEnumerable<InputDefinition> definitions,
    InputProcessorConfiguration processorConfiguration, JoinPolicy joinPolicy)
{
    #region Variables

    private readonly Dictionary<InputDeviceIdentity, InputDeviceSpecification> _deviceDescriptorLookup = deviceDescriptors.ToDictionary(descriptor => descriptor.DeviceIdentity);
    private readonly Dictionary<string, InputDefinition> _inputDefinitionLookup = definitions.ToDictionary(definition => definition.Name);

    #endregion

    #region Api

    public InputProcessorConfiguration ProcessorConfiguration => processorConfiguration;

    public JoinPolicy JoinPolicy => joinPolicy;

    public IReadOnlyCollection<InputDeviceCombination> SupportedDeviceCombinations { get; }
        = GetUniqueCombinations(definitions);

    public IReadOnlyCollection<InputDefinition> Definitions
        => _inputDefinitionLookup.Values;

    public InputDeviceSpecification? GetDeviceDescriptor(InputDeviceIdentity deviceName)
        => _deviceDescriptorLookup.TryGetValue(deviceName, out var descriptor)
            ? descriptor
            : null;
    
    public InputDefinition? GetDefinition(string definitionName)
        => !string.IsNullOrWhiteSpace(definitionName) && _inputDefinitionLookup.TryGetValue(definitionName, out var definition)
            ? definition
            : null;

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

        var deviceMaps = scheme.DeviceMaps.Where(deviceMap => _deviceDescriptorLookup.TryGetValue(deviceMap.DeviceIdentity, out _))
                .Select(deviceMap =>
                {
                    var requiredInputs = _deviceDescriptorLookup[deviceMap.DeviceIdentity].Inputs.Select(input =>
                    {
                        var inputMap = deviceMap.GetInputMap(input.Id);
                        var action = inputMap is null
                            ? null
                            : definition.GetAction(inputMap.Value.ActionName);

                    return inputMap is null || action is null
                            ? null
                            : new InputActionMap() { Input = input, Map = inputMap.Value, Action = action };
                    }).Where(inputMap => inputMap is not null).Cast<InputActionMap>() ?? [];

                    return new DeviceSchemeActionMap(deviceMap.DeviceIdentity, requiredInputs);
                });

        return new InputSchemeActionMap(scheme.VirtualInputs, deviceMaps);
    }

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

    public void Reset()
    {
        foreach (var definition in _inputDefinitionLookup.Values)
        {
            definition.ResetDefinition();
        }
    }

    #endregion

    #region Helpers

    private static IReadOnlyCollection<InputDeviceCombination> GetUniqueCombinations(IEnumerable<InputDefinition> definitions)
    {
        var controllerSchemeGroups = definitions.SelectMany(definition => definition.Schemes)
            .Select(scheme => new
            {
                ControllerName = string.Join(".", scheme.DeviceMaps.Select(map => map.DeviceIdentity)),
                Devices = scheme.DeviceMaps.Select(map => map.DeviceIdentity)
            })
            .GroupBy(controllerScheme => controllerScheme.ControllerName);

        var controllers = new Dictionary<string, InputDeviceCombination>();
        foreach (var controllerSchemeGroup in controllerSchemeGroups)
        {
            var scheme = controllerSchemeGroup.First();

            var deviceList = scheme.Devices.ToArray();
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
