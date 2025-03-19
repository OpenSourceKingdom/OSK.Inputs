using System.Data;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using OSK.Inputs.Exceptions;
using OSK.Inputs.Internal.Services;
using OSK.Inputs.Models.Configuration;
using OSK.Inputs.Ports;
using OSK.Inputs.UnitTests._Helpers;
using Xunit;

namespace OSK.Inputs.UnitTests.Internal.Services;

public class InputSystemBuilderTests
{
    #region Variables

    private readonly Mock<IInputValidationService> _mockInputValidationService;
    private readonly Mock<IInputDefinitionBuilder> _definitionBuilder;
    private readonly IServiceCollection _services;

    private readonly InputSystemBuilder _builder;

    #endregion

    #region Constructors

    public InputSystemBuilderTests()
    {
        _definitionBuilder = new Mock<IInputDefinitionBuilder>();
        _mockInputValidationService = new Mock<IInputValidationService>();
        _services = new ServiceCollection();

        _builder = new(_services,
            definitionBuilderFactory: (_, _) => _definitionBuilder.Object,
            validationServiceFactory: () => _mockInputValidationService.Object);
    }

    #endregion

    #region AddInputDefinition

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void AddInputDefinition_InvalidDefinitionName_ThrowsArgumentNullException(string? definitionName)
    {
        // Arrange/Act/Assert
        Assert.Throws<ArgumentNullException>(() => _builder.AddInputDefinition(definitionName!, _ => { }));
    }

    [Fact]
    public void AddInputDefinition_NullAction_ThrowsArgumentNullException()
    {
        // Arrange/Act/Assert
        Assert.Throws<ArgumentNullException>(() => _builder.AddInputDefinition("test", null!));
    }

    [Fact]
    public void AddInputDefinition_DuplicateDefinitionName_ThrowsDuplicateNameException()
    {
        // Arrange
        _builder.AddInputDefinition("test", _ => { });

        // Act/Assert
        Assert.Throws<DuplicateNameException>(() => _builder.AddInputDefinition("test", _ => { }));
    }

    [Fact]
    public void AddInputDefinition_Valid_ReturnsSuccesfully()
    {
        // Arrange/Act/Assert
        _builder.AddInputDefinition("test", _ => { });
    }

    #endregion

    #region AddInputController

    [Fact]
    public void AddInputController_NullConfiguration_ThrowsArgumentNullException()
    {
        // Arrange/Act/Assert
        Assert.Throws<ArgumentNullException>(() => _builder.AddInputController(null!));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void AddInputController_InvalidControllerName_ThrowsArgumentNullException(string? controllerName)
    {
        // Arrange
        var mockControllerConfiguration = new Mock<IInputControllerConfiguration>();
        mockControllerConfiguration.SetupGet(m =>  m.ControllerName)
            .Returns(controllerName!);
        
        // Act/Assert
        Assert.Throws<ArgumentNullException>(() => _builder.AddInputController(mockControllerConfiguration.Object));
    }

    [Fact]
    public void AddInputController_DuplicateControllerName_ThrowsDuplicateNameException()
    {
        // Arrange
        var mockControllerConfiguration = new Mock<IInputControllerConfiguration>();
        mockControllerConfiguration.SetupGet(m => m.ControllerName)
            .Returns("abc");

        _builder.AddInputController(mockControllerConfiguration.Object);

        // Act/Assert
        Assert.Throws<DuplicateNameException>(() => _builder.AddInputController(mockControllerConfiguration.Object));
    }

    [Fact]
    public void AddInputController_Valid_ReturnsSuccesfully()
    {
        // Arrange
        var mockControllerConfiguration = new Mock<IInputControllerConfiguration>();
        mockControllerConfiguration.SetupGet(m => m.ControllerName)
            .Returns("abc");

        //Act/Assert
        _builder.AddInputController(mockControllerConfiguration.Object);
    }

    #endregion

    #region WithMaxLocalUsers

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void WithMaxLocalUsers_MaxLocalUsersLessThan1_ThrowsArgumentOutOfRangeException(int maxLocalUsers)
    {
        // Arrange/Act/Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => _builder.WithMaxLocalUsers(maxLocalUsers));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    public void WithMaxLocalUsers_Valid_ReturnsSuccessfully(int maxLocalUsers)
    {
        // Arrange/Act/Assert
        _builder.WithMaxLocalUsers(maxLocalUsers);
    }

    #endregion

    #region AllowCustomSchemes

    [Fact]
    public void AllowCustomSchemes_ReturnsSuccessfully()
    {
        // Arrange/Act/Assert
        _builder.AllowCustomSchemes();
    }

    #endregion

    #region UseInputSchemeRepository

    [Fact]
    public void UseInputSchemeRepository_Valid_ReturnsSuccessfully()
    {
        // Arrange/Act/Assert
        _builder.UseInputSchemeRepository<TestSchemeRepository>();
    }

    #endregion

    #region ApplyInputDefinitions (Internal)

    [Fact]
    public void ApplyInputDefinitions_ValidationServiceReturnsContextWithErrors_ThrowsInputValidationException()
    {
        // Arrange
        var definition = new InputDefinition("", [], []);
        _definitionBuilder.Setup(m => m.Build())
            .Returns(definition);

        _builder
            .AddInputDefinition("test", definitionBuilder => { })
            .UseInputSchemeRepository<TestSchemeRepository>();

        var context = new InputValidationContext("Bad");
        context.AddErrors(1, "Bad day");
        _mockInputValidationService.Setup(m => m.ValidateInputSystemConfiguration(It.IsAny<InputSystemConfiguration>()))
            .Returns(context);

        // Act/Assert
        Assert.Throws<InputValidationException>(_builder.ApplyInputSystemConfiguration);
    }

    [Fact]
    public void ApplyInputDefinitions_ValidationServiceReturnsContextWithoutErrors_ReturnsSuccessfully()
    {
        // Arrange
        var definition = new InputDefinition("", [], []);
        _definitionBuilder.Setup(m => m.Build())
            .Returns(definition);

        _mockInputValidationService.Setup(m => m.ValidateInputSystemConfiguration(It.IsAny<InputSystemConfiguration>()))
            .Returns(InputValidationContext.Success);

        var mockInputController = new Mock<IInputControllerConfiguration>();
        mockInputController.SetupGet(m => m.ControllerName)
            .Returns("abc");

        _builder
            .AddInputController(mockInputController.Object)
            .AddInputDefinition("test", definitionBuilder => { })
            .WithMaxLocalUsers(12)
            .AllowCustomSchemes()
            .UseInputSchemeRepository<TestSchemeRepository>(); ;

        // Act
        _builder.ApplyInputSystemConfiguration();

        // Assert
        using var serviceProvider = _services.BuildServiceProvider();
        var inputSystemConfiguration = serviceProvider.GetRequiredService<InputSystemConfiguration>();

        Assert.Single(inputSystemConfiguration.InputDefinitions);
        Assert.Equal(definition, inputSystemConfiguration.InputDefinitions.ElementAt(0));

        Assert.Single(inputSystemConfiguration.SupportedInputControllers);
        Assert.Equal(mockInputController.Object, inputSystemConfiguration.SupportedInputControllers.ElementAt(0));

        Assert.Equal(12, inputSystemConfiguration.MaxLocalUsers);
        Assert.True(inputSystemConfiguration.AllowCustomInputSchemes);

        var schemeRepository = serviceProvider.GetRequiredService<IInputSchemeRepository>();
        Assert.IsType<TestSchemeRepository>(schemeRepository);
    }

    #endregion
}
