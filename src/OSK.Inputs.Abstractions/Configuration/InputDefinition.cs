using System;
using System.Collections.Generic;
using System.Linq;

namespace OSK.Inputs.Abstractions.Configuration;

public class InputDefinition(string name, IEnumerable<InputAction> actions, IEnumerable<InputScheme> schemes, bool isDefault)
{
    #region Variables

    private readonly Dictionary<string, InputAction> _actionLookup
        = actions?.Where(action => action?.Name is not null).ToDictionary(action => action.Name, StringComparer.OrdinalIgnoreCase) ?? [];
    private Dictionary<string, Dictionary<string, InputScheme>> _deviceCombinationSchemeLookup 
        = schemes?.Where(scheme => scheme?.Name is not null)
                  .GroupBy(scheme => scheme.CombinationId, StringComparer.OrdinalIgnoreCase)
                  .ToDictionary(schemeGroup => schemeGroup.Key, schemegroup => schemegroup.ToDictionary(scheme => scheme.Name, StringComparer.OrdinalIgnoreCase),
                        StringComparer.OrdinalIgnoreCase) 
            ?? [];

    #endregion

    #region Api

    public string Name => name;

    public bool IsDefault => isDefault;

    public IReadOnlyCollection<InputAction> Actions => _actionLookup.Values;

    public IReadOnlyCollection<InputScheme> Schemes => [.. _deviceCombinationSchemeLookup.Values.SelectMany(schemeLookup => schemeLookup.Values)];
    
    public InputAction? GetAction(string name)
        => !string.IsNullOrWhiteSpace(name) && _actionLookup.TryGetValue(name, out var action)
            ? action
            : null;

    public IEnumerable<InputScheme> GetSchemesByDevicecCombination(string combinationId)
        => !string.IsNullOrWhiteSpace(combinationId) && _deviceCombinationSchemeLookup.TryGetValue(combinationId, out var schemeGroup)
            ? schemeGroup.Values
            : Enumerable.Empty<InputScheme>();

    public InputScheme? GetScheme(string combinationId, string schemeName)
        => !string.IsNullOrWhiteSpace(combinationId) && !string.IsNullOrWhiteSpace(schemeName)
            && _deviceCombinationSchemeLookup.TryGetValue(combinationId, out var schemeGroup)
            && schemeGroup.TryGetValue(schemeName, out var scheme)
            ? scheme
            : null;

    #endregion

    #region Helpers

    internal void ResetDefinition()
    {
        _deviceCombinationSchemeLookup = _deviceCombinationSchemeLookup.SelectMany(schemeLookup => schemeLookup.Value.Values.Where(scheme => !scheme.IsCustom))
                                     .GroupBy(scheme => scheme.CombinationId)
                                     .ToDictionary(schemeGroup => schemeGroup.Key, schemeGroup => schemeGroup.ToDictionary(scheme => scheme.Name));
    }

    internal void ApplyCustomScheme(CustomInputScheme scheme)
    {
        if (scheme is null)
        {
            return;
        }
        if (string.IsNullOrWhiteSpace(scheme.DefinitionName)
            || !scheme.DefinitionName.Equals(Name, StringComparison.Ordinal)
            || string.IsNullOrWhiteSpace(scheme.Name))
        {
            return;
        }

        var inputScheme = scheme.ToInputScheme();
        if (_deviceCombinationSchemeLookup.TryGetValue(inputScheme.CombinationId, out var combinationSchemeLookup) 
             && (!combinationSchemeLookup.TryGetValue(inputScheme.Name, out var existingScheme) || existingScheme.IsCustom))
        {
            _deviceCombinationSchemeLookup[inputScheme.CombinationId][inputScheme.Name] = inputScheme;
        }
    }

    #endregion
}
