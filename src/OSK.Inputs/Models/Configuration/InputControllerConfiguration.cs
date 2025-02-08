using System;
using System.Collections.Generic;
using System.Linq;
using OSK.Inputs.Models.Runtime;

namespace OSK.Inputs.Models.Configuration;
public class InputControllerConfiguration(string controllerName, IEnumerable<IInputReceiverDescriptor> receiverDescriptors,
    IEnumerable<InputScheme> inputSchemes)
{
    #region Variables

    public string ControllerName => controllerName;

    public IReadOnlyCollection<IInputReceiverDescriptor> ReceiverDescriptors { get; } = receiverDescriptors.ToList();

    public IReadOnlyCollection<InputScheme> InputSchemes { get; } = inputSchemes.ToArray();

    #endregion

    #region Helpers

    public InputScheme GetActiveScheme(ActiveInputScheme? preferredScheme)
    {
        if (!InputSchemes.Any())
        {
            throw new InvalidOperationException("There were no input schemes that could be used.");
        }

        var defaultInputScheme = InputSchemes.First();
        foreach (var inputScheme in InputSchemes)
        {
            if (preferredScheme is not null && string.Equals(preferredScheme.ActiveInputSchemeName, inputScheme.SchemeName, StringComparison.Ordinal))
            {
                return inputScheme;
            }
            if (inputScheme.IsDefault)
            {
                defaultInputScheme = inputScheme;
            }
        }

        return defaultInputScheme;
    }

    public InputControllerConfiguration Clone(IEnumerable<InputScheme>? additionalInputSchemes)
    {
        return new InputControllerConfiguration(ControllerName, ReceiverDescriptors,
            InputSchemes.Concat(additionalInputSchemes ?? []));
    }

    #endregion
}
