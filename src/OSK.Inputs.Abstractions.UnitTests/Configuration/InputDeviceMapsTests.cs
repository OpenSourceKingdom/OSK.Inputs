using OSK.Inputs.Abstractions.Configuration;
using OSK.Inputs.Abstractions.UnitTests._Helpers;

namespace OSK.Inputs.Abstractions.UnitTests.Configuration;

public class InputDeviceMapsTests
{
    #region GetInputMap

    [Fact]
    public void GetInputMap_EmptyInputs_ReturnsNull()
    {
        // Arrange
        var map = new DeviceInputMap() { DeviceFamily = TestIdentity.Identity1, InputMaps = [] };

        // Arrange
        var input = map.GetInputMap(1);

        // Assert
        Assert.Null(input);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(100)]
    public void GetInputMap_HasInputs_InvalidInputId_ReturnsNull(int id)
    {
        // Arrange
        var map = new DeviceInputMap() { DeviceFamily = TestIdentity.Identity1, InputMaps = [ new InputMap() { ActionName = "abc", InputId = 123 }] };

        // Arrange
        var input = map.GetInputMap(id);

        // Assert
        Assert.Null(input);
    }

    [Fact]
    public void GetInputMap_HasInputs_ValidInputId_ReturnsSuccessfully()
    {
        // Arrange
        var map = new DeviceInputMap() { DeviceFamily = TestIdentity.Identity1, InputMaps = [new InputMap() { ActionName = "abc", InputId = 123 }] };

        // Arrange
        var input = map.GetInputMap(123);

        // Assert
        Assert.NotNull(input);
        Assert.Equal(123, input.Value.InputId);
    }

    #endregion
}
