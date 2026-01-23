using System;
using OSK.Hexagonal.MetaData;
using OSK.Inputs.Abstractions.Runtime;
using OSK.Inputs.Abstractions.Notifications;
using OSK.Functions.Outputs.Abstractions;

namespace OSK.Inputs.Abstractions;

/// <summary>
/// The core integration point for integrating input systems like Godot or Unity. Inputs for users are to be passed into 
/// the <see cref="ProcessEvent(TimeSpan, InputEvent)"/> method as they are received, polled, or otherwise. 
/// 
/// <br />
/// Note: the only methods that should, in most cases, be used by integrations is either the <see cref="ProcessEvent(TimeSpan, InputEvent)"/>
/// or the <see cref="ProcessMessage(IInputProcessorMessage)"/> signatures as they are used to drive inputs and messages
/// within the input system. Other methods are utilized by the Input System itself.
/// </summary>
[HexagonalIntegration(HexagonalIntegrationType.LibraryProvided)]
public interface IInputProcessor
{
    /// <summary>
    /// Toggles input processing for events or updates; i.e. if this is false, the processor will ignore calls.
    /// </summary>
    /// <param name="pause">Whether processing should be paused</param>
    void ToggleInputProcessing(bool pause);

    /// <summary>
    /// Updates internal states and input tracking for users based on the delta time provided. The delta time is to be the 
    /// time from the last frame
    /// </summary>
    /// <param name="deltaTime">The time that has passed since the last frame update</param>
    void Update(TimeSpan deltaTime);

    /// <summary>
    /// Informs the processor, and the input system at large, about a given input event read from a user input device.
    /// </summary>
    /// <param name="deltaTime">The time that has passed since the last frame update</param>
    /// <param name="inputEvent">The <see cref="InputEvent"/> the user interaction triggered</param>
    /// <returns>
    /// An output that describes whether the event was processed or not. 
    /// <br />
    /// Note: a successful output does not necessarily mean that an action was triggered - it only means that it met the requirements
    /// for validation and is being tracked within the input system. All input updates should be sent to this method.
    /// </returns>
    IOutput ProcessEvent(TimeSpan deltaTime, InputEvent inputEvent);

    /// <summary>
    /// Informs the processor of some meaningful change in input system state that the input system should be aware of.
    /// </summary>
    /// <param name="message">The <see cref="IInputProcessorMessage"/> to send to the input system</param>
    void ProcessMessage(IInputProcessorMessage message);
}
