using System;
using System.Collections.Generic;
using System.Linq;

namespace OSK.Inputs.Abstractions.Configuration;

public class InputDefinition(string name, IEnumerable<InputScheme> schemes, IEnumerable<InputAction> actions, bool isDefault)
{
    #region Variables

    private Dictionary<string, InputScheme> _schemeLookup = schemes.ToDictionary(scheme => scheme.Name);
    private readonly Dictionary<string, InputAction> _actionLookup = actions.ToDictionary(action => action.Name);

    #endregion

    #region Api

    public string Name => name;

    public bool IsDefault => isDefault;

    public IReadOnlyCollection<InputScheme> Schemes => _schemeLookup.Values;

    public IReadOnlyCollection<InputAction> Actions => _actionLookup.Values;
    
    public InputAction? GetAction(string name)
        => !string.IsNullOrWhiteSpace(name) && _actionLookup.TryGetValue(name, out var action)
            ? action
            : null;

    public InputScheme? GetScheme(string name)
        => !string.IsNullOrWhiteSpace(name) && _schemeLookup.TryGetValue(name, out var scheme)
            ? scheme
            : null;

    #endregion

    #region Helpers

    internal void ResetDefinition()
    {
        _schemeLookup = _schemeLookup.Where(schemeKvp => !schemeKvp.Value.IsCustom)
                                     .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
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

        if (!_schemeLookup.TryGetValue(scheme.Name, out var currentScheme) || currentScheme.IsCustom)
        {
            _schemeLookup[scheme.Name] = new InputScheme(scheme.Name, scheme.DeviceMaps, false, true);
        }
    }

    #endregion
}
