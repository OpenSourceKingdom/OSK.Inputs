using System.Data;
using Moq;
using OSK.Inputs.Exceptions;
using OSK.Inputs.Internal.Services;
using OSK.Inputs.Models;
using OSK.Inputs.Models.Configuration;
using OSK.Inputs.Ports;
using Xunit;

namespace OSK.Inputs.UnitTests.Internal.Services;

public class InputDefinitionBuilderTests
{
    #region Variables

    private const string TestDefinitionName = "test123";
    private readonly Mock<IInputValidationService> _mockValidationService;

    private readonly InputDefinitionBuilder _builder;

    #endregion

    #region Constructors

    public InputDefinitionBuilderTests()
    {
        _mockValidationService = new Mock<IInputValidationService>();

        _builder = new(TestDefinitionName, _mockValidationService.Object);
    }

    #endregion

    #region AddAction

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void AddAction_InvalidActionKey_ThrowsArgumentNullException(string actionKey)
    {
        // Arrange/Act/Assert
        Assert.Throws<ArgumentNullException>(() => _builder.AddAction(new InputAction(actionKey, null)));
    }

    [Fact]
    public void AddAction_DuplicateActionKey_ThrowsDuplicateNameException()
    {
        // Arrange
        _builder.AddAction(new InputAction("test", null));

        // Act/Assert
        Assert.Throws<DuplicateNameException>(() => _builder.AddAction(new InputAction("test", null)));
    }

    [Fact]
    public void AddAction_Valid_ReturnsSuccessfully()
    {
        // Arrange/Act/Assert
        _builder.AddAction(new InputAction("test", null));
    }

    #endregion

    #region AddInputController

    [Fact]
    public void AddInputController_NullController_ThrowsArgumentNullException()
    {
        // Arrange/Act/Assert
        Assert.Throws<ArgumentNullException>(() => _builder.AddInputController(null!));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void AddInputController_InvalidControllerName_ThrowsArgumentNullException(string controllerName)
    {
        // Arrange/Act/Assert
        Assert.Throws<ArgumentNullException>(() => _builder.AddInputController(new InputControllerConfiguration(controllerName, [], [])));
    }

    [Fact]
    public void AddInputController_DuplicateControllerName_ThrowsDuplicateNameException()
    {
        // Arrange
        _builder.AddInputController(new InputControllerConfiguration("test", [], []));

        // Act/Assert
        Assert.Throws<DuplicateNameException>(() => _builder.AddInputController(new InputControllerConfiguration("test", [], [])));
    }

    [Fact]
    public void AddInputController_Valid_ReturnsSuccessfully()
    {
        // Arrange/Act/Assert
        _builder.AddInputController(new InputControllerConfiguration("test", [], []));
    }

    #endregion

    #region AllowCustomInputSchemes

    [Fact]
    public void AllowCustomInputSchemes_Valid_ReturnsSuccessfully()
    {
        // Arrange/Act/Assert
        _builder.AllowCustomInputSchemes();
    }

    #endregion

    #region Build

    [Fact]
    public void Build_ValidationServiceRetrunsErrorContextWithErrors_ThrowsInputValidationException()
    {
        // Arrange
        var errorContext = new InputValidationContext();
        errorContext.AddError("Bad", 1, "No way");

        _mockValidationService.Setup(m => m.ValidateInputDefinition(It.IsAny<InputDefinition>()))
            .Returns(errorContext);

        // Act/Assert
        Assert.Throws<InputValidationException>(() => _builder.Build());
    }

    [Theory]
    [InlineData("abc", null, false, "who")]
    [InlineData("abc", "description", true, "who")]
    public void Build_VariousValidStates_ReturnsInputDefinition(string action, string? description, bool allowCustomSchemes, string controllerName)
    {
        // Arrange

        _mockValidationService.Setup(m => m.ValidateInputDefinition(It.IsAny<InputDefinition>()))
            .Returns(new InputValidationContext());

        _builder.AddAction(new InputAction(action, description));
        _builder.AddInputController(new InputControllerConfiguration(controllerName, [], []));
        if (allowCustomSchemes)
        {
            _builder.AllowCustomInputSchemes();
        }

        // Act
        var definition = _builder.Build();

        // Assert
        Assert.NotNull(definition);
        Assert.Equal(TestDefinitionName, definition.Name);
        Assert.Single(definition.InputActions);
        Assert.Equal(action, definition.InputActions.First().ActionKey);
        Assert.Equal(description, definition.InputActions.First().Description);
        Assert.Single(definition.DefaultControllerConfigurations);
        Assert.Equal(controllerName, definition.DefaultControllerConfigurations.First().ControllerName);
        Assert.Equal(allowCustomSchemes, definition.AllowCustomInputSchemes);
    }

    #endregion
}
