using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OSK.Hexagonal.MetaData;
using OSK.Inputs.Models.Configuration;
using OSK.Inputs.Models.Runtime;

namespace OSK.Inputs.Ports;

[HexagonalIntegration(HexagonalIntegrationType.LibraryProvided)]
public interface IInputHandler: IDisposable
{
    event Action<InputControllerConfiguration>? OnControllerChanged;

    Task<IEnumerable<ActivatedInput>> ReadInputsAsync();
}
