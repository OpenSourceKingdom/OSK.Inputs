using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OSK.Functions.Outputs.Abstractions;
using OSK.Inputs.Models.Configuration;
using OSK.Inputs.Models.Runtime;
using OSK.Inputs.Ports;

namespace OSK.Inputs.Internal.Services;
internal class InMemoryInputSchemeRepository(IOutputFactory outputFactory) : IInputSchemeRepository
{
    #region Variables

    internal readonly Dictionary<string, Dictionary<string, Dictionary<string, InputScheme>>> _customInputSchemes = [];
    internal readonly Dictionary<int, Dictionary<string, Dictionary<string, ActiveInputScheme>>> _activeSchemes = [];

    #endregion

    #region IInputSchemeRepository

    public Task<IOutput<IEnumerable<InputScheme>>> GetCustomInputSchemesAsync(string inputDefinitionName, CancellationToken cancellationToken = default)
    {
        var customSchemes = _customInputSchemes.TryGetValue(inputDefinitionName, out var customDeviceSchemeLookup)
            ? customDeviceSchemeLookup.Values.SelectMany(deviceSchemeGroupLookup => deviceSchemeGroupLookup.Values)
            : [];
        return Task.FromResult(outputFactory.Succeed(customSchemes));
    }

    public Task<IOutput<InputScheme>> GetCustomInputSchemeAsync(string inputDefinitionName, InputDeviceName deviceName,
        string inputSchemeName, CancellationToken cancellationToken = default)
    {
        if (!_customInputSchemes.TryGetValue(inputDefinitionName, out var customDeviceSchemeLookup))
        {
            return Task.FromResult(outputFactory.NotFound<InputScheme>($"No custom schemes were found for input definition {inputDefinitionName}"));
        }
        if (!customDeviceSchemeLookup.TryGetValue(deviceName.Name, out var customSchemeLookup))
        {
            return Task.FromResult(outputFactory.NotFound<InputScheme>($"No custom schemes were found for the {deviceName} controller using the {inputDefinitionName} input defintiion"));
        }

        return !customSchemeLookup.TryGetValue(inputSchemeName, out var inputScheme)
            ? Task.FromResult(outputFactory.NotFound<InputScheme>($"The custom scheme {inputSchemeName} was not found for the {deviceName} controller using the {inputDefinitionName} input definition"))
            : Task.FromResult(outputFactory.Succeed(inputScheme));
    }

    public Task<IOutput<InputScheme>> SaveCustomInputSchemeAsync(string inputDefinitionName, InputScheme inputScheme, CancellationToken cancellationToken = default)
    {
        if (inputScheme is null)
        {
            throw new ArgumentNullException(nameof(inputScheme));
        }

        if (!_customInputSchemes.TryGetValue(inputDefinitionName, out var customDeviceSchemeLookup))
        {
            customDeviceSchemeLookup = [];
            _customInputSchemes[inputDefinitionName] = customDeviceSchemeLookup;
        }
        if (!customDeviceSchemeLookup.TryGetValue(inputScheme.DeviceName.Name, out var customSchemeLookup))
        {
            customSchemeLookup = [];
            customDeviceSchemeLookup[inputScheme.DeviceName.Name] = customSchemeLookup;
        }

        customSchemeLookup[inputScheme.SchemeName] = inputScheme;
        return Task.FromResult(outputFactory.Succeed(inputScheme));
    }

    public Task<IOutput> DeleteCustomInputSchemeAsync(string inputDefinitionName, InputDeviceName deviceName,
        string inputSchemeName, CancellationToken cancellationToken = default)
    {
        if (!_customInputSchemes.TryGetValue(inputDefinitionName, out var customDeviceSchemeLookup))
        {
            return Task.FromResult(outputFactory.Succeed());
        }
        if (!customDeviceSchemeLookup.TryGetValue(deviceName.Name, out var customSchemeLookup))
        {
            return Task.FromResult(outputFactory.Succeed());
        }
        
        customSchemeLookup.Remove(inputSchemeName);
        return Task.FromResult(outputFactory.Succeed());
    }

    public Task<IOutput<IEnumerable<ActiveInputScheme>>> GetActiveInputSchemesAsync(int userId, string inputDefinitionName, CancellationToken cancellationToken = default)
    {
        var activeSchemes = Enumerable.Empty<ActiveInputScheme>();
        if (_activeSchemes.TryGetValue(userId, out var activeUserSchemes)
             && activeUserSchemes.TryGetValue(inputDefinitionName, out var activeDeviceSchemes))
        {
            activeSchemes = activeDeviceSchemes.Values;
        }

        return Task.FromResult(outputFactory.Succeed(activeSchemes));
    }

    public Task<IOutput<ActiveInputScheme>> SaveActiveInputSchemeAsync(ActiveInputScheme inputScheme, CancellationToken cancellationToken = default)
    {
        if (inputScheme is null)
        {
            throw new ArgumentNullException(nameof(inputScheme));
        }

        if (!_activeSchemes.TryGetValue(inputScheme.UserId, out var activeUserSchemes))
        {
            activeUserSchemes = [];
            _activeSchemes.Add(inputScheme.UserId, activeUserSchemes);
        }
        if (!activeUserSchemes.TryGetValue(inputScheme.InputDefinitionName, out var activeSchemes))
        {
            activeSchemes = [];
            activeUserSchemes.Add(inputScheme.InputDefinitionName, activeSchemes);
        }

        activeSchemes[inputScheme.DeviceName] = inputScheme;
        return Task.FromResult(outputFactory.Succeed(inputScheme));
    }

    public Task<IOutput> DeleteActiveInputSchemeAsync(int userId, string inputDefinitionId, InputDeviceName deviceName, CancellationToken cancellationToken = default)
    {
        if (_activeSchemes.TryGetValue(userId, out var activeUserSchemes)
            && activeUserSchemes.TryGetValue(inputDefinitionId, out var schemeLookup))
        {
            schemeLookup.Remove(deviceName.Name);
        }

        return Task.FromResult(outputFactory.Succeed());
    }

    #endregion
}
