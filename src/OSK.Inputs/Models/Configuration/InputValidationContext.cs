using System;
using System.Collections.Generic;
using System.Linq;
using OSK.Functions.Outputs.Abstractions;
using OSK.Inputs.Exceptions;

namespace OSK.Inputs.Models.Configuration;
public class InputValidationContext(string errorCategory)
{
    #region Static

    internal static InputValidationContext Success = new InputValidationContext(string.Empty);
    internal static InputValidationContext Error(string category, int errorCode, string message)
    {
        var context = new InputValidationContext(category);
        context.AddErrors(errorCode, message);
        return context;
    }

    #endregion

    #region Variables

    public string ErrorCategory => errorCategory;

    public IEnumerable<Error> Errors => _errorLookup.Select(errorKvp =>
    {
        var aggregateError = string.Join(Environment.NewLine, errorKvp.Value.Select(error => $"{errorKvp.Key}: {error}"));
        return new Error($"Input Validation Error: {errorCategory}{Environment.NewLine}{aggregateError}");
    });

    private readonly Dictionary<int, List<string>> _errorLookup = [];

    #endregion

    #region Helpers

    internal void AddErrors(int errorCode, params string[] messages)
    {
        if (!_errorLookup.TryGetValue(errorCode, out var errors))
        {
            errors = [];
            _errorLookup[errorCode] = errors;
        }

        errors.AddRange(messages);
    }

    internal void EnsureValid()
    {
        if (Errors.Any())
        {
            var errorMessage = string.Join(Environment.NewLine, Errors.Select(e => e.Message));
            throw new InputValidationException($"One or more errors were encountered when validating input.{Environment.NewLine}{errorMessage}");
        }
    }

    internal bool CheckErrorExists(int errorCode)
    {
        return _errorLookup.TryGetValue(errorCode, out _);
    }

    #endregion
}
