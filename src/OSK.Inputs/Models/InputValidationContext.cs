using System;
using System.Collections.Generic;
using System.Linq;
using OSK.Functions.Outputs.Abstractions;
using OSK.Inputs.Exceptions;

namespace OSK.Inputs.Models;
public class InputValidationContext
{
    #region Variables

    public IEnumerable<Error> Errors => _errorLookup.Values;

    private readonly Dictionary<string, Error> _errorLookup = [];

    #endregion

    #region Helpers

    internal void AddError(string errorCategory, int errorCode, string message)
    {
        _errorLookup.Add(GetErrorKey(errorCategory, errorCode), new Error(message));
    }

    internal void AddAggregateError(string errorCategory, int errorCode, IEnumerable<string> messages)
    {
        var errorMessage = string.Join(Environment.NewLine, messages);
        _errorLookup.Add(GetErrorKey(errorCategory, errorCode), 
            new Error($"One or more errors were encountered when validating {errorCategory}:{Environment.NewLine}{errorMessage}"));
    }

    internal void EnsureValid()
    {
        if (_errorLookup.Any())
        {
            var errorMessage = string.Join(Environment.NewLine, _errorLookup.Values.Select(e => e.Message));
            throw new InputValidationException($"One or more errors were encountered when validating input.{Environment.NewLine}{errorMessage}");
        }
    }

    internal bool CheckErrorExists(string errorCategory, int errorCode)
    {
        return _errorLookup.TryGetValue(GetErrorKey(errorCategory, errorCode), out _);
    }

    private string GetErrorKey(string errorCategory, int errorCode)
        => $"{errorCategory}-{errorCode}";

    #endregion
}
