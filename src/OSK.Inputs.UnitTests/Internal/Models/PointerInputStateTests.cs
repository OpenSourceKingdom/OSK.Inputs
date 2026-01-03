using System;
using System.Numerics;
using OSK.Inputs.Internal.Models;
using OSK.Inputs.Abstractions.Runtime;
using Xunit;
using OSK.Inputs.UnitTests._Helpers;

namespace OSK.Inputs.UnitTests.Internal.Models;

public class PointerInputStateTests
{
    #region GetCurrentPositionAndMotionData

    [Fact]
    public void GetCurrentPositionAndMotionData_NoRecords_ReturnsNull()
    {
        // Arrange
        var state = new InputPointerState(1, new TestPhysicalInput(1), 10)
        {
            DeviceIdentifier = new RuntimeDeviceIdentifier(1, TestIdentity.Identity1),
            Duration = TimeSpan.FromSeconds(0)
        };

        // Act
        var result = state.GetCurrentPositionAndMotionData();

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetCurrentPositionAndMotionData_OneRecord_ReturnsPositionAndZeroMotion()
    {
        // Arrange
        var state = new InputPointerState(1, new TestPhysicalInput(1), 10)
        {
            DeviceIdentifier = new RuntimeDeviceIdentifier(1, TestIdentity.Identity1),
            Duration = TimeSpan.FromSeconds(0)
        };
        var position = new Vector2(3f, -2f);

        state.Duration = TimeSpan.Zero; // record time = 0
        state.AddRecord(position);

        // Act
        var result = state.GetCurrentPositionAndMotionData();

        // Assert
        Assert.NotNull(result);
        var (pos, motion) = result.Value;
        Assert.Equal(position, pos);
        Assert.Equal(Vector2.Zero, motion.Velocity);
        Assert.Equal(Vector2.Zero, motion.Acceleration);
    }

    [Fact]
    public void GetCurrentPositionAndMotionData_TwoRecords_CalculatesVelocityAndAcceleration()
    {
        // Arrange
        var state = new InputPointerState(1, new TestPhysicalInput(1), 10)
        {
            DeviceIdentifier = new RuntimeDeviceIdentifier(1, TestIdentity.Identity1),
            Duration = TimeSpan.FromSeconds(0)
        };
        var p1 = new Vector2(0f, 0f);
        var p2 = new Vector2(4f, 0f);

        // times: 0s and 2s
        state.Duration = TimeSpan.Zero;
        state.AddRecord(p1);

        state.Duration = TimeSpan.FromSeconds(2);
        state.AddRecord(p2);

        // Act
        var result = state.GetCurrentPositionAndMotionData();

        // Assert
        Assert.NotNull(result);
        var (pos, motion) = result.Value;

        // Expected velocity = (p2 - p1) / 2s = (4,0) / 2 = (2,0)
        var expectedVelocity = new Vector2(2f, 0f);
        // Expected acceleration = (newVelocity - previousVelocity(0)) / 2s = (2,0)/2 = (1,0)
        var expectedAcceleration = new Vector2(1f, 0f);

        Assert.Equal(p2, pos);
        Assert.Equal(expectedVelocity.X, motion.Velocity.X, 3);
        Assert.Equal(expectedVelocity.Y, motion.Velocity.Y, 3);
        Assert.Equal(expectedAcceleration.X, motion.Acceleration.X, 3);
        Assert.Equal(expectedAcceleration.Y, motion.Acceleration.Y, 3);
    }

    [Fact]
    public void GetCurrentPositionAndMotionData_ThreeRecords_CalculatesVelocityAndAcceleration()
    {
        // Arrange
        var state = new InputPointerState(1, new TestPhysicalInput(1), 10)
        {
            DeviceIdentifier = new RuntimeDeviceIdentifier(1, TestIdentity.Identity1),
            Duration = TimeSpan.FromSeconds(0)
        };
        var p1 = new Vector2(0f, 0f);
        var p2 = new Vector2(2f, 0f);
        var p3 = new Vector2(8f, 0f);

        // times: 0s, 2s, 4s
        state.Duration = TimeSpan.Zero;
        state.AddRecord(p1);

        state.Duration = TimeSpan.FromSeconds(2);
        state.AddRecord(p2);

        state.Duration = TimeSpan.FromSeconds(4);
        state.AddRecord(p3);

        // Act
        var result = state.GetCurrentPositionAndMotionData();

        // Assert
        Assert.NotNull(result);
        var (pos, motion) = result.Value;

        // Between p1 and p2: v1 = (2 - 0) / 2 = 1
        // Between p2 and p3: v2 = (8 - 2) / 2 = 3
        // Acceleration = (v2 - v1) / 2 = (3 - 1) / 2 = 1
        var expectedVelocity = new Vector2(3f, 0f);
        var expectedAcceleration = new Vector2(1f, 0f);

        Assert.Equal(p3, pos);
        Assert.Equal(expectedVelocity.X, motion.Velocity.X, 3);
        Assert.Equal(expectedVelocity.Y, motion.Velocity.Y, 3);
        Assert.Equal(expectedAcceleration.X, motion.Acceleration.X, 3);
        Assert.Equal(expectedAcceleration.Y, motion.Acceleration.Y, 3);
    }

    #endregion
}