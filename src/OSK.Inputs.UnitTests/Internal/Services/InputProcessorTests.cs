using Microsoft.Extensions.Logging;
using Moq;
using OSK.Functions.Outputs.Abstractions;
using OSK.Functions.Outputs.Logging.Abstractions;
using OSK.Functions.Outputs.Mocks;
using OSK.Inputs.Abstractions;
using OSK.Inputs.Abstractions.Configuration;
using OSK.Inputs.Abstractions.Inputs;
using OSK.Inputs.Abstractions.Notifications;
using OSK.Inputs.Abstractions.Runtime;
using OSK.Inputs.Internal;
using OSK.Inputs.Internal.Models;
using OSK.Inputs.Internal.Services;
using OSK.Inputs.Options;
using OSK.Inputs.Ports;
using OSK.Inputs.UnitTests._Helpers;
using Xunit;

namespace OSK.Inputs.UnitTests.Internal.Services;

public class InputProcessorTests
{
    #region Variables

    private readonly Dictionary<int, IInputUserTracker> _trackers = [];

    private readonly Mock<IInputConfigurationProvider> _mockConfigurationProvider;
    private readonly Mock<IInputUserManager> _mockUserManager;
    private readonly Mock<IInputNotificationPublisher> _mockNotificationPublisher;
    private readonly Mock<IInputUserTracker> _mockUserInputTracker;
    private readonly Mock<IServiceProvider> _mockServiceProvider;
    private readonly IOutputFactory<InputProcessor> _outputFactory;

    private readonly InputProcessor _processor;

    #endregion

    #region Constructors

    public InputProcessorTests()
    {
        _mockUserManager = new();
        _mockNotificationPublisher = new();
        _mockConfigurationProvider = new();
        _mockUserInputTracker = new();
        _outputFactory = new MockOutputFactory<InputProcessor>();

        var mockLogger = new Mock<ILogger<InputUserInputTracker>>();
        _mockServiceProvider = new Mock<IServiceProvider>();
        _mockServiceProvider.Setup(m => m.GetService(typeof(ILogger<InputUserInputTracker>)))
            .Returns(mockLogger.Object);
        _mockServiceProvider.Setup(m => m.GetService(typeof(IOutputFactory<InputUserInputTracker>)))
            .Returns(new MockOutputFactory<InputUserInputTracker>());
        _mockServiceProvider.Setup(m => m.GetService(typeof(IServiceProvider)))
            .Returns(_mockServiceProvider.Object);

        _processor = new InputProcessor(_mockUserManager.Object, 
            _mockNotificationPublisher.Object, 
            _mockConfigurationProvider.Object,
            _mockServiceProvider.Object, 
            Mock.Of<ILogger<InputProcessor>>(), 
            _outputFactory,
            (_, _, _, _) => _mockUserInputTracker.Object,
            _trackers);
    }

    #endregion

    #region HandleDeviceNotification

    [Fact]
    public void HandleDeviceNotification_NullNotification_ThrowsArgumentNullException()
    {
        // Arrange/Act/Assert
        Assert.Throws<ArgumentNullException>(() => _processor.HandleDeviceNotification(null!));
    }

    [Fact]
    public void HandleDeviceNotification_UserNotFoundForDevice_NotifiesDeviceNotification()
    {
        // Arrange
        var stateChangeEvent = new DeviceStateChangedNotification(new RuntimeDeviceIdentifier(1, TestIdentity.Identity1), DeviceStatus.Disconnected);

        _mockUserManager.Setup(m => m.GetInputUserForDevice(It.IsAny<int>()))
            .Returns((IInputUser?)null);

        // Act
        _processor.HandleDeviceNotification(stateChangeEvent);

        // Assert
        _mockNotificationPublisher.Verify(m => m.Notify(It.Is<DeviceStateChangedNotification>(s => true)), Times.Once);
        _mockNotificationPublisher.Verify(m => m.Notify(It.Is<UserDeviceNotification>(s => true)), Times.Never);
    }

    [Fact]
    public void HandleDeviceNotification_UserFoundForDevice_NotifiesUserDeviceNotification()
    {
        // Arrange
        var stateChangeEvent = new DeviceStateChangedNotification(new RuntimeDeviceIdentifier(1, TestIdentity.Identity1), DeviceStatus.Disconnected);

        _mockUserManager.Setup(m => m.GetInputUserForDevice(It.IsAny<int>()))
            .Returns(new InputUser(1, default));

        // Act
        _processor.HandleDeviceNotification(stateChangeEvent);

        // Assert
        _mockNotificationPublisher.Verify(m => m.Notify(It.Is<DeviceStateChangedNotification>(s => true)), Times.Never);
        _mockNotificationPublisher.Verify(m => m.Notify(It.Is<UserDeviceNotification>(s => s.UserId == 1)), Times.Once);
    }

    #endregion

    #region ToggleInputProcessing

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void ToggleInputProcessing_NewToggleState_SetsInternalValueAsExpected(bool toggle)
    {
        // Arrange
        _processor._pauseInputProcessing = !toggle;

        // Act
        _processor.ToggleInputProcessing(toggle);

        // Assert
        Assert.Equal(toggle, _processor._pauseInputProcessing);
    }

    #endregion

    #region Update

    [Fact]
    public void Update_InputProcessingPaused_DoesNotCallInputTracker()
    {
        // Arrange
        var mockTracker = new Mock<IInputUserTracker>();
        _trackers[1] = mockTracker.Object;

        _processor._pauseInputProcessing = true;

        // Act
        _processor.Update(TimeSpan.FromSeconds(2));

        // Assert
        mockTracker.Verify(m => m.Update(It.IsAny<TimeSpan>()), Times.Never);
    }


    [Fact]
    public void Update_InputProcessing_DoesCallsInputTracker()
    {
        // Arrange
        var mockTracker = new Mock<IInputUserTracker>();
        mockTracker.Setup(m => m.Update(It.IsAny<TimeSpan>()))
            .Returns([]);

        _trackers[1] = mockTracker.Object;

        _processor._pauseInputProcessing = false;

        // Act
        _processor.Update(TimeSpan.FromSeconds(2));

        // Assert
        mockTracker.Verify(m => m.Update(It.IsAny<TimeSpan>()), Times.Once);
    }

    #endregion

    #region ProcessEvent

    [Fact]
    public void ProcessEvent_NullEvent_ThrowsArgumentNullException()
    {
        // Arrange/Act/Assert
        Assert.Throws<ArgumentNullException>(() => _processor.ProcessEvent(null!));
    }

    [Fact]
    public void ProcessEvent_PhysicalInputEvent_SetToNotProcess_ReturnsEarly()
    {
        // Arrange
        _processor._pauseInputProcessing = true;

        // Act
        var output = _processor.ProcessEvent(new InputPowerEvent(new RuntimeDeviceIdentifier(1, TestIdentity.Identity1),
            new TestPhysicalInput(1), InputPhase.Start, []));

        // Asseert
        Assert.False(output.IsSuccessful);
    }

    [Fact]
    public void ProcessEvent_HasNoUserForDevice_DeviceJoinBehaviorSetToManual_NoUsers_ReturnsNullAndNotifiesUnrecognizedDeviceEvent()
    {
        // Arrange
        _mockConfigurationProvider.SetupGet(m => m.Configuration)
            .Returns(new InputSystemConfiguration([], [], new(), new()
            {
                DeviceJoinBehavior = DevicePairingBehavior.Manual
            }));

        // Setting to null to validate this method is never called - null will NRE
        _mockUserManager.Setup(m => m.GetUsers())
            .Returns((IEnumerable<IInputUser>)null!);

        _mockUserManager.Setup(m => m.GetInputUserForDevice(It.IsAny<int>()))
            .Returns((IInputUser?)null);

        // Act
        var output = _processor.ProcessEvent(new InputPowerEvent(new RuntimeDeviceIdentifier(1, TestIdentity.Identity1),
            new TestPhysicalInput(1), InputPhase.Start, []));

        // Assert
        Assert.False(output.IsSuccessful);
        _mockNotificationPublisher.Verify(m => m.Notify(It.Is<IInputNotification>(n => n is UnrecognizedDeviceNotification)), Times.Once);
    }

    [Fact]
    public void ProcessEvent_NoUserForDevice_DeviceJoinBehaviorSetToAutomatic_DeviceNotInASupportedCombination_ReturnsNullAndNotifiesUnrecognizedDeviceEvent()
    {
        // Arrange
        _mockConfigurationProvider.SetupGet(m => m.Configuration)
            .Returns(new InputSystemConfiguration([], [], new(), new()
            {
                DeviceJoinBehavior = DevicePairingBehavior.Balanced
            }));

        // Setting to null to validate this method is never called - null will NRE
        _mockUserManager.Setup(m => m.GetUsers())
            .Returns((IEnumerable<IInputUser>)null!);

        _mockUserManager.Setup(m => m.GetInputUserForDevice(It.IsAny<int>()))
            .Returns((IInputUser?)null);

        // Act
        var output = _processor.ProcessEvent(new InputPowerEvent(new RuntimeDeviceIdentifier(1, TestIdentity.Identity1),
            new TestPhysicalInput(1), InputPhase.Start, []));

        // Assert
        Assert.False(output.IsSuccessful);
        _mockNotificationPublisher.Verify(m => m.Notify(It.Is<IInputNotification>(n => n is UnrecognizedDeviceNotification)), Times.Once);
    }

    [Fact]
    public void ProcessEvent_NoUserForDevice_NoUsers_DeviceJoinBehaviorSetToAutomatic_UserManagerFailsToCreateUser_ReturnsNullAndNotifiesUnrecognizedDeviceEvent()
    {
        // Arrange
        _mockConfigurationProvider.SetupGet(m => m.Configuration)
            .Returns(new InputSystemConfiguration([], [
                new InputDefinition("Abc", [],
                    [
                        new InputScheme("Abc", 
                            [
                                new DeviceInputMap() { DeviceIdentity = TestIdentity.Identity1, InputMaps = [] }
                            ], 
                            false, false)
                    ], false)
            ], new(), new()
            {
                DeviceJoinBehavior = DevicePairingBehavior.Balanced
            }));

        _mockUserManager.Setup(m => m.GetInputUserForDevice(It.IsAny<int>()))
            .Returns((IInputUser?)null);
        _mockUserManager.Setup(m => m.GetUsers())
            .Returns([]);
        _mockUserManager.Setup(m => m.CreateUser(It.IsAny<UserJoinOptions>()))
            .Returns(_outputFactory.Fail<IInputUser>("Bad day"));

        // Act
        var output = _processor.ProcessEvent(new InputPowerEvent(new RuntimeDeviceIdentifier(1, TestIdentity.Identity1),
            new TestPhysicalInput(1), InputPhase.Start, []));

        // Assert
        Assert.False(output.IsSuccessful);
        _mockNotificationPublisher.Verify(m => m.Notify(It.Is<IInputNotification>(n => n is UnrecognizedDeviceNotification)), Times.Once);
    }

    [Fact]
    public void ProcessEvent_NoUserForDevice_NoUsers_DeviceJoinBehaviorSetToAutomatic_DevicePairingFails_ReturnsNullAndNotifiesUnrecognizedDeviceEvent()
    {
        // Arrange
        _mockConfigurationProvider.SetupGet(m => m.Configuration)
            .Returns(new InputSystemConfiguration([], [
                new InputDefinition("Abc", [],
                    [
                        new InputScheme("Abc",
                            [
                                new DeviceInputMap() { DeviceIdentity = TestIdentity.Identity1, InputMaps = [] }
                            ],
                            false, false)
                    ], false)
            ], new(), new()
            {
                DeviceJoinBehavior = DevicePairingBehavior.Balanced
            }));

        _mockUserManager.Setup(m => m.GetInputUserForDevice(It.IsAny<int>()))
            .Returns((IInputUser?)null);
        _mockUserManager.Setup(m => m.GetUsers())
            .Returns([]);
        _mockUserManager.Setup(m => m.CreateUser(It.IsAny<UserJoinOptions>()))
            .Returns(_outputFactory.Succeed((IInputUser)new InputUser(1, new ActiveInputScheme("Abc", "Abc"))));
        _mockUserManager.Setup(m => m.PairDevice(It.IsAny<int>(), It.IsAny<RuntimeDeviceIdentifier>()))
            .Returns(_outputFactory.Fail("Bad Day"));

        // Act
        var output = _processor.ProcessEvent(new InputPowerEvent(new RuntimeDeviceIdentifier(1, TestIdentity.Identity1),
            new TestPhysicalInput(1), InputPhase.Start, []));

        // Assert
        Assert.False(output.IsSuccessful);
        _mockNotificationPublisher.Verify(m => m.Notify(It.Is<IInputNotification>(n => n is DevicePairingFailedNotification)), Times.Once);
        _mockNotificationPublisher.Verify(m => m.Notify(It.Is<IInputNotification>(n => n is UnrecognizedDeviceNotification)), Times.Once);
    }

    [Fact]
    public void ProcessEvent_NoUserForDevice_UsersExist_PairingWithSingleUser_DoesNotHaveDevice_InputTrackerDoesNotTriggerAction_ReturnsSuccessfully()
    {
        // Arrange
        _mockConfigurationProvider.SetupGet(m => m.Configuration)
            .Returns(new InputSystemConfiguration([], [
                new InputDefinition("Abc", [],
                    [
                        new InputScheme("Abc",
                            [
                                new DeviceInputMap() { DeviceIdentity = TestIdentity.Identity1, InputMaps = [] }
                            ],
                            false, false)
                    ], false)
            ], new(), new()
            {
                DeviceJoinBehavior = DevicePairingBehavior.Balanced
            }));

        var mockUser = new Mock<IInputUser>();
        mockUser.SetupGet(m => m.Id)
            .Returns(1);
        mockUser.SetupGet(m => m.PairedDevices)
            .Returns([new PairedDevice(1, new RuntimeDeviceIdentifier(1, TestIdentity.Identity1))]);

        _mockUserManager.Setup(m => m.GetInputUserForDevice(It.IsAny<int>()))
            .Returns((IInputUser?)null);
        _mockUserManager.Setup(m => m.GetUsers())
            .Returns([mockUser.Object]);
        _mockUserManager.Setup(m => m.PairDevice(It.IsAny<int>(), It.IsAny<RuntimeDeviceIdentifier>()))
            .Returns((int userId, RuntimeDeviceIdentifier _) =>
            {
                Assert.Equal(mockUser.Object.Id, userId);
                return _outputFactory.Succeed();
            });

        _mockUserInputTracker.Setup(m => m.Track(It.IsAny<InputEvent>()))
            .Returns(_outputFactory.Succeed((TriggeredActionEvent?)null));

        // Act
        var output = _processor.ProcessEvent(new InputPowerEvent(new RuntimeDeviceIdentifier(1, TestIdentity.Identity1),
            new TestPhysicalInput(1), InputPhase.Start, []));

        // Assert
        Assert.True(output.IsSuccessful);

        _mockNotificationPublisher.Verify(m => m.Notify(It.IsAny<IInputNotification>()), Times.Never);
    }

    [Fact]
    public void ProcessEvent_NoUserForDevice_UsersExist_PairingWithSingleUser_HasSimilarDevice_InputTrackerDoesNotTriggerAction_ReturnsSuccessfully()
    {
        // Arrange
        _mockConfigurationProvider.SetupGet(m => m.Configuration)
            .Returns(new InputSystemConfiguration([], [
                new InputDefinition("Abc", [],
                    [
                        new InputScheme("Abc",
                            [
                                new DeviceInputMap() { DeviceIdentity = TestIdentity.Identity1, InputMaps = [] }
                            ],
                            false, false)
                    ], false)
            ], new(), new()
            {
                DeviceJoinBehavior = DevicePairingBehavior.Balanced
            }));

        var mockUser = new Mock<IInputUser>();
        mockUser.SetupGet(m => m.Id)
            .Returns(1);
        mockUser.SetupGet(m => m.PairedDevices)
            .Returns([new PairedDevice(1, new RuntimeDeviceIdentifier(1, TestIdentity.Identity1))]);

        _mockUserManager.Setup(m => m.GetInputUserForDevice(It.IsAny<int>()))
            .Returns((IInputUser?)null);
        _mockUserManager.Setup(m => m.GetUsers())
            .Returns([mockUser.Object]);
        _mockUserManager.Setup(m => m.PairDevice(It.IsAny<int>(), It.IsAny<RuntimeDeviceIdentifier>()))
            .Returns((int userId, RuntimeDeviceIdentifier _) =>
            {
                Assert.Equal(mockUser.Object.Id, userId);
                return _outputFactory.Succeed();
            });

        _mockUserInputTracker.Setup(m => m.Track(It.IsAny<InputEvent>()))
            .Returns(_outputFactory.Succeed((TriggeredActionEvent?)null));

        // Act
        var output = _processor.ProcessEvent(new InputPowerEvent(new RuntimeDeviceIdentifier(1, TestIdentity.Identity1),
            new TestPhysicalInput(1), InputPhase.Start, []));

        // Assert
        Assert.True(output.IsSuccessful);

        _mockNotificationPublisher.Verify(m => m.Notify(It.IsAny<IInputNotification>()), Times.Never);
    }

    [Fact]
    public void ProcessEvent_NoUserForDevice_UsersExist_PairingWithMultipleUsers_OneHasAndOtherDoesNotHaveSimilarDevice_InputTrackerDoesNotTriggerAction_ReturnsSuccessfully()
    {
        // Arrange
        _mockConfigurationProvider.SetupGet(m => m.Configuration)
            .Returns(new InputSystemConfiguration([], [
                new InputDefinition("Abc", [],
                    [
                        new InputScheme("Abc",
                            [
                                new DeviceInputMap() { DeviceIdentity = TestIdentity.Identity1, InputMaps = [] }
                            ],
                            false, false)
                    ], false)
            ], new(), new()
            {
                DeviceJoinBehavior = DevicePairingBehavior.Balanced
            }));

        var mockUser = new Mock<IInputUser>();
        mockUser.SetupGet(m => m.Id)
            .Returns(1);
        mockUser.SetupGet(m => m.PairedDevices)
            .Returns([new PairedDevice(1, new RuntimeDeviceIdentifier(1, TestIdentity.Identity1))]);

        var mockUser2 = new Mock<IInputUser>();
        mockUser2.SetupGet(m => m.Id)
            .Returns(2);
        mockUser2.SetupGet(m => m.PairedDevices)
            .Returns([]);

        _mockUserManager.Setup(m => m.GetInputUserForDevice(It.IsAny<int>()))
            .Returns((IInputUser?)null);
        _mockUserManager.Setup(m => m.GetUsers())
            .Returns([mockUser.Object, mockUser2.Object]);
        _mockUserManager.Setup(m => m.PairDevice(It.IsAny<int>(), It.IsAny<RuntimeDeviceIdentifier>()))
            .Returns((int userId, RuntimeDeviceIdentifier _) =>
            {
                Assert.Equal(mockUser2.Object.Id, userId);
                return _outputFactory.Succeed();
            });

        _mockUserInputTracker.Setup(m => m.Track(It.IsAny<InputEvent>()))
            .Returns(_outputFactory.Succeed((TriggeredActionEvent?)null));

        // Act
        var output = _processor.ProcessEvent(new InputPowerEvent(new RuntimeDeviceIdentifier(2, TestIdentity.Identity1),
            new TestPhysicalInput(1), InputPhase.Start, []));

        // Assert
        Assert.True(output.IsSuccessful);

        _mockNotificationPublisher.Verify(m => m.Notify(It.IsAny<IInputNotification>()), Times.Never);
    }

    [Fact]
    public void ProcessEvent_NoUserForDevice_UsersExist_PairingWithMultipleUsers_BothUsersHaveSimilarDevice_InputTrackerDoesNotTriggerAction_ReturnsSuccessfully()
    {
        // Arrange
        _mockConfigurationProvider.SetupGet(m => m.Configuration)
            .Returns(new InputSystemConfiguration([], [
                new InputDefinition("Abc", [],
                    [
                        new InputScheme("Abc",
                            [
                                new DeviceInputMap() { DeviceIdentity = TestIdentity.Identity1, InputMaps = [] }
                            ],
                            false, false)
                    ], false)
            ], new(), new()
            {
                DeviceJoinBehavior = DevicePairingBehavior.Balanced
            }));

        var mockUser = new Mock<IInputUser>();
        mockUser.SetupGet(m => m.Id)
            .Returns(1);
        mockUser.SetupGet(m => m.PairedDevices)
            .Returns([new PairedDevice(1, new RuntimeDeviceIdentifier(1, TestIdentity.Identity1))]);

        var mockUser2 = new Mock<IInputUser>();
        mockUser2.SetupGet(m => m.Id)
            .Returns(2);
        mockUser2.SetupGet(m => m.PairedDevices)
            .Returns([new PairedDevice(2, new RuntimeDeviceIdentifier(2, TestIdentity.Identity1))]);

        _mockUserManager.Setup(m => m.GetInputUserForDevice(It.IsAny<int>()))
            .Returns((IInputUser?)null);
        _mockUserManager.Setup(m => m.GetUsers())
            .Returns([mockUser.Object, mockUser2.Object]);
        _mockUserManager.Setup(m => m.PairDevice(It.IsAny<int>(), It.IsAny<RuntimeDeviceIdentifier>()))
            .Returns((int userId, RuntimeDeviceIdentifier _) =>
            {
                Assert.Equal(mockUser.Object.Id, userId);
                return _outputFactory.Succeed();
            });

        _mockUserInputTracker.Setup(m => m.Track(It.IsAny<InputEvent>()))
            .Returns(_outputFactory.Succeed((TriggeredActionEvent?)null));

        // Act
        var output = _processor.ProcessEvent(new InputPowerEvent(new RuntimeDeviceIdentifier(3, TestIdentity.Identity1),
            new TestPhysicalInput(1), InputPhase.Start, []));

        // Assert
        Assert.True(output.IsSuccessful);

        _mockNotificationPublisher.Verify(m => m.Notify(It.IsAny<IInputNotification>()), Times.Never);
    }

    [Fact]
    public void ProcessEvent_NoUserForDevice_UsersExist_PairingWithMultipleUsers_OneUserWithCompletedCombination_InputTrackerDoesNotTriggerAction_ReturnsSuccessfully()
    {
        // Arrange
        _mockConfigurationProvider.SetupGet(m => m.Configuration)
            .Returns(new InputSystemConfiguration([], [
                new InputDefinition("Abc", [],
                    [
                        new InputScheme("Abc",
                            [
                                new DeviceInputMap() { DeviceIdentity = TestIdentity.Identity1, InputMaps = [] },
                                new DeviceInputMap() { DeviceIdentity = TestIdentity.Identity2, InputMaps = [] }
                            ],
                            false, false)
                    ], false)
            ], new(), new()
            {
                DeviceJoinBehavior = DevicePairingBehavior.Balanced
            }));

        var mockUser = new Mock<IInputUser>();
        mockUser.SetupGet(m => m.Id)
            .Returns(1);
        mockUser.SetupGet(m => m.PairedDevices)
            .Returns([new PairedDevice(1, new RuntimeDeviceIdentifier(1, TestIdentity.Identity1)), 
                    new PairedDevice(1, new RuntimeDeviceIdentifier(2, TestIdentity.Identity2))]);

        var mockUser2 = new Mock<IInputUser>();
        mockUser2.SetupGet(m => m.Id)
            .Returns(2);
        mockUser2.SetupGet(m => m.PairedDevices)
            .Returns([new PairedDevice(2, new RuntimeDeviceIdentifier(3, TestIdentity.Identity1))]);

        _mockUserManager.Setup(m => m.GetInputUserForDevice(It.IsAny<int>()))
            .Returns((IInputUser?)null);
        _mockUserManager.Setup(m => m.GetUsers())
            .Returns([mockUser.Object, mockUser2.Object]);
        _mockUserManager.Setup(m => m.PairDevice(It.IsAny<int>(), It.IsAny<RuntimeDeviceIdentifier>()))
            .Returns((int userId, RuntimeDeviceIdentifier _) =>
            {
                Assert.Equal(mockUser2.Object.Id, userId);
                return _outputFactory.Succeed();
            });

        _mockUserInputTracker.Setup(m => m.Track(It.IsAny<InputEvent>()))
            .Returns(_outputFactory.Succeed((TriggeredActionEvent?)null));

        // Act
        var output = _processor.ProcessEvent(new InputPowerEvent(new RuntimeDeviceIdentifier(4, TestIdentity.Identity2),
            new TestPhysicalInput(1), InputPhase.Start, []));

        // Assert
        Assert.True(output.IsSuccessful);

        _mockNotificationPublisher.Verify(m => m.Notify(It.IsAny<IInputNotification>()), Times.Never);
    }

    [Fact]
    public void ProcessEvent_NoUserForDevice_UsersExist_PairingWithMultipleUsers_DeviceInDifferentCombinationThanUsersHave_InputTrackerDoesNotTriggerAction_ReturnsErrorDueToNotProcessingInputFromRandomDevice()
    {
        // Arrange
        _mockConfigurationProvider.SetupGet(m => m.Configuration)
            .Returns(new InputSystemConfiguration([], [
                new InputDefinition("Abc", [],
                    [
                        new InputScheme("Abc",
                            [
                                new DeviceInputMap() { DeviceIdentity = TestIdentity.Identity1, InputMaps = [] },
                                new DeviceInputMap() { DeviceIdentity = TestIdentity.Identity2, InputMaps = [] }
                            ],
                            false, false),

                        new InputScheme("Def",
                            [
                                new DeviceInputMap() { DeviceIdentity = TestIdentity.Identity3, InputMaps = [] }
                            ],
                            false, false)
                    ], false)
            ], new(), new()
            {
                DeviceJoinBehavior = DevicePairingBehavior.Balanced
            }));

        var mockUser = new Mock<IInputUser>();
        mockUser.SetupGet(m => m.Id)
            .Returns(1);
        mockUser.SetupGet(m => m.PairedDevices)
            .Returns([new PairedDevice(1, new RuntimeDeviceIdentifier(1, TestIdentity.Identity1)),
                    new PairedDevice(1, new RuntimeDeviceIdentifier(2, TestIdentity.Identity2))]);

        var mockUser2 = new Mock<IInputUser>();
        mockUser2.SetupGet(m => m.Id)
            .Returns(2);
        mockUser2.SetupGet(m => m.PairedDevices)
            .Returns([new PairedDevice(2, new RuntimeDeviceIdentifier(3, TestIdentity.Identity1))]);

        _mockUserManager.Setup(m => m.GetInputUserForDevice(It.IsAny<int>()))
            .Returns((IInputUser?)null);
        _mockUserManager.Setup(m => m.GetUsers())
            .Returns([mockUser.Object, mockUser2.Object]);
        _mockUserManager.Setup(m => m.PairDevice(It.IsAny<int>(), It.IsAny<RuntimeDeviceIdentifier>()))
            .Returns((int userId, RuntimeDeviceIdentifier _) =>
            {
                Assert.Equal(mockUser2.Object.Id, userId);
                return _outputFactory.Succeed();
            });

        _mockUserInputTracker.Setup(m => m.Track(It.IsAny<InputEvent>()))
            .Returns(_outputFactory.Succeed((TriggeredActionEvent?)null));

        // Act
        var output = _processor.ProcessEvent(new InputPowerEvent(new RuntimeDeviceIdentifier(4, TestIdentity.Identity3),
            new TestPhysicalInput(1), InputPhase.Start, []));

        // Assert
        Assert.True(output.IsSuccessful);

        _mockNotificationPublisher.Verify(m => m.Notify(It.IsAny<IInputNotification>()), Times.Never);
    }

    [Fact]
    public void ProcessEvent_NoUserForDevice_NoUsers_CreatesUserAndPairs_InputTrackerDoesNotTriggerAction_ReturnsSuccessfully()
    {
        // Arrange
        _mockConfigurationProvider.SetupGet(m => m.Configuration)
            .Returns(new InputSystemConfiguration([], [
                new InputDefinition("Abc", [],
                    [
                        new InputScheme("Abc",
                            [
                                new DeviceInputMap() { DeviceIdentity = TestIdentity.Identity1, InputMaps = [] }
                            ],
                            false, false)
                    ], false)
            ], new(), new()
            {
                DeviceJoinBehavior = DevicePairingBehavior.Balanced
            }));

        _mockUserManager.Setup(m => m.GetInputUserForDevice(It.IsAny<int>()))
            .Returns((IInputUser?)null);
        _mockUserManager.Setup(m => m.GetUsers())
            .Returns([]);
        _mockUserManager.Setup(m => m.CreateUser(It.IsAny<UserJoinOptions>()))
            .Returns(_outputFactory.Succeed((IInputUser)new InputUser(1, new ActiveInputScheme("Abc", "Abc"))));
        _mockUserManager.Setup(m => m.PairDevice(It.IsAny<int>(), It.IsAny<RuntimeDeviceIdentifier>()))
            .Returns(_outputFactory.Succeed());

        _mockUserInputTracker.Setup(m => m.Track(It.IsAny<InputEvent>()))
            .Returns(_outputFactory.Succeed((TriggeredActionEvent?)null));

        // Act
        var output = _processor.ProcessEvent(new InputPowerEvent(new RuntimeDeviceIdentifier(1, TestIdentity.Identity1),
            new TestPhysicalInput(1), InputPhase.Start, []));

        // Assert
        Assert.True(output.IsSuccessful);

        _mockNotificationPublisher.Verify(m => m.Notify(It.IsAny<IInputNotification>()), Times.Never);
    }

    [Fact]
    public void ProcessEvent_UserFoundForDevice_InputTrackerDoesNotTriggerAction_ReturnsSuccessfullyAndNoNotifications()
    {
        // Arrange
        _mockConfigurationProvider.SetupGet(m => m.Configuration)
            .Returns(new InputSystemConfiguration([], [
                new InputDefinition("Abc", [],
                    [
                        new InputScheme("Abc",
                            [
                                new DeviceInputMap() { DeviceIdentity = TestIdentity.Identity1, InputMaps = [] }
                            ],
                            false, false)
                    ], false)
            ], new(), new()
            {
                DeviceJoinBehavior = DevicePairingBehavior.Balanced
            }));

        _mockUserManager.Setup(m => m.GetUsers())
            .Returns([]);
        _mockUserManager.Setup(m => m.GetInputUserForDevice(It.IsAny<int>()))
            .Returns(new InputUser(1, new ActiveInputScheme("Abc", "Abc")));
        _mockUserInputTracker.Setup(m => m.Track(It.IsAny<InputEvent>()))
            .Returns(_outputFactory.Succeed((TriggeredActionEvent?)null));

        // Act
        var output = _processor.ProcessEvent(new InputPowerEvent(new RuntimeDeviceIdentifier(1, TestIdentity.Identity1),
            new TestPhysicalInput(1), InputPhase.Start, []));

        // Assert
        Assert.True(output.IsSuccessful);

        _mockNotificationPublisher.Verify(m => m.Notify(It.IsAny<IInputNotification>()), Times.Never);
    }

    #endregion

    #region ObjectFactory

    [Fact]
    public void ObjectFactory_UserInputTrackerFactory_Validation()
    {
        // Arrange/Act/Assert
        Assert.NotNull(_processor._userInputTrackerFactory.Invoke(_mockServiceProvider.Object,
            [1, new ActiveInputScheme("Abc", "Abc"), new InputSchemeActionMap([]), new InputProcessorConfiguration()]));
    }

    #endregion
}
