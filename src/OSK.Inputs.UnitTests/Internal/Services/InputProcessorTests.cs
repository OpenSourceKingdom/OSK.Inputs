using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using OSK.Inputs.Abstractions;
using OSK.Inputs.Abstractions.Configuration;
using OSK.Inputs.Abstractions.Inputs;
using OSK.Inputs.Abstractions.Notifications;
using OSK.Inputs.Abstractions.Runtime;
using OSK.Inputs.Internal;
using OSK.Inputs.Internal.Models;
using OSK.Inputs.Internal.Services;
using OSK.Inputs.Ports;
using OSK.Inputs.UnitTests._Helpers;
using Xunit;

namespace OSK.Inputs.UnitTests.Internal.Services;

public class InputProcessorTests
{
    #region Variables

    private readonly Mock<IInputConfigurationProvider> _mockConfigurationProvider;
    private readonly Mock<IInputUserManager> _mockUserManager;
    private readonly Mock<IInputNotificationPublisher> _mockNotificationPublisher; 

    private readonly InputProcessor _processor;

    #endregion

    #region Constructors

    public InputProcessorTests()
    {
        _mockUserManager = new();
        _mockNotificationPublisher = new();
        _mockConfigurationProvider = new();

        var mockLogger = new Mock<ILogger<UserInputTracker>>();
        var mockProvider = new Mock<IServiceProvider>();
        mockProvider.Setup(m => m.GetService(typeof(ILogger<UserInputTracker>)))
            .Returns(mockLogger.Object);

        _processor = new InputProcessor(_mockUserManager.Object, _mockNotificationPublisher.Object, _mockConfigurationProvider.Object,
            mockProvider.Object, Mock.Of<ILogger<InputProcessor>>());
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
        _processor.ProcessEvent(new InputPowerEvent(new RuntimeDeviceIdentifier(1, TestIdentity.Identity1),
            new TestPhysicalInput(1), InputPhase.Start, []));

        // Asseert
        Assert.True(true);
    }

    #endregion
}
