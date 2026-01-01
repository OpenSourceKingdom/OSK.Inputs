using System;
using OSK.Hexagonal.MetaData;
using OSK.Inputs.Abstractions.Runtime;
using OSK.Inputs.Abstractions.Notifications;

namespace OSK.Inputs.Abstractions;

[HexagonalIntegration(HexagonalIntegrationType.LibraryProvided)]
public interface IInputProcessor
{
    void ToggleInputProcessing(bool pause);

    void Update(TimeSpan deltaTime);

    void ProcessEvent(InputEvent inputEvent);

    void HandleDeviceNotification(DeviceStateChangedNotification notification);
}
