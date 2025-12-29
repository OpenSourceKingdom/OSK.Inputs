using System.Data;
using Moq;
using OSK.Inputs.Internal.Services;
using OSK.Inputs.Models.Configuration;
using OSK.Inputs.Models.Inputs;
using OSK.Inputs.UnitTests._Helpers;
using Xunit;

namespace OSK.Inputs.UnitTests.Internal.Services;

public class InputDefinitionBuilderTests
{
    #region Variables

    private const string TestDefinitionName = "test123";

    private readonly Mock<IInputDeviceConfiguration> _mockDeviceConfiguration;
    private readonly Mock<IInputDeviceConfiguration> _mockDeviceConfiguration2;

    private readonly InputDefinitionBuilder _builder;

    #endregion

    #region Constructors

    public InputDefinitionBuilderTests()
    {
        _mockDeviceConfiguration = new();
        _mockDeviceConfiguration.SetupGet(m => m.DeviceName)
            .Returns(new InputDeviceName("abc"));

        _mockDeviceConfiguration2 = new();
        _mockDeviceConfiguration2.SetupGet(m => m.DeviceName)
            .Returns(new InputDeviceName("def"));

        _builder = new(TestDefinitionName, [ _mockDeviceConfiguration.Object, _mockDeviceConfiguration2.Object ]);
    }

    #endregion

    #region AddAction

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void AddAction_InvalidActionKey_ThrowsArgumentNullException(string? actionKey)
    {
        // Arrange
        var action = new InputAction(actionKey!, _ => ValueTask.CompletedTask , null);

        // Act/Assert
        Assert.Throws<ArgumentNullException>(() => _builder.AddAction(action));
    }

    [Fact]
    public void AddAction_DuplicateActionKey_ThrowsDuplicateNameException()
    {
        // Arrange
        _builder.AddAction(new InputAction("test", _ => ValueTask.CompletedTask , null));

        // Act/Assert
        Assert.Throws<DuplicateNameException>(() => _builder.AddAction(new InputAction("test", _ => ValueTask.CompletedTask , null)));
    }

    [Fact]
    public void AddAction_NullActionExecutor_ThrowsInvalidOperaitonException()
    {
        // Arrange/Act/Assert
        Assert.Throws<InvalidOperationException>(() => _builder.AddAction(new InputAction("test", null!, null)));
    }

    [Fact]
    public void AddAction_Valid_ReturnsSuccessfully()
    {
        // Arrange/Act/Assert
        _builder.AddAction(new InputAction("test", _ => ValueTask.CompletedTask , null));
    }

    #endregion

    #region AddInputScheme

    [Fact]
    public void AddInputScheme_ControllerConfigurationActionNull_ThrowsArgumentNullException()
    {
        // Arrange/Act/Assert
        Assert.Throws<ArgumentNullException>(() => _builder.AddInputScheme("abc", null!));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void AddInputScheme_InvalidSchemeName_ThrowsArgumentException(string? schemeName)
    {
        // Arrange/Act/Assert
        Assert.Throws<ArgumentException>(() => _builder.AddInputScheme(schemeName!, _ => { }));
    }

    [Fact]
    public void AddInputScheme_SchemeNameAlreadyAdded_ThrowsDuplicateNameException()
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

    #region Build

    [Fact]
    public void Build_Valid_ReturnsInputDefinition()
    {
        // Arrange
        var action1 = new InputAction("Fire", _ => ValueTask.CompletedTask, "Triggers a boom");
        var action2 = new InputAction("Reload", _ => ValueTask.CompletedTask, "Does a thing for resupply");
        _builder.AddAction(action1);
        _builder.AddAction(action2);

        _builder.AddInputScheme("Scheme1", builder => 
        {
            builder.AddDevice<IInput>(_mockDeviceConfiguration.Object.DeviceName, _ => { });
        });
        _builder.AddInputScheme("Scheme2", builder => 
        {
            builder.AddDevice<IInput>(_mockDeviceConfiguration.Object.DeviceName, _ => { });
        });

        _builder.AddInputScheme("Scheme1", builder => 
        { 
            builder.AddDevice<IInput>(_mockDeviceConfiguration2.Object.DeviceName, _ => { }); 
        });

        // Act
        var definition = _builder.Build();

        // Assert
        Assert.NotNull(definition);
        Assert.Equal(TestDefinitionName, definition.Name);
        
        Assert.Equal(2, definition.InputActions.Count());
        Assert.Contains(action1, definition.InputActions);
        Assert.Contains(action2, definition.InputActions);

        Assert.Equal(3, definition.InputSchemes.Count());
    }

    [Fact]
    public void Build_TestRegistrationService_Valid_ReturnsInputDefinition()
    {
        // Arrange
        _builder.RegisterActions<TestRegistrationService>();

        // Act
        var definition = _builder.Build();

        // Assert
        Assert.NotNull(definition);

        Assert.Equal(2, definition.InputActions.Count);

        Assert.True(definition.InputActions.Count(action => nameof(TestRegistrationService.ValidMethodA).Equals(action.ActionKey)) == 1);
        Assert.True(definition.InputActions.Count(action => "SpecialAction".Equals(action.ActionKey)) == 1);
    }

    #endregion
}
