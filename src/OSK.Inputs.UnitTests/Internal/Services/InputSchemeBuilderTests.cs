using System.Data;
using Moq;
using OSK.Inputs.Internal.Services;
using OSK.Inputs.Models.Configuration;
using OSK.Inputs.Models.Inputs;
using OSK.Inputs.Options;
using OSK.Inputs.UnitTests._Helpers;
using Xunit;

namespace OSK.Inputs.UnitTests.Internal.Services;

public class InputSchemeBuilderTests
{
    #region Variables

    private const string DefinitionName = "testDefinition";
    private const string ControllerName = "testController";
    private const string SchemeName = "testScheme";

    private readonly Mock<IInputControllerConfiguration> _mockControllerConfiguration;

    private readonly InputSchemeBuilder _builder;

    #endregion

    #region Constructors

    public InputSchemeBuilderTests()
    {
        _mockControllerConfiguration = new();
        _mockControllerConfiguration.SetupGet(m => m.ControllerName)
            .Returns(ControllerName);

        _builder = new(DefinitionName, _mockControllerConfiguration.Object, SchemeName);
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
        Assert.Throws<ArgumentNullException>(() => _builder.AssignInput(actionKey!, Mock.Of<IInput>(), InputPhase.Start));
    }

    [Fact]
    public void AssignInput_NullInput_ThrowsArgumentNullException()
    {
        // Arrange/Act/Assert
        Assert.Throws<ArgumentNullException>(() => _builder.AssignInput("abc", null!, InputPhase.Start));
    }

    [Fact]
    public void AssignInput_InputNotValidForConfiguration_ThrowsInvalidOperationException()
    {
        // Arrange/Act/Assert
        Assert.Throws<InvalidOperationException>(() => _builder.AssignInput("abc", new TestInputB(), InputPhase.Start));
    }

    [Fact]
    public void AssignInput_CombinationInput_AnInputNotValidForConfiguration_ThrowsInvalidOperationException()
    {
        // Arrange
        var combinationInput = new CombinationInput("Abc", [new TestInputA(), new TestInputB()], new CombinationInputOptions(                                                                 ));

        // Act/Assert
        Assert.Throws<InvalidOperationException>(() => _builder.AssignInput("abc", combinationInput, InputPhase.Start));
    }

    [Fact]
    public void AssignInput_CombinationInput_DuplicateInputsInCombinationInput_ThrowsInvalidOperationException()
    {
        // Arrange
        var combinationInput = new CombinationInput("Abc", [new TestInputA(), new TestInputA()], new CombinationInputOptions());

        // Act/Assert
        Assert.Throws<DuplicateNameException>(() => _builder.AssignInput("abc", combinationInput, InputPhase.Start));
    }

    [Fact]
    public void AssignInput_ActionKeyAlreadyAdded_ThrowsDuplicateNameException()
    {
        // Arrange
        _mockControllerConfiguration.Setup(m => m.IsValidInput(It.IsAny<IInput>()))
            .Returns(true);

        _builder.AssignInput("abc", new TestInputA(), InputPhase.Start);

        // Act/Assert
        Assert.Throws<DuplicateNameException>(() => _builder.AssignInput("abc", new TestInputA(), InputPhase.Start));
    }

    [Fact]
    public void AssignInput_CombinationInput_AllInputAreValid_ReturnsSuccessfully()
    {
        // Arrange
        _mockControllerConfiguration.Setup(m => m.IsValidInput(It.IsAny<IInput>()))
            .Returns(true);
        var combinationInput = new CombinationInput("Abc", [new TestInputA(), new TestInputC()], new CombinationInputOptions());

        // Act/Assert
        _builder.AssignInput("abc", combinationInput, InputPhase.Start);
    }

    [Fact]
    public void AssignInput_SingleInput_Valid_ReturnsSuccessfully()
    {
        // Arrange
        _mockControllerConfiguration.Setup(m => m.IsValidInput(It.IsAny<IInput>()))
            .Returns(true);

        // Act/Assert
        _builder.AssignInput("abc", new TestInputA(), InputPhase.Start);
    }

    #endregion

    #region MakeDefault

    [Fact]
    public void MakeDefault_Valid_ReturnsSuccessfully()
    {
        // Arrange/Act/Assert
        _builder.MakeDefault();
    }

    #endregion
}
