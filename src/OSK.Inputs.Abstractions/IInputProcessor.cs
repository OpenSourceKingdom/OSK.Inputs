using System;
using OSK.Hexagonal.MetaData;
using OSK.Inputs.Abstractions.Runtime;
using OSK.Inputs.Abstractions.Notifications;

namespace OSK.Inputs.Abstractions;

[HexagonalIntegration(HexagonalIntegrationType.LibraryProvided)]
public interface IInputProcessor
{
    void Pause(bool pause);

    void ProcessInputs(TimeSpan deltaTime);

    void ProcessEvent(InputEvent inputEvent);

    void HandleDeviceNotification(DeviceStateChangedNotification notification);
}
