using System;
using System.Collections.Generic;
using System.Text;
using OSK.Inputs.Abstractions.Configuration;
using OSK.Inputs.Abstractions.UnitTests._Helpers;

namespace OSK.Inputs.Abstractions.UnitTests.Configuration;

public class InputSchemeTests
{
    #region GetDeviceMap

    [Fact]
    public void GetDeviceMap_DeviceIdentityDoesNotExist_ReturnsNull()
    {
        // Arrange
        var scheme = new InputScheme("Hello", [], false, false);

        // Act
        var map = scheme.GetDeviceMap(TestIdentity.Identity1);

        // Assert
        Assert.Null(map);
    }


    [Fact]
    public void GetDeviceMap_ValidDeviceIdentity_ReturnsExpectedMap()
    {
        // Arrange
        var expectedMap = new InputDeviceMap() { DeviceIdentity = TestIdentity.Identity1, InputMaps = [] };
        var scheme = new InputScheme("Hello", [expectedMap], false, false);

        // Act
        var map = scheme.GetDeviceMap(TestIdentity.Identity1);

        // Assert
        Assert.NotNull(map);
        Assert.Equal(expectedMap, map);
    }

    #endregion
}
