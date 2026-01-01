using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using OSK.Inputs.Abstractions;
using OSK.Inputs.Abstractions.Notifications;
using OSK.Inputs.Abstractions.Runtime;
using OSK.Inputs.Internal;
using OSK.Inputs.Internal.Services;
using OSK.Inputs.UnitTests._Helpers;
using Xunit;

namespace OSK.Inputs.UnitTests.Internal.Services;

public class InputNotificationPublisherTests
{
    #region Variables

    private readonly InputNotificationPublisher _publisher = new();

    #endregion

    #region Notify

    [Fact]
    public void Notify_NullNotification_ThrowsArgumentNullException()
    {
        // Arrange/Act/Assert
        Assert.Throws<ArgumentNullException>(() => _publisher.Notify(null!));
    }

    [Fact]
    public void Notify_UnhandledNotification_ThrowsInvalidOperationException()
    {
        // Arrange/Act/Assert
        Assert.Throws<InvalidOperationException>(() => _publisher.Notify(new TestDeviceNotification()));
    }

    [Fact]
    public void Notify_InputDeviceNotification_NoRegisteredAction_ReturnsSuccessfully()
    {
        // Arrange/Act
        _publisher.Notify(new DevicePairedNotification(1, new RuntimeDeviceIdentifier(1, TestIdentity.Identity1)));

        // Assert
        Assert.True(true);
    }

    [Fact]
    public void Notify_InputDeviceNotification_NotifiesDeviceDelegate()
    {
        // Arrange
        var updated = false;
        _publisher.OnDeviceNotification += _ => updated = true;

        // Act
        _publisher.Notify(new DevicePairedNotification(1, new RuntimeDeviceIdentifier(1, TestIdentity.Identity1)));

        // Assert
        Assert.True(updated);
    }

    [Fact]
    public void Notify_InputUserNotification_NoRegisteredAction_ReturnsSuccessfully()
    {
        // Arrange/Act
        _publisher.Notify(new UserDeviceConnectedNotification(1, new RuntimeDeviceIdentifier(1, TestIdentity.Identity1)));

        // Assert
        Assert.True(true);
    }

    [Fact]
    public void Notify_InputUsereNotification_NotifiesDeviceDelegate()
    {
        // Arrange
        var updated = false;
        _publisher.OnUserNotification += _ => updated = true;

        // Act
        _publisher.Notify(new InputUserJoinedNotification(Mock.Of<IInputUser>()));

        // Assert
        Assert.True(updated);
    }

    [Fact]
    public void Notify_InputSystemNotification_NoRegisteredAction_ReturnsSuccessfully()
    {
        // Arrange/Act
        _publisher.Notify(new InputProcessingStateChangedNotification(false));

        // Assert
        Assert.True(true);
    }

    [Fact]
    public void Notify_InputSystemNotification_NotifiesDeviceDelegate()
    {
        // Arrange
        var updated = false;
        _publisher.OnSystemNotification += _ => updated = true;

        // Act
        _publisher.Notify(new InputProcessingStateChangedNotification(false));

        // Assert
        Assert.True(updated);
    }

    #endregion
}
