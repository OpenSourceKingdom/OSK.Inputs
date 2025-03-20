using System.Data;
using Moq;
using OSK.Inputs.Internal.Services;
using OSK.Inputs.Models.Configuration;
using OSK.Inputs.Models.Inputs;
using OSK.Inputs.UnitTests._Helpers;
using Xunit;

namespace OSK.Inputs.UnitTests.Internal.Services;
public class InputControllerBuilderTests
{
    #region Variables

    private readonly InputControllerName _controllerName;
    private readonly InputControllerBuilder _builder;

    #endregion

    #region Constructors

    public InputControllerBuilderTests()
    {
        _controllerName = new InputControllerName("Test");

        _builder = new InputControllerBuilder(_controllerName);
    }

    #endregion

    #region AddInput

    [Fact]
    public void AddInput_NullInput_ThrowsArgumentNullException()
    {
        // Arrnage/Act/Assert
        Assert.Throws<ArgumentNullException>(() => _builder.AddInput(null!));
    }

    [Fact]
    public void AddInput_DuplicateInput_ThrowsDuplicateNameException()
    {
        // Arrnage
        var mockInput = new Mock<IInput>();
        mockInput.SetupGet(x => x.Name)
            .Returns("test");

        _builder.AddInput(mockInput.Object);

        // Act/Assert
        Assert.Throws<DuplicateNameException>(() => _builder.AddInput(mockInput.Object));
    }

    [Fact]
    public void AddInput_Valid_ReturnsSuccessfully()
    {
        // Arrnage
        var mockInput = new Mock<IInput>();
        mockInput.SetupGet(x => x.Name)
            .Returns("test");

        // Act/Assert
        _builder.AddInput(mockInput.Object);
    }

    #endregion

    #region AddCombinationInput

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void AddCombinationInput_InvalidInputName_ThrowsArgumentNullException(string? inputName)
    {
        // Arrange/Act/Assert
        Assert.Throws<ArgumentNullException>(() => _builder.AddCombinationInput(inputName!));
    }

    [Theory]
    [InlineData(true, typeof(ArgumentNullException))]
    [InlineData(false, typeof(InvalidOperationException))]
    public void AddCombinationInput_EmptyCombinationInputNames_ThrowsExpectedException(bool useNull, Type type)
    {
        // Arrange
        var inputs = useNull ? null! : new string[0];

        // Act/Assert
        Assert.Throws(type, () => _builder.AddCombinationInput("valid", inputs!));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void AddCombinationInput_InvalidCombinationInputName_ThrowsInvalidOperationException(string? inputName)
    {
        // Arrange/Act/Assert
        Assert.Throws<InvalidOperationException>(() => _builder.AddCombinationInput("valid", inputName!));
    }

    [Fact]
    public void AddCombinationInput_DuplicateCombinationName_ReturnsSuccessfully()
    {
        // Arrange
        _builder.AddCombinationInput("valid", "abc", "def");

        // Act/Assert
        Assert.Throws<DuplicateNameException>(() => _builder.AddCombinationInput("valid", "a"));
    }

    [Fact]
    public void AddCombinationInput_Valid_ReturnsSuccessfully()
    {
        // Arrange/Act/Assert
        _builder.AddCombinationInput("valid", "abc", "def");
    }

    #endregion

    #region UseInputReader

    [Fact]
    public void UseInputReader_ReaderAlreadyTypeSet_ThrowsInvalidOperationException()
    {
        // Arrange
        _builder.UseInputReader<TestInputReader>();

        // Act/Assert
        Assert.Throws<InvalidOperationException>(() => _builder.UseInputReader<TestInputReader>());
    }

    [Fact]
    public void UseInputReader_NoReaderTypeSet_ReturnsSuccessfully()
    {
        // Arrange/Act/Assert
        _builder.UseInputReader<TestInputReader>();
    }

    #endregion

    #region UseValidation

    [Fact]
    public void UseValidation_ValidateFunctionSet_ThrowsInvalidOperationException()
    {
        // Arrange
        _builder.UseValidation(_ => true);

        // Act/Assert
        Assert.Throws<InvalidOperationException>(() => _builder.UseValidation(_ => false));
    }

    [Fact]
    public void UseValidation_NoValidatorFunctionSet_ReturnsSuccessfully()
    {
        // Arrange/Act/Assert
        _builder.UseValidation(_ => false);
    }

    #endregion

    #region BuildInputController

    [Fact]
    public void BuildInputController_NotInputReaderSet_ThrowsArgumentException()
    {
        // Arrange/Act/Assert
        Assert.Throws<ArgumentNullException>(_builder.BuildInputController);
    }

    [Fact]
    public void BuildInputController_CombinationInputReferencesInputsNotRegistered_ThrowsInvalidOperationException()
    {
        // Arrange
        _builder.UseInputReader<TestInputReader>();
        _builder.AddCombinationInput("abc", "a");

        // Act/Assert
        Assert.Throws<InvalidOperationException>(_builder.BuildInputController);
    }

    [Fact]
    public void BuildInputController_CombinationInputReferencesNonHardwareInput_ThrowsInvalidOperationException()
    {
        // Arrange
        _builder.UseInputReader<TestInputReader>();

        var mockInput = new Mock<IInput>();
        mockInput.SetupGet(m => m.Name)
            .Returns("a");

        _builder.AddInput(mockInput.Object);
        _builder.AddCombinationInput("abc", "a");

        // Act/Assert
        Assert.Throws<InvalidOperationException>(_builder.BuildInputController);
    }

    [Fact]
    public void BuildInputController_NullValidator_ReturnsSuccessfully()
    {
        // Arrange
        _builder.UseInputReader<TestInputReader>();

        var mockInputA = new Mock<HardwareInput>("a");
        var mockInputB = new Mock<HardwareInput>("b");

        _builder.AddInput(mockInputA.Object);
        _builder.AddInput(mockInputB.Object);
        _builder.AddCombinationInput("abc", "a", "b");

        // Act
        var controller = _builder.BuildInputController();

        // Assert
        Assert.Equal(_controllerName, controller.ControllerName);

        Assert.Equal(3, controller.Inputs.Count);
        Assert.Contains(mockInputA.Object, controller.Inputs);
        Assert.Contains(mockInputB.Object, controller.Inputs);
        Assert.Contains(controller.Inputs, input => input is CombinationInput combinationInput && 
            combinationInput.Inputs.Contains(mockInputA.Object) && combinationInput.Inputs.Contains(mockInputB.Object));
    }

    [Fact]
    public void BuildInputController_CustomValidator_ReturnsSuccessfully()
    {
        // Arrange
        _builder.UseInputReader<TestInputReader>();

        var mockInputA = new Mock<HardwareInput>("a");
        var mockInputB = new Mock<HardwareInput>("b");

        _builder.UseValidation(_ => true);

        _builder.AddInput(mockInputA.Object);
        _builder.AddInput(mockInputB.Object);
        _builder.AddCombinationInput("abc", "a", "b");

        // Act
        var controller = _builder.BuildInputController();

        // Assert
        Assert.Equal(_controllerName, controller.ControllerName);

        Assert.Equal(3, controller.Inputs.Count);
        Assert.Contains(mockInputA.Object, controller.Inputs);
        Assert.Contains(mockInputB.Object, controller.Inputs);
        Assert.Contains(controller.Inputs, input => input is CombinationInput combinationInput &&
            combinationInput.Inputs.Contains(mockInputA.Object) && combinationInput.Inputs.Contains(mockInputB.Object));
    }

    #endregion
}

