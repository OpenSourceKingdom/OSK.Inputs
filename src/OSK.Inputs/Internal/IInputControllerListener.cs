using System;
using System.Collections.Generic;
using OSK.Inputs.Models.Runtime;
using OSK.Inputs.Options;
using System.Threading.Tasks;
using OSK.Inputs.Models.Configuration;

namespace OSK.Inputs.Internal;
internal interface IInputControllerListener: IDisposable
{
    InputControllerConfiguration ControllerConfiguration { get; }

    Task<IEnumerable<ActivatedInput>> ReadInputsAsync(InputHandlerOptions options);
}
