using Microsoft.Extensions.Options;
using Moq;
using OSK.Extensions.Object.DeepEquals;
using OSK.Functions.Outputs.Abstractions;
using OSK.Functions.Outputs.Mocks;
using OSK.Inputs.Internal;
using OSK.Inputs.Internal.Services;
using OSK.Inputs.Models;
using OSK.Inputs.Models.Configuration;
using OSK.Inputs.Models.Runtime;
using OSK.Inputs.Options;
using OSK.Inputs.Ports;
using OSK.Inputs.UnitTests._Helpers;
using Xunit;

namespace OSK.Inputs.UnitTests.Internal.Services;

public class InputManagerTests
{
    #region Variables

    private readonly List<InputDefinition> _inputDefinitions;

    private readonly InputDefinition _testDefinitionWithCustomSchemes;
    private readonly InputDefinition _testDefinitionNoCustomSchemes;

    private readonly Mock<IInputSchemeRepository> _mockInputSchemeRepository;
    private readonly Mock<IInputValidationService> _mockValidationService;
    private readonly Mock<IServiceProvider> _mockServiceProvider;
    private readonly IOutputFactory _outputFactory;

    private readonly InputManager _manager;

    #endregion

    #region Constructors

    public InputManagerTests()
    {
        _inputDefinitions = new List<InputDefinition>();
        _testDefinitionWithCustomSchemes = new("Test", true, [
            new InputControllerConfiguration("TestController", [], 
                [ new BuiltInInputScheme("Test", "TestController", "TestScheme", [], false) ]) ], 
              []);
        _testDefinitionNoCustomSchemes = new("Test2", false, [
            new InputControllerConfiguration("TestController2", [new InputReceiverDescriptor("test", typeof(TestInputSystem), _ => true)],
                [ new BuiltInInputScheme("Test2", "TestController2", "TestScheme2", [new InputReceiverConfiguration("test", [])], false) ]) ],
              []);

        _mockInputSchemeRepository = new();
        _mockValidationService = new();
        _mockServiceProvider = new();
        _outputFactory = new MockOutputFactory();

        _manager = new(_inputDefinitions,
            _mockValidationService.Object, _mockInputSchemeRepository.Object, _mockServiceProvider.Object,
            _outputFactory);
    }

    #endregion

    #region GetInputDefinitionsAsync

    [Fact]
    public async Task GetInputDefinitionsAsync_InputDefinitionDoesNotAllowCustomSchemes_ReturnsBuiltInList()
    {
        // Arrange
        _inputDefinitions.Add(_testDefinitionNoCustomSchemes);

        // Act
        var getOutput = await _manager.GetInputDefinitionsAsync();

        // Assert
        Assert.True(getOutput.IsSuccessful);
        Assert.Single(getOutput.Value);

        var definition = getOutput.Value.First();
        Assert.True(definition.DeepEquals(_testDefinitionNoCustomSchemes));
    }

    [Fact]
    public async Task GetInputDefinitionsAsync_InputDefinitionDoesAllowCustomSchemes_ReturnsBuiltInListWithCustomSchemes()
    {
        // Arrange
        _inputDefinitions.Add(_testDefinitionNoCustomSchemes);
        _inputDefinitions.Add(_testDefinitionWithCustomSchemes);

        var customScheme = new InputScheme(_testDefinitionWithCustomSchemes.Name,
            _testDefinitionWithCustomSchemes.DefaultControllerConfigurations.First().ControllerName,
            "custom", [], false);

        _mockInputSchemeRepository.Setup(m => m.GetInputSchemesAsync(It.Is<string>(name => name == _testDefinitionWithCustomSchemes.Name),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(_outputFactory.Succeed((IEnumerable<InputScheme>)new List<InputScheme>() { customScheme }));

        // Act
        var getOutput = await _manager.GetInputDefinitionsAsync();

        // Assert
        Assert.True(getOutput.IsSuccessful);
        Assert.Equal(2, getOutput.Value.Count());

        var definitionBuiltIn = getOutput.Value.First(definition => definition.Name == _testDefinitionNoCustomSchemes.Name);
        var definitionCustomSchemes = getOutput.Value.First(definition => definition.Name == _testDefinitionWithCustomSchemes.Name);

        Assert.True(_testDefinitionNoCustomSchemes.DeepEquals(definitionBuiltIn));

        // Definition should now include a new custom scheme
        Assert.False(definitionCustomSchemes.DeepEquals(_testDefinitionWithCustomSchemes));

        var controller = definitionCustomSchemes.DefaultControllerConfigurations.First();
        Assert.Equal(2, controller.InputSchemes.Count());
        
        var actualCustomScheme = controller.InputSchemes.Last();
        Assert.True(customScheme.DeepEquals(actualCustomScheme));
    }

    #endregion

    #region GetInputHandlerAsync

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public async Task GetInputHandlerAsync_EmptyDefinitionNames_ThrowsArgumentNullException(string? definitionName)
    {
        // Arrange/Act/Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _manager.GetInputHandlerAsync(definitionName!, 1));
    }

    [Fact]
    public async Task GetInputHandlerAsync_InputDefinitionDoesNotExist_ReturnsNotFound()
    {
        // Arrange/Act
        var getOutput = await _manager.GetInputHandlerAsync("NotADefomotopm", 1);

        // Assert
        Assert.False(getOutput.IsSuccessful);
        Assert.Equal(OutputSpecificityCode.DataNotFound, getOutput.StatusCode.SpecificityCode);
    }

    [Fact]
    public async Task GetInputHandlerAsync_InputDefinitionAllowsCustomSchemes_GetSchemesReturnsError_ReturnsError()
    {
        // Arrange
        var expectedSpecificityCode = OutputSpecificityCode.NotAuthenticated;
        _inputDefinitions.Add(_testDefinitionWithCustomSchemes);

        _mockInputSchemeRepository.Setup(m => m.GetInputSchemesAsync(It.IsAny<string>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(_outputFactory.Fail<IEnumerable<InputScheme>>("Bad Day", expectedSpecificityCode));

        // Act
        var getOutput = await _manager.GetInputHandlerAsync(_testDefinitionWithCustomSchemes.Name, 1);

        // Assert
        Assert.False(getOutput.IsSuccessful);
        Assert.Equal(expectedSpecificityCode, getOutput.StatusCode.SpecificityCode);
    }

    [Fact]
    public async Task GetInputHandlerAsync_GetActiveSchemesReturnsError_ReturnsError()
    {
        // Arrange
        var expectedSpecificityCode = OutputSpecificityCode.InsufficientStorage;
        _inputDefinitions.Add(_testDefinitionNoCustomSchemes);

        _mockInputSchemeRepository.Setup(m => m.GetActiveInputSchemesAsync(It.IsAny<string>(), It.IsAny<int>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(_outputFactory.Fail<IEnumerable<ActiveInputScheme>>("Bad Day", expectedSpecificityCode));

        // Act
        var getOutput = await _manager.GetInputHandlerAsync(_testDefinitionNoCustomSchemes.Name, 1);

        // Assert
        Assert.False(getOutput.IsSuccessful);
        Assert.Equal(expectedSpecificityCode, getOutput.StatusCode.SpecificityCode);
    }

    [Fact]
    public async Task GetInputHandlerAsync_InvalidControllerScheme_ReturnsError()
    {
        // Arrange
        var expectedSpecificityCode = OutputSpecificityCode.InvalidParameter;
        _inputDefinitions.Add(_testDefinitionNoCustomSchemes);
        var controller = _testDefinitionNoCustomSchemes.DefaultControllerConfigurations.First();
        var scheme = controller.InputSchemes.First();

        _mockInputSchemeRepository.Setup(m => m.GetActiveInputSchemesAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_outputFactory.Succeed((IEnumerable<ActiveInputScheme>)[new ActiveInputScheme(1, _testDefinitionNoCustomSchemes.Name, controller.ControllerName, scheme.SchemeName)]));

        _mockValidationService.Setup(m => m.ValidateInputScheme(It.IsAny<InputDefinition>(), It.IsAny<InputScheme>()))
            .Returns((InputDefinition _, InputScheme _) =>
            {
                var errorContext = new InputValidationContext();
                errorContext.AddError("Bad Day", 1, "Unable to do it man");

                return errorContext;
            });

        // Act
        var getOutput = await _manager.GetInputHandlerAsync(_testDefinitionNoCustomSchemes.Name, 1);

        // Assert
        Assert.False(getOutput.IsSuccessful);
        Assert.Equal(expectedSpecificityCode, getOutput.StatusCode.SpecificityCode);
    }

    [Fact]
    public async Task GetInputHandlerAsync_Valid_ReturnsSuccessfully()
    {
        // Arrange
        _inputDefinitions.Add(_testDefinitionNoCustomSchemes);
        var controller = _testDefinitionNoCustomSchemes.DefaultControllerConfigurations.First();
        var scheme = controller.InputSchemes.First();

        _mockInputSchemeRepository.Setup(m => m.GetActiveInputSchemesAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_outputFactory.Succeed((IEnumerable<ActiveInputScheme>)[new ActiveInputScheme(1, _testDefinitionNoCustomSchemes.Name, controller.ControllerName, scheme.SchemeName)]));

        _mockValidationService.Setup(m => m.ValidateInputScheme(It.IsAny<InputDefinition>(), It.IsAny<InputScheme>()))
            .Returns(new InputValidationContext());

        _mockServiceProvider.Setup(m => m.GetService(It.Is<Type>(t => t == typeof(IOptions<InputHandlerOptions>))))
            .Returns(() =>
            {
                var mockOptions = new Mock<IOptions<InputHandlerOptions>>();
                mockOptions.SetupGet(m => m.Value)
                    .Returns(new InputHandlerOptions());

                return mockOptions.Object;
            });

        // Act
        var getOutput = await _manager.GetInputHandlerAsync(_testDefinitionNoCustomSchemes.Name, 1);

        // Assert
        Assert.True(getOutput.IsSuccessful);
    }

    #endregion

    #region SaveInputSchemeAsync

    [Fact]
    public async Task SaveInputSchemeAsync_NullInputScheme_ThrowArgumentNullException()
    {
        // Arrange/Act/Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _manager.SaveInputSchemeAsync(null!));
    }


    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public async Task SaveInputSchemeAsync_EmptyInputSchemeDefinitionName_ReturnsError(string? definitionName)
    {
        // Arrange
        var scheme = new InputScheme(definitionName!, "abc", "abc", [], false);

        // Act
        var saveOutput = await _manager.SaveInputSchemeAsync(scheme);

        // Assert
        Assert.False(saveOutput.IsSuccessful);
    }

    [Fact]
    public async Task SaveInputSchemeAsync_InputDefinitionDoesNotAllowCustomSchemes_ReturnsError()
    {
        // Arrange
        _inputDefinitions.Add(_testDefinitionNoCustomSchemes);
        var scheme = new InputScheme(_testDefinitionNoCustomSchemes.Name, "abc", "abc", [], false);

        // Act
        var saveOutput = await _manager.SaveInputSchemeAsync(scheme);

        // Assert
        Assert.False(saveOutput.IsSuccessful);
    }

    [Fact]
    public async Task SaveInputSchemeAsync_InputSchemeHasValidationErrors_ReturnsError()
    {
        // Arrange
        _inputDefinitions.Add(_testDefinitionNoCustomSchemes);
        var scheme = new InputScheme(_testDefinitionNoCustomSchemes.Name, "abc", "abc", [], false);

        var validationContext = new InputValidationContext();
        validationContext.AddError("test", 1, "Oh ya");

        _mockValidationService.Setup(m => m.ValidateInputScheme(It.IsAny<InputDefinition>(), It.IsAny<InputScheme>()))
            .Returns(validationContext);

        // Act
        var saveOutput = await _manager.SaveInputSchemeAsync(scheme);

        // Assert
        Assert.False(saveOutput.IsSuccessful);
    }

    [Fact]
    public async Task SaveInputSchemeAsync_InputSchemeHasNoValidationErrors_CallsRepository_ReturnsRepositoryResult()
    {
        // Arrange
        _inputDefinitions.Add(_testDefinitionNoCustomSchemes);
        var scheme = new InputScheme(_testDefinitionNoCustomSchemes.Name, "abc", "abc", [], false);

        var validationContext = new InputValidationContext();

        _mockInputSchemeRepository.Setup(m => m.SaveInputSchemeAsync(It.IsAny<string>(), It.IsAny<InputScheme>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string _, InputScheme scheme, CancellationToken _) =>
            {
                return _outputFactory.Succeed(scheme);
            });

        _mockValidationService.Setup(m => m.ValidateInputScheme(It.IsAny<InputDefinition>(), It.IsAny<InputScheme>()))
            .Returns(validationContext);

        // Act
        var saveOutput = await _manager.SaveInputSchemeAsync(scheme);

        // Assert
        Assert.False(saveOutput.IsSuccessful);
    }

    #endregion

    #region DeleteInputSchemeAsync

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public async Task DeleteInputSchemeAsync_EmptyInputDefinitionName_ThrowsArgumentNullException(string? definitionName)
    {
        // Arrange/Act/Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _manager.DeleteInputSchemeAsync(definitionName!, "abc", "def"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public async Task DeleteInputSchemeAsync_EmptyInputControllerName_ThrowsArgumentNullException(string? controllerName)
    {
        // Arrange/Act/Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _manager.DeleteInputSchemeAsync("abc", controllerName!, "def"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public async Task DeleteInputSchemeAsync_EmptyInputSchemeName_ThrowsArgumentNullException(string? schemeName)
    {
        // Arrange/Act/Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _manager.DeleteInputSchemeAsync("abc", "def", schemeName!));
    }

    [Fact]
    public async Task DeleteInputSchemeAsync_InputSchemeIsBuiltIn_ReturnsError()
    {
        // Arrange
        _inputDefinitions.Add(_testDefinitionNoCustomSchemes);
        var controllerName = _testDefinitionNoCustomSchemes.DefaultControllerConfigurations.First().ControllerName;
        var schemeName = _testDefinitionNoCustomSchemes.DefaultControllerConfigurations.First().InputSchemes.First().SchemeName;

        // Act
        var deleteOutput = await _manager.DeleteInputSchemeAsync(_testDefinitionNoCustomSchemes.Name, controllerName, schemeName);

        // Assert
        Assert.False(deleteOutput.IsSuccessful);
    }

    [Fact]
    public async Task DeleteInputSchemeAsync_InputDefinitionDoesNotExist_ReturnsSuccessfully()
    {
        // Arrange/Act
        var deleteOutput = await _manager.DeleteInputSchemeAsync("abc", "abc", "abc");

        // Assert
        Assert.True(deleteOutput.IsSuccessful);
    }

    [Fact]
    public async Task DeleteInputSchemeAsync_InputControllerDoesNotExist_ReturnsSuccessfully()
    {
        // Arrange
        _inputDefinitions.Add(_testDefinitionNoCustomSchemes);

        // Act
        var deleteOutput = await _manager.DeleteInputSchemeAsync(_testDefinitionNoCustomSchemes.Name, "abc", "abc");

        // Assert
        Assert.True(deleteOutput.IsSuccessful);
    }

    [Fact]
    public async Task DeleteInputSchemeAsync_InputSchemeDoesNotExist_DefinitionDoesNotAllowCustomSchemes_ReturnsSuccessfully()
    {
        // Arrange
        _inputDefinitions.Add(_testDefinitionNoCustomSchemes);
        var controllerName = _testDefinitionNoCustomSchemes.DefaultControllerConfigurations.First().ControllerName;

        // Act
        var deleteOutput = await _manager.DeleteInputSchemeAsync(_testDefinitionNoCustomSchemes.Name, controllerName, "abc");

        // Assert
        Assert.True(deleteOutput.IsSuccessful);
    }

    [Fact]
    public async Task DeleteInputSchemeAsync_InputSchemeDoesNotExistOnDefinition_DefinitionAllowsCustomSchemes_CallsRepository_ReturnsOutput()
    {
        // Arrange
        _inputDefinitions.Add(_testDefinitionWithCustomSchemes);
        var controllerName = _testDefinitionWithCustomSchemes.DefaultControllerConfigurations.First().ControllerName;
        var schemeName = _testDefinitionWithCustomSchemes.DefaultControllerConfigurations.First().InputSchemes.First().SchemeName;

        _mockInputSchemeRepository.Setup(m => m.DeleteInputSchemeAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(_outputFactory.Succeed());

        // Act
        var deleteOutput = await _manager.DeleteInputSchemeAsync(_testDefinitionNoCustomSchemes.Name, controllerName, schemeName);

        // Assert
        Assert.True(deleteOutput.IsSuccessful);
    }

    [Fact]
    public async Task DeleteInputSchemeAsync_InputSchemeDoesNotExistOnDefinition_DefinitionAllowsCustomSchemes_CallsRepository_MasksNotFoundAsSuccess()
    {
        // Arrange
        _inputDefinitions.Add(_testDefinitionWithCustomSchemes);
        var controllerName = _testDefinitionWithCustomSchemes.DefaultControllerConfigurations.First().ControllerName;
        var schemeName = _testDefinitionWithCustomSchemes.DefaultControllerConfigurations.First().InputSchemes.First().SchemeName;

        _mockInputSchemeRepository.Setup(m => m.DeleteInputSchemeAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(_outputFactory.NotFound());

        // Act
        var deleteOutput = await _manager.DeleteInputSchemeAsync(_testDefinitionNoCustomSchemes.Name, controllerName, schemeName);

        // Assert
        Assert.True(deleteOutput.IsSuccessful);
    }

    #endregion
}
