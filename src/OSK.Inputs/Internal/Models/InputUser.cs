using System;
using System.Collections.Generic;
using System.Linq;
using OSK.Inputs.Abstractions;
using OSK.Inputs.Abstractions.Runtime;

namespace OSK.Inputs.Internal.Models;

internal class InputUser(int id): IInputUser
{
    #region Variables

    private Dictionary<int, PairedDevice> _pairedDevices = [];
    private Dictionary<string, Dictionary<string, PreferredInputScheme>> _preferredSchemeLookup = [];

    #endregion

    #region Constructors

    internal InputUser(int id, Dictionary<int, PairedDevice> pairedDevices)
        : this(id)
    {
        _pairedDevices = pairedDevices;
    }

    #endregion

    #region IApplicationUser

    public int Id => id;

    public string ActiveInputDefinitionName { get; internal set; } = string.Empty;

    public PreferredInputScheme? GetPreferredInputScheme(string definitionName, string combinationId)
        => string.IsNullOrWhiteSpace(definitionName) || string.IsNullOrWhiteSpace(combinationId)
            || !(_preferredSchemeLookup.TryGetValue(definitionName, out var definitionSchemeLookup)
                && definitionSchemeLookup.TryGetValue(combinationId, out var scheme))
            ? null
            : scheme;

    public IReadOnlyCollection<PairedDevice> PairedDevices => _pairedDevices.Values;

    public PairedDevice? GetDevice(int deviceId)
        => _pairedDevices.TryGetValue(deviceId, out var device)
            ? device
            : null;

    #endregion

    #region Helpers

    public void SetPreferredSchemes(IEnumerable<PreferredInputScheme> preferredInputSchemes)
    {
        // Create our lookup using only one preferred scheme per definition per combination, if there are mulitples, we'll ignore them
        _preferredSchemeLookup = preferredInputSchemes.GroupBy(scheme
            => new { scheme.DefinitionName, scheme.CombinationId, scheme.SchemeName })
            .Select(schemeDuplicates => schemeDuplicates.First())
            .GroupBy(scheme => new { scheme.DefinitionName })
            .ToDictionary(schemeGroup => schemeGroup.Key.DefinitionName, 
                            schemeGroup => schemeGroup.ToDictionary(scheme => scheme.CombinationId, StringComparer.OrdinalIgnoreCase),
                            StringComparer.OrdinalIgnoreCase);
    }

    public void AddDevice(RuntimeDeviceIdentifier deviceIdentifier)
    {
        _pairedDevices[deviceIdentifier.DeviceId] = new PairedDevice(Id, deviceIdentifier);
    }

    public PairedDevice? RemoveDevice(int deviceId)
    {
        if (_pairedDevices.TryGetValue(deviceId, out var device))
        {
            _pairedDevices.Remove(deviceId);
            return device;
        }

        return null;
    }

    public IReadOnlyCollection<PairedDevice> GetPairedDevices()
        => _pairedDevices.Values;

    public PairedDevice? GetPairedDevice(int id)
        => _pairedDevices.TryGetValue(id, out var device)
            ? device
            : null;

    #endregion
}
