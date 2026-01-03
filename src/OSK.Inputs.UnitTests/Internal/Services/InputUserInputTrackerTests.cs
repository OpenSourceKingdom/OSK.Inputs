using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using OSK.Functions.Outputs.Mocks;
using OSK.Inputs.Abstractions.Configuration;
using OSK.Inputs.Abstractions.Inputs;
using OSK.Inputs.Abstractions.Runtime;
using OSK.Inputs.Internal;
using OSK.Inputs.UnitTests._Helpers;

namespace OSK.Inputs.UnitTests.Internal.Services;

public class InputUserInputTrackerTests
{
    #region Variables

    private readonly InputUserInputTracker _tracker;

    #endregion

    #region Constructors

    public InputUserInputTrackerTests()
    {
        var outputFactory = new MockOutputFactory<InputUserInputTracker>();

        _tracker = new InputUserInputTracker(1, new ActiveInputScheme() { DefinitionName = "Abc", SchemeName = "Abc" },
            new InputSchemeActionMap
            ([ 
                new DeviceSchemeActionMap(TestIdentity.Identity1,
                 [
                    new DeviceInputActionMap() {
                        Action = new InputAction("Abc", new HashSet<InputPhase>() { InputPhase.Active }, _ => { }),
                        Input = new TestPhysicalInput(1),
                        LinkedInputIds = []
                    }
                 ])
            ]), new InputProcessorConfiguration(), Mock.Of<ILogger<InputUserInputTracker>>(), outputFactory,
                Mock.Of<IServiceProvider>());
    }

    #endregion
}
