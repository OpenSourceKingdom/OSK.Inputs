using OSK.Inputs.Abstractions.Configuration;
using OSK.Inputs.Abstractions.UnitTests._Helpers;

namespace OSK.Inputs.Abstractions.UnitTests.Configuration;

public class InputSchemeTests
{
    #region GetDeviceMap

    [Fact]
    public void GetDeviceMap_deviceFamilyDoesNotExist_ReturnsNull()
    {
        // Arrange
        var scheme = new InputScheme("Hello", [], false, false);

        // Act
        var map = scheme.GetDeviceMap(TestDeviceFamily.Identity1);

        // Assert
        Assert.Null(map);
    }


    [Fact]
    public void GetDeviceMap_ValiddeviceFamily_ReturnsExpectedMap()
    {
        // Arrange
        var expectedMap = new DeviceInputMap() { DeviceFamily = TestDeviceFamily.Identity1, InputMaps = [] };
        var scheme = new InputScheme("Hello", [expectedMap], false, false);

        // Act
        var map = scheme.GetDeviceMap(TestDeviceFamily.Identity1);

        // Assert
        Assert.NotNull(map);
        Assert.Equal(expectedMap, map);
    }

    #endregion
}
