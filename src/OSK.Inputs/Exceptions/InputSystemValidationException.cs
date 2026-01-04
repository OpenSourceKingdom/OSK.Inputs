using System;

namespace OSK.Inputs.Exceptions;

public class InputSystemValidationException(string message) : Exception(message)
{
}
