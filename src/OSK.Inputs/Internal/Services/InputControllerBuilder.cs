using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using OSK.Inputs.Models.Configuration;
using OSK.Inputs.Models.Inputs;
using OSK.Inputs.Options;
using OSK.Inputs.Ports;

namespace OSK.Inputs.Internal.Services;
internal class InputControllerBuilder(InputControllerName controllerName) : IInputControllerBuilder
{
    #region Variables

    private readonly Dictionary<string, IEnumerable<string>> _combinationInputLookup = [];
    private readonly Dictionary<string, IInput> _inputLookup = [];
    private Type? _inputReaderType;
    private Func<IInput, bool>? _inputValidator;

    #endregion

    #region IInputControllerBuilder

    public IInputControllerBuilder AddInput(IInput input)
    {
        if (input is null)
        {
            throw new ArgumentNullException(nameof(input));
        }
        if (_inputLookup.TryGetValue(input.Name, out _))
        {
            throw new DuplicateNameException($"The input name {input.Name} has already been added to the {controllerName} controller configuration.");
        }

        _inputLookup[input.Name] = input;
        return this;
    }

    public IInputControllerBuilder AddCombinationInput(string inputName, params string[] combinationInputNames)
    {
        if (string.IsNullOrWhiteSpace(inputName))
        {
            throw new ArgumentNullException(inputName, $"Unable to add combination input {inputName} because the input name is empty.");
        }
        if (combinationInputNames is null)
        {
            throw new ArgumentNullException(nameof(combinationInputNames));
        }
        if (combinationInputNames.Length == 0) 
        {
            throw new InvalidOperationException($"The combination input {inputName} for controller {controllerName} has no inputs to be usable.");
        }
        if (combinationInputNames.Any(name => string.IsNullOrWhiteSpace(name)))
        {
            throw new InvalidOperationException($"One of the inputs for the combination input {inputName} had an empty name.");
        }
        if (_combinationInputLookup.TryGetValue(inputName, out _))
        {
            throw new DuplicateNameException($"Unable to add combination input with the name {inputName} since it was already added to {controllerName} controller configuration.");
        }

        _combinationInputLookup[inputName] = combinationInputNames;
        return this;
    }

    public IInputControllerBuilder UseInputReader<TReader>()
        where TReader : IInputReader
    {
        if (_inputReaderType is not null)
        {
            throw new InvalidOperationException($"Unable to set input reader type to {typeof(TReader).FullName} for {controllerName} controller since it was already set to {_inputReaderType.FullName}. Did you mean to overwrite this value?");
        }

        _inputReaderType = typeof(TReader);
        return this;
    }

    public IInputControllerBuilder UseValidation(Func<IInput, bool> inputValidator)
    {
        if (_inputValidator is not null)
        {
            throw new InvalidOperationException($"Unable to set input validator for {controllerName} controller since it was already set. Did you mean to overwrite this value?");
        }

        _inputValidator = inputValidator;
        return this;
    }

    #endregion

    #region Helpers

    public IInputControllerConfiguration BuildInputController()
    {
        if (_inputReaderType is null)
        {
            throw new ArgumentNullException("inputReaderType", $"Unable to build input controller {controllerName} because there was no valid input reader to use.");
        }

        var invalidCombinationInputs = _combinationInputLookup.Where(kvp => kvp.Value.Any(inputName => !_inputLookup.TryGetValue(inputName, out _)));
        if (invalidCombinationInputs.Any())
        {
            var errorMessages = invalidCombinationInputs.Select(kvp => $"Invalid Combination: {kvp.Key}. The following input names were not registered on the {controllerName} controller: {kvp.Value.Where(inputName => !_inputLookup.TryGetValue(inputName, out _))}");
            var errorMessage = string.Join(Environment.NewLine, errorMessages);
            throw new InvalidOperationException($"Unable to build input controller {controllerName} since one or more combination inputs have input names that were not registed to the controller.{Environment.NewLine}{errorMessage}");
        }

        invalidCombinationInputs = _combinationInputLookup.Where(kvp => kvp.Value.Any(inputName => _inputLookup.TryGetValue(inputName, out var input) && input is not HardwareInput));
        if (invalidCombinationInputs.Any())
        {
            var errorMessages = invalidCombinationInputs.Select(kvp => $"Invalid Combination: {kvp.Key}. The following input names were not registered as hardware inputs for the {controllerName} controller: {kvp.Value.Where(inputName => !_inputLookup.TryGetValue(inputName, out _))}");
            var errorMessage = string.Join(Environment.NewLine, errorMessages);
            throw new InvalidOperationException($"Unable to build input controller {controllerName} since one or more combination inputs have combination inputs with nonhardware input types.{Environment.NewLine}{errorMessage}");
        }

        var combinationInputs = _combinationInputLookup.Select(kvp => new CombinationInput(kvp.Key, kvp.Value.Select(inputName => (HardwareInput ) _inputLookup[inputName]),
            new CombinationInputOptions()));

        return new DefaultInputControllerConfiguration(controllerName, _inputReaderType, _inputLookup.Values.Concat(combinationInputs), _inputValidator);
    }

    #endregion
}
