using System;
using OSK.Hexagonal.MetaData;
using OSK.Inputs.Abstractions.Runtime;
using OSK.Inputs.Abstractions.Notifications;
using OSK.Functions.Outputs.Abstractions;

namespace OSK.Inputs.Abstractions;

/// <summary>
/// The core integration point for integrating input systems like Godot or Unity. Inputs for users are to be passed into 
/// the <see cref="ProcessEvent(InputEvent)"/> method as they are received, polled, or otherwise. 
/// 
/// <br />
/// Note: the only methods that should, in most cases, be used by integrations is either the <see cref="ProcessEvent(InputEvent)"/>
/// or the <see cref="HandleDeviceNotification(DeviceStateChangedNotification)"/> signatures as they are used to drive inputs and notifications
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
    /// <param name="deltaTime">The time since the last frame</param>
    void Update(TimeSpan deltaTime);

    /// <summary>
    /// Informs the processor, and the input system at large, about a given input event read from a user input device.
    /// </summary>
    /// <param name="inputEvent">The <see cref="InputEvent"/> the user interaction triggered</param>
    /// <returns>
    /// An output that describes whether the event was processed or not. 
    /// <br />
    /// Note: a successful output does not necessarily mean that an action was triggered - it only means that it met the requirements
    /// for validation and is being tracked within the input system. All input updates should be sent to this method.
    /// </returns>
    IOutput ProcessEvent(InputEvent inputEvent);

    /// <summary>
    /// Notifies the input system and any listeners to device changes
    /// </summary>
    /// <param name="notification">The <see cref="DeviceStateChangedNotification"/> to send to the input system</param>
    void HandleDeviceNotification(DeviceStateChangedNotification notification);
}
