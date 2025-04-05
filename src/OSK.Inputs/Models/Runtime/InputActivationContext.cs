using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace OSK.Inputs.Models.Runtime;

public class InputActivationContext(IServiceProvider serviceProvider, IEnumerable<UserActivatedInput> activatedInputs)
{
    #region Variables

    public IServiceProvider Services => serviceProvider;

    private readonly IReadOnlyDictionary<int, IReadOnlyCollection<UserActivatedInput>> _activatedUserInputs =
        new ReadOnlyDictionary<int, IReadOnlyCollection<UserActivatedInput>>(
            activatedInputs.GroupBy(input => input.UserId).ToDictionary(userInputGroup => userInputGroup.Key,
                userInputGroup => (IReadOnlyCollection<UserActivatedInput>)[.. userInputGroup]));

    #endregion

    #region Helpers

    public IReadOnlyCollection<UserActivatedInput> GetAllActivatedInputs => _activatedUserInputs.Values.SelectMany(userInputs => userInputs).ToArray();

    public IReadOnlyCollection<UserActivatedInput> GetActivatedInputsByUser(int userId)
        => _activatedUserInputs.TryGetValue(userId, out var userInputs) ? userInputs : Array.Empty<UserActivatedInput>();

    #endregion
}
