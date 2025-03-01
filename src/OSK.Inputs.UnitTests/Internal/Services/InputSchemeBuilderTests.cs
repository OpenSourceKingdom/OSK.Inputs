using Moq;
using OSK.Inputs.Internal;
using OSK.Inputs.Internal.Services;
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

    private readonly InputReceiverDescriptor _testDescription;
    private readonly InputSchemeBuilder _builder;

    #endregion

    #region Constructors

    public InputSchemeBuilderTests()
    {
        _testDescription = new InputReceiverDescriptor("abc", typeof(TestInputReceiver), input => input is TestInputA || input is TestInputC);
        _builder = new(DefinitionName, ControllerName, SchemeName, [ _testDescription ]);
    }

    #endregion

    #region AssignInput

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void AssignInput_InvalidReceiverName_ThrowsArgumentNullException(string? receiverName)
    {
        // Arrange/Act/Assert
        Assert.Throws<ArgumentNullException>(() => _builder.AssignInput(receiverName!, "abc", Mock.Of<IInput>(), InputPhase.Start));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void AssignInput_InvalidActionKey_ThrowsArgumentNullException(string? actionKey)
    {
        // Arrange/Act/Assert
        Assert.Throws<ArgumentNullException>(() => _builder.AssignInput("abc", actionKey!, Mock.Of<IInput>(), InputPhase.Start));
    }

    [Fact]
    public void AssignInput_NullInput_ThrowsArgumentNullException()
    {
        // Arrange/Act/Assert
        Assert.Throws<ArgumentNullException>(() => _builder.AssignInput("abc", "abc", null!, InputPhase.Start));
    }

    [Fact]
    public void AssignInput_ReceiverNameNotInReceiverList_ThrowsInvalidOperationException()
    {
        // Arrange/Act/Assert
        Assert.Throws<InvalidOperationException>(() => _builder.AssignInput("abc", "abc", Mock.Of<IInput>(), InputPhase.Start));
    }

    [Fact]
    public void AssignInput_InputNotValidForDescriptor_ThrowsInvalidOperationException()
    {
        // Arrange/Act/Assert
        Assert.Throws<InvalidOperationException>(() => _builder.AssignInput(_testDescription.ReceiverName, "abc", new TestInputB(), InputPhase.Start));
    }

    [Fact]
    public void AssignInput_CombinationInput_AnInputNotValidForDescriptor_ThrowsInvalidOperationException()
    {
        // Arrange
        var combinationInput = new CombinationInput("Abc", [new TestInputA(), new TestInputB()], new CombinationInputOptions(                                                                 ));

        // Act/Assert
        Assert.Throws<InvalidOperationException>(() => _builder.AssignInput(_testDescription.ReceiverName, "abc", combinationInput, InputPhase.Start));
    }

    [Fact]
    public void AssignInput_CombinationInput_DuplicateInputsInCombinationInput_ThrowsInvalidOperationException()
    {
        // Arrange
        var combinationInput = new CombinationInput("Abc", [new TestInputA(), new TestInputA()], new CombinationInputOptions());

        // Act/Assert
        Assert.Throws<InvalidOperationException>(() => _builder.AssignInput(_testDescription.ReceiverName, "abc", combinationInput, InputPhase.Start));
    }

    [Fact]
    public void AssignInput_ActionKeyAlreadyAdded_ThrowsDuplicateException()
    {
        // Arrange
        _builder.AssignInput(_testDescription.ReceiverName, "abc", new TestInputA(), InputPhase.Start);

        // Act/Assert
        Assert.Throws<InvalidOperationException>(() => _builder.AssignInput(_testDescription.ReceiverName, "abc", new TestInputA(), InputPhase.Start));
    }

    [Fact]
    public void AssignInput_CombinationInput_AllInputAreValid_ReturnsSuccessfully()
    {
        // Arrange
        var combinationInput = new CombinationInput("Abc", [new TestInputA(), new TestInputC()], new CombinationInputOptions());

        // Act/Assert
        _builder.AssignInput(_testDescription.ReceiverName, "abc", combinationInput, InputPhase.Start);
    }

    [Fact]
    public void AssignInput_SingleInput_Valid_ReturnsSuccessfully()
    {
        // Arrange/Act/Assert
        _builder.AssignInput(_testDescription.ReceiverName, "abc", new TestInputA(), InputPhase.Start);
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
