using System;

namespace OSK.Inputs.Exceptions;

public class InputValidationException(string message) : Exception(message)
{
}
