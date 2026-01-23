namespace OSK.Inputs.Models;

/// <summary>
/// Represents a validation state for an input configuration
/// </summary>
public enum InputConfigurationValidation
{
    /// <summary>
    /// The configuration is valid
    /// </summary>
    Ok,

    /// <summary>
    /// The target contains duplicate data
    /// </summary>
    DuplicateData,

    /// <summary>
    /// The target contains invalid data that was rejected
    /// </summary>
    InvalidData,

    /// <summary>
    /// The target is missing required data and it must be applied
    /// </summary>
    MissingData
}
