using OSK.Inputs.Abstractions.Configuration;
using OSK.Inputs.Abstractions.Inputs;
using OSK.Inputs.Abstractions.UnitTests._Helpers;

namespace OSK.Inputs.Abstractions.UnitTests.Configuration;

public class DeviceSchemeActionMapTests
{
    #region GetActionMaps

    [Fact]
    public void GetActionMaps_EmptyMaps_ReturnsNoActionMap()
    {
        // Arrange
        var map = CreateActionMap();

        // Act
        var actions = map.GetActionMaps(1);

        // Assert
        Assert.NotNull(actions);
        Assert.Empty(actions);
    }

    [Fact]
    public void GetActionMaps_OnlyVirtualInput_ReturnsActionMapForInputInCombination()
    {
        // Arrange
        var combinationInput = new CombinationInput(1, new TestInput(1));
        var map = CreateActionMap(combinationInput);

        // Act
        var actions = map.GetActionMaps(1);

        // Assert
        Assert.NotNull(actions);
        Assert.Single(actions);
    }

    [Fact]
    public void GetActionMaps_OnlyPhysicalInput_ReturnsActionMapForSingleInput()
    {
        // Arrange
        var map = CreateActionMap(new TestInput(1));

        // Act
        var actions = map.GetActionMaps(1);

        // Arrange
        Assert.NotNull(actions);
        Assert.Single(actions);
    }

    [Fact]
    public void GetActions_MixedInputs_ReturnsExpectedActionMaps()
    {
        // Arrange
        var combinationInput = new CombinationInput(1, new TestInput(2), new TestInput(3));
        var map = CreateActionMap(combinationInput, new TestInput(2), new TestInput(4));

        // Act
        var actions = map.GetActionMaps(2);

        // Arrange
        Assert.NotNull(actions);
        Assert.Equal(2, actions.Count());
    }

    #endregion

    #region Helpers

    private DeviceSchemeActionMap CreateActionMap(params Input[] inputs)
        => new DeviceSchemeActionMap(TestIdentity.Identity1, inputs
            .Select(input => new DeviceInputActionMap() 
            { 
                Action = new InputAction("", new HashSet<InputPhase>(), _ => { }),
                Input = input,
                LinkedInputIds = input is CombinationInput combinationInput
                    ? combinationInput.DeviceInputs.Select(d => d.Id).ToArray() 
                    : []
            }));

    #endregion
}
