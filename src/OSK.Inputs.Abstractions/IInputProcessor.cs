using System;
using OSK.Hexagonal.MetaData;
using OSK.Inputs.Abstractions.Events;

namespace OSK.Inputs.Abstractions;

[HexagonalIntegration(HexagonalIntegrationType.LibraryProvided)]
public interface IInputProcessor
{
    void Pause(bool pause);

    void ProcessInputs(TimeSpan deltaTime);

    void ProcessActivation(InputActivation activation);

    void HandleDeviceEvent(DeviceStateChangedEvent deviceEvent);
}
