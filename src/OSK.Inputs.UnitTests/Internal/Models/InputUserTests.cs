using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OSK.Inputs.Abstractions.Runtime;
using OSK.Inputs.Internal.Models;
using OSK.Inputs.UnitTests._Helpers;
using Xunit;

namespace OSK.Inputs.UnitTests.Internal.Models;

public class InputUserTests
{
    #region AddDevice

    [Fact]
    public void AddDevice_NoDevices_ReturnsSuccessfully()
    {
        // Arrange
        var inputUser = new InputUser(1, new ActiveInputScheme("Abc", "Abc"));

        // Act
        inputUser.AddDevice(new RuntimeDeviceIdentifier(1, TestIdentity.Identity1));

        // Asssert
        Assert.Single(inputUser._pairedDevices);
    }

    [Fact]
    public void AddDevice_IdenticalDeviceAlreadyAdded_DoesNotAddNewDeviceToUser_ReturnsSuccessfully()
    {
        // Arrange
        var inputUser = new InputUser(1, new ActiveInputScheme("Abc", "Abc"));
        var deviceIdentifier = new RuntimeDeviceIdentifier(1, TestIdentity.Identity1);
        inputUser._pairedDevices[1] = new PairedDevice(1, deviceIdentifier);

        // Act
        inputUser.AddDevice(deviceIdentifier);

        // Asssert
        Assert.Single(inputUser._pairedDevices);
    }


    [Fact]
    public void AddDevice_SimilarButNotIdenticalDeviceAdded_AddsNewDeviceToUser_ReturnsSuccessfully()
    {
        // Arrange
        var inputUser = new InputUser(1, new ActiveInputScheme("Abc", "Abc"));
        inputUser._pairedDevices[1] = new PairedDevice(1, new RuntimeDeviceIdentifier(1, TestIdentity.Identity1));

        // Act
        inputUser.AddDevice(new RuntimeDeviceIdentifier(2, TestIdentity.Identity1));

        // Asssert
        Assert.Equal(2, inputUser._pairedDevices.Count);
    }

    #endregion

    #region RemoveDevice

    [Fact]
    public void RemoveDevice_DeviceNotAdded_ReturnsNull()
    {
        // Arrange
        var inputUser = new InputUser(1, new ActiveInputScheme("Abc", "Abc"));

        // Act
        var device = inputUser.RemoveDevice(1);

        // Assert
        Assert.Null(device);
    }


    [Fact]
    public void RemoveDevice_DeviceAdded_RemovesFromUserAndReturnsDevice()
    {
        // Arrange
        var inputUser = new InputUser(1, new ActiveInputScheme("Abc", "Abc"));
        var deviceIdentifier = new RuntimeDeviceIdentifier(1, TestIdentity.Identity1);
        var pairedDevice = new PairedDevice(1, deviceIdentifier);
        inputUser._pairedDevices[1] = pairedDevice;

        // Act
        var device = inputUser.RemoveDevice(1);

        // Assert
        Assert.NotNull(device);
        Assert.Equal(pairedDevice, device);
        Assert.Empty(inputUser._pairedDevices);
    }

    #endregion

    #region GetPairedDevices

    [Fact]
    public void GetPairedDevices_NoDevices_ReturnsEmpty()
    {
        // Arrange
        var inputUser = new InputUser(1, new ActiveInputScheme("Abc", "Abc"));

        // Act
        var devices = inputUser.GetPairedDevices();

        // Assert
        Assert.Empty(devices);
    }


    [Fact]
    public void GetPairedDevices_HasDevices_ReturnsExpectedList()
    {
        // Arrange
        var inputUser = new InputUser(1, new ActiveInputScheme("Abc", "Abc"));
        var deviceIdentifier = new RuntimeDeviceIdentifier(1, TestIdentity.Identity1);
        var pairedDevice = new PairedDevice(1, deviceIdentifier);
        inputUser._pairedDevices[1] = pairedDevice;

        // Act
        var devices = inputUser.GetPairedDevices();

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
        // Arrange
        var inputUser = new InputUser(1, new ActiveInputScheme("Abc", "Abc"));

        // Act
        var device = inputUser.GetDevice(index);

        // Assert
        Assert.Null(device);
    }

    [Fact]
    public void GetPairedDevice_DeviceIdDoesNotExist_ReturnsNull()
    {
        // Arrange
        var inputUser = new InputUser(1, new ActiveInputScheme("Abc", "Abc"));
        var deviceIdentifier = new RuntimeDeviceIdentifier(1, TestIdentity.Identity1);
        var pairedDevice = new PairedDevice(1, deviceIdentifier);
        inputUser._pairedDevices[1] = pairedDevice;

        // Act
        var device = inputUser.GetDevice(2);

        // Assert
        Assert.Null(device);
    }

    [Fact]
    public void GetPairedDevice_Valid_ReturnsDevice()
    {
        // Arrange
        var inputUser = new InputUser(1, new ActiveInputScheme("Abc", "Abc"));
        var deviceIdentifier = new RuntimeDeviceIdentifier(1, TestIdentity.Identity1);
        var pairedDevice = new PairedDevice(1, deviceIdentifier);
        inputUser._pairedDevices[1] = pairedDevice;

        // Act
        var device = inputUser.GetDevice(1);

        // Assert
        Assert.NotNull(device);
        Assert.Equal(pairedDevice, device);
    }

    #endregion
}
