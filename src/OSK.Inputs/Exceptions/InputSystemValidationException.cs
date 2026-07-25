using System;

namespace OSK.Inputs.Exceptions;

/// <summary>
/// Ann exception that is specific to the input system validation process
/// </summary>
/// <param name="message">The error message</param>
public class InputSystemValidationException(string message) : Exception(message)
{
}
