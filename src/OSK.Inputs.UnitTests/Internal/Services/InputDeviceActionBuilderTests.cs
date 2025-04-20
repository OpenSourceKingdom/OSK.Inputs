using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using OSK.Inputs.Internal.Services;
using OSK.Inputs.Models.Configuration;
using OSK.Inputs.Models.Inputs;
using Xunit;

namespace OSK.Inputs.UnitTests.Internal.Services;
public class InputDeviceActionBuilderTests
{
    #region Variables

    private const string DefinitionName = "testDefinition";
    private static InputDeviceName DeviceName = new InputDeviceName("testController");
    private const string SchemeName = "testScheme";

    private readonly Mock<IInputDeviceConfiguration> _mockDeviceConfiguration;

    private readonly InputDeviceActionBuilder _builder;

    #endregion

    #region Constructors

    public InputDeviceActionBuilderTests()
    {
        _mockDeviceConfiguration = new();
        _mockDeviceConfiguration.SetupGet(m => m.DeviceName)
            .Returns(DeviceName);

        _builder = new("abc", "abc", _mockDeviceConfiguration.Object);
    }

    #endregion

    #region AssignInput

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void AssignInput_InvalidActionKey_ThrowsArgumentNullException(string? actionKey)
    {
        // Arrange/Act/Assert
        Assert.Throws<ArgumentNullException>(() => _builder.AssignInput(Mock.Of<IInput>(), InputPhase.Start, actionKey!));
    }

    [Fact]
    public void AssignInput_NullInput_ThrowsArgumentNullException()
    {
        // Arrange/Act/Assert
        Assert.Throws<ArgumentNullException>(() => _builder.AssignInput(null!, InputPhase.Start, "abc"));
    }

    [Fact]
    public void AssignInput_InputNotValidForConfiguration_ThrowsInvalidOperationException()
    {
        // Arrange
        _mockDeviceConfiguration.Setup(m => m.IsValidInput(It.IsAny<IInput>()))
            .Returns(false);

        var mockInput = new Mock<IInput>();
        mockInput.SetupGet(m => m.Name)
            .Returns("a");

        // Act/Assert
        Assert.Throws<InvalidOperationException>(() => _builder.AssignInput(mockInput.Object, InputPhase.Start, "abc"));
    }

    [Fact]
    public void AssignInput_CombinationInput_AnInputNotValidForConfiguration_ThrowsInvalidOperationException()
    {
        // Arrange
        _mockDeviceConfiguration.Setup(m => m.IsValidInput(It.Is<IInput>(input => input.Name == "a")))
            .Returns(true);

        _mockDeviceConfiguration.Setup(m => m.IsValidInput(It.Is<IInput>(input => input.Name == "a")))
            .Returns(false);

        var mockInputA = new Mock<HardwareInput>(1, "a", "a");
        var mockInputB = new Mock<HardwareInput>(1, "b", "b");

        var combinationInput = new CombinationInput(1, "Abc", "Abc", [mockInputA.Object, mockInputB.Object]);

        // Act/Assert
        Assert.Throws<InvalidOperationException>(() => _builder.AssignInput(combinationInput, InputPhase.Start, "abc"));
    }

    [Fact]
    public void AssignInput_CombinationInput_DuplicateInputsInCombinationInput_ThrowsInvalidOperationException()
    {
        // Arrange
        var mockInputA = new Mock<HardwareInput>(1, "a", "a");
        var combinationInput = new CombinationInput(1, "Abc", "Abc", [mockInputA.Object, mockInputA.Object]);

        // Act/Assert
        Assert.Throws<DuplicateNameException>(() => _builder.AssignInput(combinationInput, InputPhase.Start, "abc"));
    }

    [Fact]
    public void AssignInput_ActionKeyAlreadyAdded_ThrowsDuplicateNameException()
    {
        // Arrange
        _mockDeviceConfiguration.Setup(m => m.IsValidInput(It.IsAny<IInput>()))
            .Returns(true);

        var mockInputA = new Mock<IInput>();
        mockInputA.SetupGet(m => m.Name)
            .Returns("abc");

        _builder.AssignInput(mockInputA.Object, InputPhase.Start, "abc");

        // Act/Assert
        Assert.Throws<DuplicateNameException>(() => _builder.AssignInput(mockInputA.Object, InputPhase.Start, "abc"));
    }

    [Fact]
    public void AssignInput_CombinationInput_AllInputAreValid_ReturnsSuccessfully()
    {
        // Arrange
        _mockDeviceConfiguration.Setup(m => m.IsValidInput(It.IsAny<IInput>()))
            .Returns(true);

        var mockInputA = new Mock<HardwareInput>(1, "a", "a");
        var mockInputB = new Mock<HardwareInput>(1, "b", "b");

        var combinationInput = new CombinationInput(1, "Abc", "Abc", [mockInputA.Object, mockInputB.Object]);

        // Act/Assert
        _builder.AssignInput(combinationInput, InputPhase.Start, "abc");
    }

    [Fact]
    public void AssignInput_SingleInput_Valid_ReturnsSuccessfully()
    {
        // Arrange
        _mockDeviceConfiguration.Setup(m => m.IsValidInput(It.IsAny<IInput>()))
            .Returns(true);

        var mockInputA = new Mock<HardwareInput>(1, "a", "a");

        // Act/Assert
        _builder.AssignInput(mockInputA.Object, InputPhase.Start, "abc");
    }

    #endregion
}
