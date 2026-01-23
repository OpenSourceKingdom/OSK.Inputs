using OSK.Inputs.Abstractions.Runtime;
using OSK.Inputs.Internal.Models;
using OSK.Inputs.UnitTests._Helpers;
using Xunit;

namespace OSK.Inputs.UnitTests.Internal.Models;

public class InputUserTests
{
    #region Variables

    private readonly Dictionary<int, PairedDevice> _pairedDevices;

    private InputUser _inputUser;

    #endregion

    #region Constructors

    public InputUserTests()
    {
        _pairedDevices = [];
        _inputUser = new InputUser(1, _pairedDevices);
    }

    #endregion

    #region AddDevice

    [Fact]
    public void AddDevice_NoDevices_ReturnsSuccessfully()
    {
        // Arrange/Act
        _inputUser.AddDevice(new RuntimeDeviceIdentifier(1, TestIdentity.Identity1));

        // Asssert
        Assert.Single(_pairedDevices);
    }

    [Fact]
    public void AddDevice_IdenticalDeviceAlreadyAdded_DoesNotAddNewDeviceToUser_ReturnsSuccessfully()
    {
        // Arrange;
        var deviceIdentifier = new RuntimeDeviceIdentifier(1, TestIdentity.Identity1);
        _pairedDevices[1] = new PairedDevice(1, deviceIdentifier);

        // Act
        _inputUser.AddDevice(deviceIdentifier);

        // Asssert
        Assert.Single(_pairedDevices);
    }


    [Fact]
    public void AddDevice_SimilarButNotIdenticalDeviceAdded_AddsNewDeviceToUser_ReturnsSuccessfully()
    {
        // Arrange
        _pairedDevices[1] = new PairedDevice(1, new RuntimeDeviceIdentifier(1, TestIdentity.Identity1));

        // Act
        _inputUser.AddDevice(new RuntimeDeviceIdentifier(2, TestIdentity.Identity1));

        // Asssert
        Assert.Equal(2, _pairedDevices.Count);
    }

    #endregion

    #region RemoveDevice

    [Fact]
    public void RemoveDevice_DeviceNotAdded_ReturnsNull()
    {
        // Arrange
        var inputUser = new InputUser(1);

        // Act
        var device = inputUser.RemoveDevice(1);

        // Assert
        Assert.Null(device);
    }

    [Fact]
    public void RemoveDevice_DeviceAdded_RemovesFromUserAndReturnsDevice()
    {
        // Arrange
        var deviceIdentifier = new RuntimeDeviceIdentifier(1, TestIdentity.Identity1);
        var pairedDevice = new PairedDevice(1, deviceIdentifier);
        _pairedDevices[1] = pairedDevice;

        // Act
        var device = _inputUser.RemoveDevice(1);

        // Assert
        Assert.NotNull(device);
        Assert.Equal(pairedDevice, device);
        Assert.Empty(_pairedDevices);
    }

    #endregion

    #region GetPairedDevices

    [Fact]
    public void GetPairedDevices_NoDevices_ReturnsEmpty()
    {
        // Arrange/Act
        var devices = _inputUser.GetPairedDevices();

        // Assert
        Assert.Empty(devices);
    }

    [Fact]
    public void GetPairedDevices_HasDevices_ReturnsExpectedList()
    {
        // Arrange
        var deviceIdentifier = new RuntimeDeviceIdentifier(1, TestIdentity.Identity1);
        var pairedDevice = new PairedDevice(1, deviceIdentifier);
        _pairedDevices[1] = pairedDevice;

        // Act
        var devices = _inputUser.GetPairedDevices();

        // Assert
        Assert.Single(devices);
        Assert.Equal(pairedDevice, devices.First());
    }

    #endregion

    #region GetPairedDevice

    [Theory]
    [InlineData(-1)]
    [InlineData(100)]
    public void GetPairedDevice_IndexOutOfRange_ReturnsNull(int index)
    {
        // Arrange/Act
        var device = _inputUser.GetDevice(index);

        // Assert
        Assert.Null(device);
    }

    [Fact]
    public void GetPairedDevice_DeviceIdDoesNotExist_ReturnsNull()
    {
        // Arrange
        var deviceIdentifier = new RuntimeDeviceIdentifier(1, TestIdentity.Identity1);
        var pairedDevice = new PairedDevice(1, deviceIdentifier);
        _pairedDevices[1] = pairedDevice;

        // Act
        var device = _inputUser.GetDevice(2);

        // Assert
        Assert.Null(device);
    }

    [Fact]
    public void GetPairedDevice_Valid_ReturnsDevice()
    {
        // Arrange
        var deviceIdentifier = new RuntimeDeviceIdentifier(1, TestIdentity.Identity1);
        var pairedDevice = new PairedDevice(1, deviceIdentifier);
        _pairedDevices[1] = pairedDevice;

        // Act
        var device = _inputUser.GetDevice(1);

        // Assert
        Assert.NotNull(device);
        Assert.Equal(pairedDevice, device);
    }

    #endregion
}
