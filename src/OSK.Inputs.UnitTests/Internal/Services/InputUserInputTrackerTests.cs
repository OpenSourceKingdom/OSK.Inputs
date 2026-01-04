using Microsoft.Extensions.Logging;
using Moq;
using OSK.Functions.Outputs.Mocks;
using OSK.Inputs.Abstractions.Configuration;
using OSK.Inputs.Abstractions.Inputs;
using OSK.Inputs.Abstractions.Runtime;
using OSK.Inputs.UnitTests._Helpers;
using Xunit;
using OSK.Inputs.Internal.Services;

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
                    new InputActionMap() {
                        Action = new InputAction("Abc", new HashSet<InputPhase>() { InputPhase.Active }, _ => { }),
                        Input = new TestPhysicalInput(1),
                        LinkedInputIds = []
                    }
                 ])
            ]), new InputProcessorConfiguration(), Mock.Of<ILogger<InputUserInputTracker>>(), outputFactory,
                Mock.Of<IServiceProvider>());
    }

    #endregion

    #region Track

    [Fact]
    public void Track_NonPhysicalEvent_ReturnsFailureToProcess()
    {
        // Arrange/Act
        var output = _tracker.Track(new VirtualInputEvent(new TestVirtualInput(new TestPhysicalInput(1)), InputPhase.Start));

        // Assert
        Assert.False(output.IsSuccessful);
    }

    [Fact]
    public void Track_DeviceNotSetForTracking_ReturnsFailureToProcess()
    {
        // Arrange/Act
        var output = _tracker.Track(new InputPowerEvent(new RuntimeDeviceIdentifier(1, TestIdentity.Identity2),
            new TestPhysicalInput(1), InputPhase.Start, []));

        // Assert
        Assert.False(output.IsSuccessful);
    }

    [Fact]
    public void Track_InputReceivedNotPartOfMap_ReturnsFailureToProcess()
    {
        // Arrange/Act
        var output = _tracker.Track(new InputPowerEvent(new RuntimeDeviceIdentifier(1, TestIdentity.Identity1),
            new TestPhysicalInput(10), InputPhase.Start, []));

        // Assert
        Assert.False(output.IsSuccessful);
    }

    [Fact]
    public void Track_InputReceivedPartOfMap_InputEventUnrecognizedPhysicalInputEvent_ReturnsFailureToProcess()
    {
        // Arrange/Act
        var output = _tracker.Track(new SpecialInputEvent(new RuntimeDeviceIdentifier(1, TestIdentity.Identity1),
            new TestPhysicalInput(1), InputPhase.Start));

        // Assert
        Assert.False(output.IsSuccessful);
    }


    [Fact]
    public void Track_InputReceivedPartOfMap_ReturnsSuccessfulProcess()
    {
        // Arrange/Act
        var output = _tracker.Track(new InputPowerEvent(new RuntimeDeviceIdentifier(1, TestIdentity.Identity1),
            new TestPhysicalInput(1), InputPhase.Start, []));

        // Assert
        Assert.True(output.IsSuccessful);
    }

    #endregion
}