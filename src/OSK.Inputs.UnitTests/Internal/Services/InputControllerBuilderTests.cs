using System.Data;
using OSK.Inputs.Internal;
using OSK.Inputs.Internal.Services;
using OSK.Inputs.UnitTests._Helpers;
using Xunit;

namespace OSK.Inputs.UnitTests.Internal.Services;

public class InputControllerBuilderTests
{
    #region Variables

    private const string TestDefinitionName = "TestDefinition";
    private const string TestControllerName = "TestController";

    private InputControllerBuilder _builder;

    #endregion

    #region Constructors

    public InputControllerBuilderTests()
    {
        _builder = new InputControllerBuilder(TestDefinitionName, TestControllerName);
    }

    #endregion

    #region AddInputReceiver

    [Fact]
    public void AddInputReceiver_NullInputReceiverDescription_ThrowsArgumentNullException()
    {
        // Arrange/Act/Assert
        Assert.Throws<ArgumentNullException>(() => _builder.AddInputReceiver(null!));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void AddInputReceiver_InvalidInputReceiverDescriptionName_ThrowsArgumentNullException(string? receiverName)
    {
        // Arrange/Act/Assert
        Assert.Throws<ArgumentNullException>(() => _builder.AddInputReceiver(new InputReceiverDescriptor(receiverName!, typeof(TestInputSystem), _ => true)));
    }

    [Fact]
    public void AddInputReceiver_NullInputReceiverDescriptionType_ThrowsArgumentNullException()
    {
        // Arrange/Act/Assert
        Assert.Throws<ArgumentNullException>(() => _builder.AddInputReceiver(new InputReceiverDescriptor("abc", null!, _ => true)));
    }

    [Fact]
    public void AddInputReceiver_InputReceiverDescriptionTypeNotAnIInputReceiver_ThrowsInvalidOperationException()
    {
        // Arrange/Act/Assert
        Assert.Throws<InvalidOperationException>(() => _builder.AddInputReceiver(new InputReceiverDescriptor("abc", typeof(TestSchemeRepository), _ => true)));
    }

    [Fact]
    public void AddInputReceiver_DuplicateReceiverDescriptionName_ThrowsDuplicateNameException()
    {
        // Arrange
        _builder.AddInputReceiver(new InputReceiverDescriptor("abc", typeof(TestInputSystem), _ => true));

        // Act/Assert
        Assert.Throws<DuplicateNameException>(() => _builder.AddInputReceiver(new InputReceiverDescriptor("abc", typeof(TestInputSystem), _ => true)));
    }

    [Fact]
    public void AddInputReceiver_Valid_ReturnsSuccessfully()
    {
        // Arrange/Act/Assert
        _builder.AddInputReceiver(new InputReceiverDescriptor("abc", typeof(TestInputSystem), _ => true));
    }

    #endregion

    #region AddInputScheme

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void AddInputScheme_InvalidInputSchemeName_ThrowsArgumentNullException(string? inputSchemeName)
    {
        // Arrange/Act/Assert
        Assert.Throws<ArgumentNullException>(() => _builder.AddInputScheme(inputSchemeName!, _ => { }));
    }

    [Fact]
    public void AddInputScheme_NullInputSchemeConfigurationAction_ThrowsArgumentNullException()
    {
        // Arrange/Act/Assert
        Assert.Throws<ArgumentNullException>(() => _builder.AddInputScheme("abc", null!));
    }

    [Fact]
    public void AddInputScheme_DuplicateInputSchemeName_ThrowsDuplicateNameException()
    {
        // Arrange
        _builder.AddInputScheme("abc", _ => { });

        // Act/Assert
        Assert.Throws<DuplicateNameException>(() => _builder.AddInputScheme("abc", _ => { }));
    }

    [Fact]
    public void AddInputScheme_Valid_ReturnsSuccessfully()
    {
        // Arrange/Act/Assert
        _builder.AddInputScheme("abc", _ => { });
    }

    #endregion

    #region Build (Internal)

    #endregion
}
