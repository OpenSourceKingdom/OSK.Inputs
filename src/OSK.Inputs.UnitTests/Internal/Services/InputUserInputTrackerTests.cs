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

    private readonly MockOutputFactory<InputUserInputTracker> _outputFactory;

    #endregion

    #region Constructors

    public InputUserInputTrackerTests()
    {
        _outputFactory = new MockOutputFactory<InputUserInputTracker>();
    }

    #endregion

    #region Track

    [Fact]
    public void Track_NonPhysicalEvent_ReturnsFailureToProcess()
    {
        // Arrange
        var tracker = CreateTracker();

        // Act
        var output = tracker.Track(TimeSpan.Zero, new VirtualInputEvent(new TestVirtualInput(new TestPhysicalInput(1)), InputPhase.Start));

        // Assert
        Assert.False(output.IsSuccessful);
    }

    [Fact]
    public void Track_DeviceNotSetForTracking_ReturnsFailureToProcess()
    {
        // Arrange
        var tracker = CreateTracker();

        // Act
        var output = tracker.Track(TimeSpan.Zero, new InputPowerEvent(new RuntimeDeviceIdentifier(1, TestIdentity.Identity2),
            1, InputPhase.Start, []));

        // Assert
        Assert.False(output.IsSuccessful);
    }

    [Fact]
    public void Track_InputReceivedNotPartOfMap_ReturnsFailureToProcess()
    {
        // Arrange
        var tracker = CreateTracker();

        // Act
        var output = tracker.Track(TimeSpan.Zero, new InputPowerEvent(new RuntimeDeviceIdentifier(1, TestIdentity.Identity1),
            200, InputPhase.Start, []));

        // Assert
        Assert.False(output.IsSuccessful);
    }

    [Fact]
    public void Track_InputReceivedPartOfMap_InputEventUnrecognizedPhysicalInputEvent_ReturnsFailureToProcess()
    {
        // Arrange
        var tracker = CreateTracker();

        // Act
        var output = tracker.Track(TimeSpan.Zero, new SpecialInputEvent(new RuntimeDeviceIdentifier(1, TestIdentity.Identity1),
            new TestPhysicalInput(1), InputPhase.Start));

        // Assert
        Assert.False(output.IsSuccessful);
    }

    [Fact]
    public void Track_InputReceivedPartOfMap_InputStartPhase_RemainsAfterProcess_ReturnsSuccessfulProcess()
    {
        // Arrange
        var tracker = CreateTracker();

        // Act
        var output = tracker.Track(TimeSpan.Zero, new InputPowerEvent(new RuntimeDeviceIdentifier(1, TestIdentity.Identity1),
            1, InputPhase.Start, []));


        // Assert
        Assert.True(output.IsSuccessful);

        var deviceTracker = tracker.GetInputTrackers().Single();

        Assert.Single(deviceTracker.AllInputStates);
    }

    [Fact]
    public void Track_InputReceivedPartOfMap_InputEndPhase_ConfigHasNoTapDelay_RemovedAfterProcess_ReturnsSuccessfulProcess()
    {
        // Arrange
        var tracker = CreateTracker();
        tracker.Track(TimeSpan.Zero, new InputPowerEvent(new RuntimeDeviceIdentifier(1, TestIdentity.Identity1),
            1, InputPhase.Start, []));

        // Act
        var output = tracker.Track(TimeSpan.Zero, new InputPowerEvent(new RuntimeDeviceIdentifier(1, TestIdentity.Identity1),
            1, InputPhase.End, []));

        // Assert
        Assert.True(output.IsSuccessful);

        var deviceTracker = tracker.GetInputTrackers().Single();

        Assert.Empty(deviceTracker.AllInputStates);
    }

    [Fact]
    public void Track_InputReceivedPartOfMap_InputEndPhase_ConfigHasTapDelay_NotRemovedAfterProcess_ReturnsSuccessfulProcess()
    {
        // Arrange
        var processorConfiguration = new InputProcessorConfiguration() { TapReactivationTime = TimeSpan.FromMicroseconds(0.1) };
        var tracker = CreateTracker(processorConfiguration);

        tracker.Track(TimeSpan.Zero, new InputPowerEvent(new RuntimeDeviceIdentifier(1, TestIdentity.Identity1),
            1, InputPhase.Start, []));

        // Act
        var output = tracker.Track(TimeSpan.Zero, new InputPowerEvent(new RuntimeDeviceIdentifier(1, TestIdentity.Identity1),
            1, InputPhase.End, []));

        // Assert
        Assert.True(output.IsSuccessful);

        var deviceTracker = tracker.GetInputTrackers().Single();

        Assert.Single(deviceTracker.AllInputStates);
    }

    [Fact]
    public void Track_CombinationInput_OneInputReceived_ReturnsSuccessfulProcess()
    {
        // Arrange
        var processorConfiguration = new InputProcessorConfiguration() { TapReactivationTime = TimeSpan.FromMicroseconds(0.1) };
        var tracker = CreateTracker(processorConfiguration);

        // Act
        var output = tracker.Track(TimeSpan.Zero, new InputPowerEvent(new RuntimeDeviceIdentifier(1, TestIdentity.Identity1),
            2, InputPhase.Start, []));

        // Assert
        Assert.True(output.IsSuccessful);

        var deviceTracker = tracker.GetInputTrackers().Single();

        Assert.Single(deviceTracker.AllInputStates);
    }

    [Fact]
    public void Track_CombinationInput_AllInputsReceived_TriggersCombinationState_ReturnsSuccessfulProcess()
    {
        // Arrange
        var processorConfiguration = new InputProcessorConfiguration() { TapReactivationTime = TimeSpan.FromMicroseconds(0.1) };
        var tracker = CreateTracker(processorConfiguration);

        // Act
        tracker.Track(TimeSpan.Zero, new InputPowerEvent(new RuntimeDeviceIdentifier(1, TestIdentity.Identity1),
            2, InputPhase.Start, []));
        var output = tracker.Track(TimeSpan.Zero, new InputPowerEvent(new RuntimeDeviceIdentifier(1, TestIdentity.Identity1),
            3, InputPhase.Start, []));

        // Assert
        Assert.True(output.IsSuccessful);

        var deviceTracker = tracker.GetInputTrackers().Single();

        Assert.Equal(3, deviceTracker.AllInputStates.Count());
    }

    [Fact]
    public void Track_CombinationInput_AllInputsReceived_NewInputEnds_RemovesAllAssociatedStates_ReturnsSuccessfulProcess()
    {
        // Arrange
        var processorConfiguration = new InputProcessorConfiguration() { TapReactivationTime = TimeSpan.FromMicroseconds(0) };
        var tracker = CreateTracker(processorConfiguration);

        // Act
        tracker.Track(TimeSpan.Zero, new InputPowerEvent(new RuntimeDeviceIdentifier(1, TestIdentity.Identity1),
            2, InputPhase.Start, []));
        tracker.Track(TimeSpan.Zero, new InputPowerEvent(new RuntimeDeviceIdentifier(1, TestIdentity.Identity1),
            3, InputPhase.Start, []));
        var output = tracker.Track(TimeSpan.Zero, new InputPowerEvent(new RuntimeDeviceIdentifier(1, TestIdentity.Identity1),
            3, InputPhase.End, []));

        // Assert
        Assert.True(output.IsSuccessful);

        var deviceTracker = tracker.GetInputTrackers().Single();

        Assert.Empty(deviceTracker.AllInputStates);
    }

    #endregion

    #region Update

    [Fact]
    public void Update_InputInEndPhase_ConfigHasTapDelay_RemovedAfterUpdate_ReturnsSuccessfulProcess()
    {
        // Arrange
        var processorConfiguration = new InputProcessorConfiguration() { TapReactivationTime = TimeSpan.FromMicroseconds(0.1) };
        var tracker = CreateTracker(processorConfiguration);

        tracker.Track(TimeSpan.Zero, new InputPowerEvent(new RuntimeDeviceIdentifier(1, TestIdentity.Identity1),
            1, InputPhase.End, []));

        // Act
        tracker.Update(TimeSpan.FromSeconds(.5));

        // Assert
        var deviceTracker = tracker.GetInputTrackers().Single();

        Assert.Empty(deviceTracker.AllInputStates);
    }

    [Fact]
    public void Update_CombinationInput_InputsStartPhaseTurnToActive_RunsCombinationBasedOnInputs_ReturnsSuccessfulProcess()
    {
        // Arrange
        var processorConfiguration = new InputProcessorConfiguration() { TapReactivationTime = TimeSpan.FromMicroseconds(0.1) };
        var tracker = CreateTracker(processorConfiguration);

        tracker.Track(TimeSpan.Zero, new InputPowerEvent(new RuntimeDeviceIdentifier(1, TestIdentity.Identity1),
            2, InputPhase.Start, []));
        tracker.Track(TimeSpan.Zero, new InputPowerEvent(new RuntimeDeviceIdentifier(1, TestIdentity.Identity1),
            3, InputPhase.Start, []));


        // Act
        var processedEvents = tracker.Update(TimeSpan.FromSeconds(0.1));

        // Assert
        var deviceTracker = tracker.GetInputTrackers().Single();

        Assert.Equal(3, deviceTracker.AllInputStates.Count());
        Assert.Single(processedEvents);

        var processedEvent = processedEvents.Single();
        Assert.True(processedEvent.Triggered);
    }

    [Fact]
    public void Update_CombinationInput_MultipleActiveRuns_RunsCombinationBasedOnInputs_ReturnsSuccessfulProcess()
    {
        // Arrange
        var processorConfiguration = new InputProcessorConfiguration() { TapReactivationTime = TimeSpan.FromMicroseconds(0.1) };
        var tracker = CreateTracker(processorConfiguration);

        tracker.Track(TimeSpan.Zero, new InputPowerEvent(new RuntimeDeviceIdentifier(1, TestIdentity.Identity1),
            2, InputPhase.Start, []));
        tracker.Track(TimeSpan.Zero, new InputPowerEvent(new RuntimeDeviceIdentifier(1, TestIdentity.Identity1),
            3, InputPhase.Start, []));


        // Act
        var processedEvents = tracker.Update(TimeSpan.FromSeconds(0.1));
        var processedEvents2 = tracker.Update(TimeSpan.FromSeconds(0.1));
        var processedEvents3 = tracker.Update(TimeSpan.FromSeconds(0.1));

        // Assert
        var deviceTracker = tracker.GetInputTrackers().Single();

        Assert.Equal(3, deviceTracker.AllInputStates.Count());
        Assert.Single(processedEvents);

        var processedEvent = processedEvents.Single();
        Assert.True(processedEvent.Triggered);

        var processedEvent2 = processedEvents2.Single();
        Assert.True(processedEvent2.Triggered);

        var processedEvent3 = processedEvents3.Single();
        Assert.True(processedEvent3.Triggered);
    }

    #endregion

    #region Helpers

    private InputUserInputTracker CreateTracker(InputProcessorConfiguration? configuration = null)
    {
        return new InputUserInputTracker(1,
            new InputSchemeActionMap
            ("Abc", "Abc", [
                new DeviceSchemeActionMap(TestIdentity.Identity1,
                 [
                    new ActiveInputActionMap() {
                        Action = new InputAction("Abc", new HashSet<InputPhase>() { InputPhase.Active }, _ => { }),
                        Input = new TestPhysicalInput(1)
                    },
                    new ActiveInputActionMap() {
                        Action = new InputAction("Abc", new HashSet<InputPhase>() { InputPhase.Active }, _ => { }),
                        Input = new DeviceCombinationInput(TestIdentity.Identity1.DeviceType, new TestPhysicalInput(2),new TestPhysicalInput(3))
                    }
                 ])
            ]), new InputSystemConfiguration(
                [new TestDeviceSpecification(TestIdentity.Identity1, new TestPhysicalInput(1), new TestPhysicalInput(2), new TestPhysicalInput(3))], 
                [], configuration ?? new(), new()), Mock.Of<ILogger<InputUserInputTracker>>(), _outputFactory,
                Mock.Of<IServiceProvider>()); 
    }

    #endregion
}