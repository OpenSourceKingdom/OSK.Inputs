using Moq;
using OSK.Extensions.Object.DeepEquals;
using OSK.Functions.Outputs.Abstractions;
using OSK.Functions.Outputs.Logging.Abstractions;
using OSK.Functions.Outputs.Mocks;
using OSK.Inputs.Internal;
using OSK.Inputs.Internal.Services;
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

    private readonly Mock<IInputDeviceConfiguration> _mockDeviceConfiguration;

    private readonly Mock<IInputValidationService> _mockValidationService;
    private readonly Mock<IInputSchemeRepository> _mockInputSchemeRepository;
    private readonly Mock<IInputReaderProvider> _mockInputReaderProvider;
    private readonly Mock<IServiceProvider> _mockServiceProvider;
    private readonly IOutputFactory<InputManager> _outputFactory;

    private readonly InputManager _4UserManagerWithCustomSchemes;
    private readonly InputManager _2UserManagerWithNoCustomSchemes;

    #endregion

    #region Constructors

    public InputManagerTests()
    {
        _mockDeviceConfiguration = new();
        _mockDeviceConfiguration.SetupGet(m => m.InputReaderType)
            .Returns(typeof(TestInputReader));
        _mockDeviceConfiguration.SetupGet(m => m.DeviceName)
            .Returns(new InputDeviceName("TestController"));

        _mockDeviceConfiguration.SetupGet(m => m.Inputs)
            .Returns([]);

        _mockValidationService = new();
        _mockInputSchemeRepository = new();
        _mockInputReaderProvider = new();
        _mockServiceProvider = new();
        _outputFactory = new MockOutputFactory<InputManager>();

        var testDefinition = new InputDefinition("Test", [], [ new BuiltInInputScheme("Test", "TestScheme", false, [new(_mockDeviceConfiguration.Object.DeviceName, [])]) ]);
        var testDefinition2 = new InputDefinition("Test2", [], [new BuiltInInputScheme("Test", "TestScheme", false, [])]);

        var inputSystemConfigurationA = new InputSystemConfiguration([ testDefinition, testDefinition2 ],
            [new InputControllerConfiguration("abc", [ _mockDeviceConfiguration.Object.DeviceName ])],
            [_mockDeviceConfiguration.Object], false, 2);
        _2UserManagerWithNoCustomSchemes = new(inputSystemConfigurationA, _mockValidationService.Object, _mockInputSchemeRepository.Object, _mockInputReaderProvider.Object,
            _mockServiceProvider.Object, _outputFactory);

        var inputSystemConfigurationB = new InputSystemConfiguration([ testDefinition, testDefinition2 ],
            [new InputControllerConfiguration("abc", [_mockDeviceConfiguration.Object.DeviceName])], 
            [ _mockDeviceConfiguration.Object ], true, 4);
        _4UserManagerWithCustomSchemes = new(inputSystemConfigurationB, _mockValidationService.Object, _mockInputSchemeRepository.Object, _mockInputReaderProvider.Object,
            _mockServiceProvider.Object, _outputFactory);

        DeepEqualsConfiguration.CustomConfiguration = builder =>
        {
            builder.WithExecutionOptions(executor =>
            {
                executor.ThrowOnFailure = true;
            });
            builder.WithEnumerableComparisonOptions(comparison =>
            {
                comparison.EnforceEnumerableOrdering = false;
            });
        };
    } 

    #endregion

    #region GetInputDefinitionsAsync

    [Fact]
    public async Task GetInputDefinitionsAsync_InputDefinitionDoesNotAllowCustomSchemes_ReturnsBuiltInList()
    {
        // Arrange/Act
        var getOutput = await _2UserManagerWithNoCustomSchemes.GetInputDefinitionsAsync();

        // Assert
        Assert.True(getOutput.IsSuccessful);
        Assert.Equal(_2UserManagerWithNoCustomSchemes.Configuration.InputDefinitions.Count(), getOutput.Value.Count());

        var definition = getOutput.Value.First();
        Assert.True(definition.DeepEquals(_2UserManagerWithNoCustomSchemes.Configuration.InputDefinitions.First()));

        var definition2 = getOutput.Value.Last();
        Assert.True(definition2.DeepEquals(_2UserManagerWithNoCustomSchemes.Configuration.InputDefinitions.Last()));
    }

    [Fact]
    public async Task GetInputDefinitionsAsync_InputDefinitionDoesAllowCustomSchemes_ReturnsBuiltInListWithCustomSchemes()
    {
        // Arrange
        var testDefinition = _4UserManagerWithCustomSchemes.Configuration.InputDefinitions.First();
        var testDefinition2 = _4UserManagerWithCustomSchemes.Configuration.InputDefinitions.Last();
         
        var customScheme = new InputScheme(testDefinition.Name,
            "custom", false, [new InputDeviceActionMap(_4UserManagerWithCustomSchemes.Configuration.SupportedInputDevices.First().DeviceName, [])]);

        _mockInputSchemeRepository.Setup(m => m.GetCustomInputSchemesAsync(It.Is<string>(name => name == testDefinition.Name),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(_outputFactory.Succeed((IEnumerable<InputScheme>)new List<InputScheme>() { customScheme }));

        _mockInputSchemeRepository.Setup(m => m.GetCustomInputSchemesAsync(It.Is<string>(name => name == testDefinition2.Name),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(_outputFactory.Succeed((IEnumerable<InputScheme>)new List<InputScheme>()));

        // Act
        var getDefinitionsOutput = await _4UserManagerWithCustomSchemes.GetInputDefinitionsAsync();

        // Assert
        Assert.True(getDefinitionsOutput.IsSuccessful);
        Assert.Equal(_4UserManagerWithCustomSchemes.Configuration.InputDefinitions.Count, getDefinitionsOutput.Value.Count());

        var expectedCustomDefinition = testDefinition.Clone([customScheme]);
        Assert.True(expectedCustomDefinition.DeepEquals(getDefinitionsOutput.Value.First()));
        Assert.True(testDefinition2.DeepEquals(getDefinitionsOutput.Value.Last()));
    }

    #endregion

    #region SetUserActiveInputDefinitionAsync

    [Fact]
    public async Task SetUserActiveInputDefinitionAsync_UserIdDoesNotExistOnInputManager_ReturnsNotFound()
    {
        // Arrange/Act
        var result = await _2UserManagerWithNoCustomSchemes.SetUserActiveInputDefinitionAsync(1, "abc");

        // Assert
        Assert.False(result.IsSuccessful);
        Assert.Equal(OutputSpecificityCode.DataNotFound, result.StatusCode.SpecificityCode);
    }

    [Fact]
    public async Task SetUserActiveInputDefinitionAsync_InputDefinitionNameDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        _mockInputReaderProvider.Setup(m => m.GetInputReader(It.IsAny<IInputDeviceConfiguration>(),
            It.IsAny<InputDeviceIdentifier>()))
            .Returns(Mock.Of<IInputReader>());

        _mockInputSchemeRepository.Setup(m => m.GetCustomInputSchemesAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_outputFactory.Succeed(Enumerable.Empty<InputScheme>()));

        _mockInputSchemeRepository.Setup(m => m.GetActiveInputSchemesAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_outputFactory.Succeed(Enumerable.Empty<ActiveInputScheme>()));

        await _2UserManagerWithNoCustomSchemes.JoinUserAsync(1, JoinUserOptions.Default);
        
        // Act
        var result = await _2UserManagerWithNoCustomSchemes.SetUserActiveInputDefinitionAsync(1, "notreal");

        // Assert
        Assert.False(result.IsSuccessful);
        Assert.Equal(OutputSpecificityCode.DataNotFound, result.StatusCode.SpecificityCode);
    }

    [Fact]
    public async Task SetUserActiveInputDefinitionAsync_GetCustomSchemesFail_ReturnsError()
    {
        // Arrange
        _mockInputReaderProvider.Setup(m => m.GetInputReader(It.IsAny<IInputDeviceConfiguration>(),
            It.IsAny<InputDeviceIdentifier>()))
            .Returns(Mock.Of<IInputReader>());

        _mockInputSchemeRepository.Setup(m => m.GetCustomInputSchemesAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_outputFactory.Succeed(Enumerable.Empty<InputScheme>()));

        _mockInputSchemeRepository.Setup(m => m.GetActiveInputSchemesAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_outputFactory.Succeed(Enumerable.Empty<ActiveInputScheme>()));

        await _4UserManagerWithCustomSchemes.JoinUserAsync(1, JoinUserOptions.Default);

        _mockInputSchemeRepository.Setup(m => m.GetCustomInputSchemesAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_outputFactory.Fail<IEnumerable<InputScheme>>("Bad Day", OutputSpecificityCode.InvalidParameter));

        var definition = _4UserManagerWithCustomSchemes.Configuration.InputDefinitions.Last();

        // Act
        var result = await _4UserManagerWithCustomSchemes.SetUserActiveInputDefinitionAsync(1, definition.Name);

        // Assert
        Assert.False(result.IsSuccessful);
        Assert.Equal(OutputSpecificityCode.InvalidParameter, result.StatusCode.SpecificityCode);
    }

    [Fact]
    public async Task SetUserActiveInputDefinitionAsync_GetActiveSchemesFail_ReturnsError()
    {
        // Arrange
        _mockInputReaderProvider.Setup(m => m.GetInputReader(It.IsAny<IInputDeviceConfiguration>(),
            It.IsAny<InputDeviceIdentifier>()))
            .Returns(Mock.Of<IInputReader>());

        _mockInputSchemeRepository.Setup(m => m.GetCustomInputSchemesAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_outputFactory.Succeed(Enumerable.Empty<InputScheme>()));

        _mockInputSchemeRepository.Setup(m => m.GetActiveInputSchemesAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_outputFactory.Succeed(Enumerable.Empty<ActiveInputScheme>()));

        await _4UserManagerWithCustomSchemes.JoinUserAsync(1, JoinUserOptions.Default);

        _mockInputSchemeRepository.Setup(m => m.GetCustomInputSchemesAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_outputFactory.Fail<IEnumerable<InputScheme>>("Bad Day", OutputSpecificityCode.EndlessLoop));

        var definition = _4UserManagerWithCustomSchemes.Configuration.InputDefinitions.Last();

        // Act
        var result = await _4UserManagerWithCustomSchemes.SetUserActiveInputDefinitionAsync(1, definition.Name);

        // Assert
        Assert.False(result.IsSuccessful);
        Assert.Equal(OutputSpecificityCode.EndlessLoop, result.StatusCode.SpecificityCode);
    }

    [Fact]
    public async Task SetUserActiveInputDefinitionAsync_UserIdAlreadyUsingInputDefinition_ShortCircuits_ReturnsSuccessfully()
    {
        // Arrange
        _mockInputReaderProvider.Setup(m => m.GetInputReader(It.IsAny<IInputDeviceConfiguration>(),
            It.IsAny<InputDeviceIdentifier>()))
            .Returns(Mock.Of<IInputReader>());

        _mockInputSchemeRepository.Setup(m => m.GetCustomInputSchemesAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_outputFactory.Succeed(Enumerable.Empty<InputScheme>()));

        _mockInputSchemeRepository.Setup(m => m.GetActiveInputSchemesAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_outputFactory.Succeed(Enumerable.Empty<ActiveInputScheme>()));

        await _4UserManagerWithCustomSchemes.JoinUserAsync(1, JoinUserOptions.Default);

        _mockInputSchemeRepository.Reset();

        var definition = _4UserManagerWithCustomSchemes.Configuration.InputDefinitions.First();

        // Act
        var result = await _4UserManagerWithCustomSchemes.SetUserActiveInputDefinitionAsync(1, definition.Name);

        // Assert
        Assert.True(result.IsSuccessful);
        _mockInputSchemeRepository.Verify(m => m.GetCustomInputSchemesAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task SetUserActiveInputDefinitionAsync_NewDefinition_ReturnsSuccessfully()
    {
        // Arrange
        _mockInputReaderProvider.Setup(m => m.GetInputReader(It.IsAny<IInputDeviceConfiguration>(),
            It.IsAny<InputDeviceIdentifier>()))
              .Returns(Mock.Of<IInputReader>());

        _mockInputSchemeRepository.Setup(m => m.GetCustomInputSchemesAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_outputFactory.Succeed(Enumerable.Empty<InputScheme>()));

        _mockInputSchemeRepository.Setup(m => m.GetActiveInputSchemesAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_outputFactory.Succeed(Enumerable.Empty<ActiveInputScheme>()));

        var user = await _4UserManagerWithCustomSchemes.JoinUserAsync(1, JoinUserOptions.Default);

        _mockInputSchemeRepository.Reset();

        _mockInputSchemeRepository.Setup(m => m.GetCustomInputSchemesAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_outputFactory.Succeed(Enumerable.Empty<InputScheme>()));

        _mockInputSchemeRepository.Setup(m => m.GetActiveInputSchemesAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_outputFactory.Succeed(Enumerable.Empty<ActiveInputScheme>()));

        var definition = _4UserManagerWithCustomSchemes.Configuration.InputDefinitions.Last();

        // Act
        var result = await _4UserManagerWithCustomSchemes.SetUserActiveInputDefinitionAsync(1, definition.Name);

        // Assert
        Assert.True(result.IsSuccessful);
        Assert.True(user.Value.ActiveInputDefinition.DeepEquals(definition));
        _mockInputSchemeRepository.Verify(m => m.GetCustomInputSchemesAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region SetActiveInputSchemeAsync

    [Fact]
    public async Task SetActiveInputSchemeAsync_UserIdDoesNotExistOnInputManager_ReturnsNotFound()
    {
        // Arrange/Act
        var result = await _2UserManagerWithNoCustomSchemes.SetActiveInputSchemeAsync(1, "abc", "controller", "abc");

        // Assert
        Assert.False(result.IsSuccessful);
        Assert.Equal(OutputSpecificityCode.DataNotFound, result.StatusCode.SpecificityCode);
    }

    [Fact]
    public async Task SetActiveInputSchemeAsync_InputDefinitionNameDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        _mockInputSchemeRepository.Setup(m => m.GetActiveInputSchemesAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_outputFactory.Succeed(Enumerable.Empty<ActiveInputScheme>()));

        var controller = _2UserManagerWithNoCustomSchemes.Configuration.InputControllers.First();
        var inputDefinition = _2UserManagerWithNoCustomSchemes.Configuration.InputDefinitions.First();

        await _2UserManagerWithNoCustomSchemes.JoinUserAsync(1, JoinUserOptions.Default);

        // Act
        var result = await _2UserManagerWithNoCustomSchemes.SetActiveInputSchemeAsync(1, "notreal", controller.ControllerName, inputDefinition.InputSchemes.First().Name);

        // Assert
        Assert.False(result.IsSuccessful);
        Assert.Equal(OutputSpecificityCode.DataNotFound, result.StatusCode.SpecificityCode);
    }

    [Fact]
    public async Task SetActiveInputSchemeAsync_InputContorllerNameNotSupported_ReturnsNotFound()
    {
        // Arrange
        _mockInputSchemeRepository.Setup(m => m.GetActiveInputSchemesAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_outputFactory.Succeed(Enumerable.Empty<ActiveInputScheme>()));

        var device = _2UserManagerWithNoCustomSchemes.Configuration.SupportedInputDevices.First();
        var inputDefinition = _2UserManagerWithNoCustomSchemes.Configuration.InputDefinitions.First();

        await _2UserManagerWithNoCustomSchemes.JoinUserAsync(1, JoinUserOptions.Default);

        // Act
        var result = await _2UserManagerWithNoCustomSchemes.SetActiveInputSchemeAsync(1, inputDefinition.Name, "notreal", inputDefinition.InputSchemes.First().Name);

        // Assert
        Assert.False(result.IsSuccessful);
        Assert.Equal(OutputSpecificityCode.DataNotFound, result.StatusCode.SpecificityCode);
    }

    [Fact]
    public async Task SetActiveInputSchemeAsync_InputSchemeNameDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        _mockInputSchemeRepository.Setup(m => m.GetActiveInputSchemesAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_outputFactory.Succeed(Enumerable.Empty<ActiveInputScheme>()));

        var controller = _2UserManagerWithNoCustomSchemes.Configuration.InputControllers.First();
        var inputDefinition = _2UserManagerWithNoCustomSchemes.Configuration.InputDefinitions.First();

        await _2UserManagerWithNoCustomSchemes.JoinUserAsync(1, JoinUserOptions.Default);

        // Act
        var result = await _2UserManagerWithNoCustomSchemes.SetActiveInputSchemeAsync(1, inputDefinition.Name, controller.ControllerName, "notreal");

        // Assert
        Assert.False(result.IsSuccessful);
        Assert.Equal(OutputSpecificityCode.DataNotFound, result.StatusCode.SpecificityCode);
    }

    [Fact]
    public async Task SetActiveInputSchemeAsync_RepositorySaveFails_ReturnsError()
    {
        // Arrange
        _mockInputSchemeRepository.Setup(m => m.GetActiveInputSchemesAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_outputFactory.Succeed(Enumerable.Empty<ActiveInputScheme>()));

        var controller = _2UserManagerWithNoCustomSchemes.Configuration.InputControllers.First();
        var inputDefinition = _2UserManagerWithNoCustomSchemes.Configuration.InputDefinitions.First();

        _mockInputSchemeRepository.Setup(m => m.SaveActiveInputSchemeAsync(It.IsAny<ActiveInputScheme>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_outputFactory.Fail<ActiveInputScheme>("Bad Day", OutputSpecificityCode.BadGateway));

        await _2UserManagerWithNoCustomSchemes.JoinUserAsync(1, JoinUserOptions.Default);

        // Act
        var result = await _2UserManagerWithNoCustomSchemes.SetActiveInputSchemeAsync(1, inputDefinition.Name, controller.ControllerName, inputDefinition.InputSchemes.First().Name);

        // Assert
        Assert.False(result.IsSuccessful);
        Assert.Equal(OutputSpecificityCode.BadGateway, result.StatusCode.SpecificityCode);
    }

    [Fact]
    public async Task SetActiveInputSchemeAsync_UsersActiveInputDefinitionIsTheDefinitionUpdated_UpdatesInputSchemesForUser_SchemeRepositoryFails_ReturnsError()
    {
        // Arrange
        _mockInputReaderProvider.Setup(m => m.GetInputReader(It.IsAny<IInputDeviceConfiguration>(),
            It.IsAny<InputDeviceIdentifier>()))
            .Returns(Mock.Of<IInputReader>());

        _mockInputSchemeRepository.Setup(m => m.GetCustomInputSchemesAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_outputFactory.Succeed(Enumerable.Empty<InputScheme>()));

        _mockInputSchemeRepository.Setup(m => m.GetActiveInputSchemesAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_outputFactory.Succeed(Enumerable.Empty<ActiveInputScheme>()));

        var controller = _2UserManagerWithNoCustomSchemes.Configuration.InputControllers.First();
        var inputDefinition = _2UserManagerWithNoCustomSchemes.Configuration.InputDefinitions.First();

        var userJoined = await _2UserManagerWithNoCustomSchemes.JoinUserAsync(1, JoinUserOptions.Default);
        Assert.True(userJoined.IsSuccessful);

        _mockInputSchemeRepository.Setup(m => m.SaveActiveInputSchemeAsync(It.IsAny<ActiveInputScheme>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ActiveInputScheme scheme, CancellationToken _) => _outputFactory.Succeed(scheme));

        _mockInputSchemeRepository.Setup(m => m.GetActiveInputSchemesAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_outputFactory.Fail<IEnumerable<ActiveInputScheme>>("Bad Day", OutputSpecificityCode.SpecificityNotRecognized));

        // Act
        var result = await _2UserManagerWithNoCustomSchemes.SetActiveInputSchemeAsync(1, inputDefinition.Name, controller.ControllerName, inputDefinition.InputSchemes.First().Name);

        // Assert
        Assert.False(result.IsSuccessful);
        Assert.Equal(OutputSpecificityCode.SpecificityNotRecognized, result.StatusCode.SpecificityCode);
    }

    [Fact]
    public async Task SetActiveInputSchemeAsync_UsersActiveInputDefinitionNotTheDefinitionUpdated_ShortCircuits_ReturnsSuccessfully()
    {
        // Arrange
        _mockInputReaderProvider.Setup(m => m.GetInputReader(It.IsAny<IInputDeviceConfiguration>(),
            It.IsAny<InputDeviceIdentifier>()))
            .Returns(Mock.Of<IInputReader>());

        _mockInputSchemeRepository.Setup(m => m.GetCustomInputSchemesAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_outputFactory.Succeed(Enumerable.Empty<InputScheme>()));

        _mockInputSchemeRepository.Setup(m => m.GetActiveInputSchemesAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_outputFactory.Succeed(Enumerable.Empty<ActiveInputScheme>()));

        var controller = _2UserManagerWithNoCustomSchemes.Configuration.InputControllers.First();
        var inputDefinition = _2UserManagerWithNoCustomSchemes.Configuration.InputDefinitions.Last();

        await _2UserManagerWithNoCustomSchemes.JoinUserAsync(1, JoinUserOptions.Default);

        _mockInputSchemeRepository.Reset();

        _mockInputSchemeRepository.Setup(m => m.SaveActiveInputSchemeAsync(It.IsAny<ActiveInputScheme>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ActiveInputScheme scheme, CancellationToken _) => _outputFactory.Succeed(scheme));

        // Act
        var result = await _2UserManagerWithNoCustomSchemes.SetActiveInputSchemeAsync(1, inputDefinition.Name, controller.ControllerName, inputDefinition.InputSchemes.First().Name);

        // Assert
        Assert.True(result.IsSuccessful);
        _mockInputSchemeRepository.Verify(m => m.GetActiveInputSchemesAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task SetActiveInputSchemeAsync_UsersActiveInputDefinitionIsTheDefinitionUpdated_UpdatesInputSchemesForUser_ReturnsSuccessfully()
    {
        // Arrange
        _mockInputSchemeRepository.Setup(m => m.GetActiveInputSchemesAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_outputFactory.Succeed(Enumerable.Empty<ActiveInputScheme>()));

        _mockInputSchemeRepository.Setup(m => m.GetActiveInputSchemesAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_outputFactory.Succeed(Enumerable.Empty<ActiveInputScheme>()));

        var controller = _2UserManagerWithNoCustomSchemes.Configuration.InputControllers.First();
        var inputDefinition = _2UserManagerWithNoCustomSchemes.Configuration.InputDefinitions.Last();

        ActiveInputScheme activeScheme = new ActiveInputScheme(1, "", "", "");
        _mockInputSchemeRepository.Setup(m => m.SaveActiveInputSchemeAsync(It.IsAny<ActiveInputScheme>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ActiveInputScheme scheme, CancellationToken _) =>
            {
                activeScheme = scheme;
                return _outputFactory.Succeed(scheme);
            });

        _mockInputSchemeRepository.Setup(m => m.GetActiveInputSchemesAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_outputFactory.Succeed((IEnumerable<ActiveInputScheme>)[activeScheme]));

        await _2UserManagerWithNoCustomSchemes.JoinUserAsync(1, JoinUserOptions.Default);

        // Act
        var result = await _2UserManagerWithNoCustomSchemes.SetActiveInputSchemeAsync(1, inputDefinition.Name, controller.ControllerName, inputDefinition.InputSchemes.First().Name);

        // Assert
        Assert.True(result.IsSuccessful);
    }

    #endregion

    #region ResetUserActiveInputSchemeAsync

    [Fact]
    public async Task ResetUserActiveInputSchemeAsync_UserIdDoesNotExistOnInputManager_ReturnsNotFound()
    {
        // Arrange/Act
        var result = await _2UserManagerWithNoCustomSchemes.ResetUserActiveInputSchemeAsync(1, "abc", "abc");

        // Assert
        Assert.False(result.IsSuccessful);
        Assert.Equal(OutputSpecificityCode.DataNotFound, result.StatusCode.SpecificityCode);
    }

    [Fact]
    public async Task ResetUserActiveInputSchemeAsync_RepositoryError_ReturnsError()
    {
        // Arrange
        _mockInputReaderProvider.Setup(m => m.GetInputReader(It.IsAny<IInputDeviceConfiguration>(),
            It.IsAny<InputDeviceIdentifier>()))
            .Returns(Mock.Of<IInputReader>());

        _mockInputSchemeRepository.Setup(m => m.GetCustomInputSchemesAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_outputFactory.Succeed(Enumerable.Empty<InputScheme>()));

        _mockInputSchemeRepository.Setup(m => m.GetActiveInputSchemesAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_outputFactory.Succeed(Enumerable.Empty<ActiveInputScheme>()));

        _mockInputSchemeRepository.Setup(m => m.DeleteActiveInputSchemeAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(_outputFactory.Fail("A bad day", OutputSpecificityCode.DataTooLarge));

        var controller = _2UserManagerWithNoCustomSchemes.Configuration.InputControllers.First();
        var inputDefinition = _2UserManagerWithNoCustomSchemes.Configuration.InputDefinitions.First();

        await _2UserManagerWithNoCustomSchemes.JoinUserAsync(1, JoinUserOptions.Default);

        // Act
        var result = await _2UserManagerWithNoCustomSchemes.ResetUserActiveInputSchemeAsync(1, inputDefinition.Name, controller.ControllerName);

        // Assert
        Assert.False(result.IsSuccessful);
        Assert.Equal(OutputSpecificityCode.DataTooLarge, result.StatusCode.SpecificityCode);
    }

    [Fact]
    public async Task ResetUserActiveInputSchemeAsync_GetActiveSchemesErrors_ReturnsError()
    {
        // Arrange
        _mockInputReaderProvider.Setup(m => m.GetInputReader(It.IsAny<IInputDeviceConfiguration>(),
            It.IsAny<InputDeviceIdentifier>()))
            .Returns(Mock.Of<IInputReader>());

        _mockInputSchemeRepository.Setup(m => m.GetCustomInputSchemesAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_outputFactory.Succeed(Enumerable.Empty<InputScheme>()));

        _mockInputSchemeRepository.Setup(m => m.GetActiveInputSchemesAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_outputFactory.Succeed(Enumerable.Empty<ActiveInputScheme>()));

        _mockInputSchemeRepository.Setup(m => m.DeleteActiveInputSchemeAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(_outputFactory.Succeed());

        var controller = _2UserManagerWithNoCustomSchemes.Configuration.InputControllers.First();
        var inputDefinition = _2UserManagerWithNoCustomSchemes.Configuration.InputDefinitions.First();

        await _2UserManagerWithNoCustomSchemes.JoinUserAsync(1, JoinUserOptions.Default);
        var user = (ApplicationInputUser)_2UserManagerWithNoCustomSchemes.GetApplicationInputUser(1);
        user._inputControllers[controller.ControllerName] = new RuntimeInputController(null!, new InputScheme("", "", false, []), []);
        
        _mockInputSchemeRepository.Setup(m => m.GetActiveInputSchemesAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_outputFactory.Fail<IEnumerable<ActiveInputScheme>>("a bad day", OutputSpecificityCode.DataTooLarge));

        // Act
        var result = await _2UserManagerWithNoCustomSchemes.ResetUserActiveInputSchemeAsync(1, inputDefinition.Name, controller.ControllerName);

        // Assert
        Assert.False(result.IsSuccessful);
        Assert.Equal(OutputSpecificityCode.DataTooLarge, result.StatusCode.SpecificityCode);
    }

    [Fact]
    public async Task ResetUserActiveInputSchemeAsync_UpdatedDefinitionIsNotUserDefinition_ReturnsSuccessfully()
    {
        // Arrange
        _mockInputReaderProvider.Setup(m => m.GetInputReader(It.IsAny<IInputDeviceConfiguration>(),
            It.IsAny<InputDeviceIdentifier>()))
            .Returns(Mock.Of<IInputReader>());

        _mockInputSchemeRepository.Setup(m => m.GetCustomInputSchemesAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_outputFactory.Succeed(Enumerable.Empty<InputScheme>()));

        _mockInputSchemeRepository.Setup(m => m.GetActiveInputSchemesAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_outputFactory.Succeed(Enumerable.Empty<ActiveInputScheme>()));

        var controller = _2UserManagerWithNoCustomSchemes.Configuration.InputControllers.First();
        var inputDefinition = _2UserManagerWithNoCustomSchemes.Configuration.InputDefinitions.Last();

        await _2UserManagerWithNoCustomSchemes.JoinUserAsync(1, JoinUserOptions.Default);

        _mockInputSchemeRepository.Reset();

        _mockInputSchemeRepository.Setup(m => m.DeleteActiveInputSchemeAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(_outputFactory.Succeed());

        // Act
        var result = await _2UserManagerWithNoCustomSchemes.ResetUserActiveInputSchemeAsync(1, inputDefinition.Name, controller.ControllerName);

        // Assert
        Assert.True(result.IsSuccessful);
        _mockInputSchemeRepository.Verify(m => m.GetActiveInputSchemesAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ResetUserActiveInputSchemeAsync_UpdatedDefinitionIsUserDefinition_UpdatesUserSchemes_ReturnsSuccessfully()
    {
        // Arrange
        _mockInputSchemeRepository.Setup(m => m.DeleteActiveInputSchemeAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(_outputFactory.Succeed());

        _mockInputSchemeRepository.Setup(m => m.GetActiveInputSchemesAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_outputFactory.Succeed(Enumerable.Empty<ActiveInputScheme>()));

        var controller = _2UserManagerWithNoCustomSchemes.Configuration.InputControllers.First();
        var inputDefinition = _2UserManagerWithNoCustomSchemes.Configuration.InputDefinitions.First();

        await _2UserManagerWithNoCustomSchemes.JoinUserAsync(1, JoinUserOptions.Default);

        // Act
        var result = await _2UserManagerWithNoCustomSchemes.ResetUserActiveInputSchemeAsync(1, inputDefinition.Name, controller.ControllerName);

        // Assert
        Assert.True(result.IsSuccessful);
    }

    #endregion

    #region SaveCustomInputSchemeAsync

    [Fact]
    public async Task SaveCustomInputSchemeAsync_NullInputScheme_ThrowArgumentNullException()
    {
        // Arrange/Act/Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _2UserManagerWithNoCustomSchemes.SaveCustomInputSchemeAsync(null!));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public async Task SaveCustomInputSchemeAsync_EmptyInputSchemeDefinitionName_ReturnsError(string? definitionName)
    {
        // Arrange
        var scheme = new InputScheme(definitionName!, "abc", false, []);

        // Act
        var saveOutput = await _2UserManagerWithNoCustomSchemes.SaveCustomInputSchemeAsync(scheme);

        // Assert
        Assert.False(saveOutput.IsSuccessful);
    }

    [Fact]
    public async Task SaveCustomInputSchemeAsync_InputSystemDoesNotAllowCustomSchemes_ReturnsError()
    {
        // Arrange
        var scheme = new InputScheme(_2UserManagerWithNoCustomSchemes.Configuration.InputDefinitions.First().Name, "abc", false, []);

        // Act
        var saveOutput = await _2UserManagerWithNoCustomSchemes.SaveCustomInputSchemeAsync(scheme);

        // Assert
        Assert.False(saveOutput.IsSuccessful);
    }

    [Fact]
    public async Task SaveCustomInputSchemeAsync_InputSchemeHasValidationErrors_ReturnsError()
    {
        // Arrange
        var definition = _4UserManagerWithCustomSchemes.Configuration.InputDefinitions.First();
        var device = _4UserManagerWithCustomSchemes.Configuration.SupportedInputDevices.First();
        var scheme = new InputScheme(definition.Name, "abc", false, []);

        var validationContext = new InputValidationContext("test");
        validationContext.AddErrors(1, "Oh ya");

        _mockValidationService.Setup(m => m.ValidateCustomInputScheme(It.IsAny<InputSystemConfiguration>(), It.IsAny<InputScheme>()))
            .Returns(validationContext);

        // Act
        var saveOutput = await _4UserManagerWithCustomSchemes.SaveCustomInputSchemeAsync(scheme);

        // Assert
        Assert.False(saveOutput.IsSuccessful);
    }

    [Fact]
    public async Task SaveCustomInputSchemeAsync_InputSchemeHasNoValidationErrors_CallsRepository_ReturnsRepositoryResult()
    {
        // Arrange
        var definition = _4UserManagerWithCustomSchemes.Configuration.InputDefinitions.First();
        var scheme = new InputScheme(definition.Name, "abc", false, []);

        _mockInputSchemeRepository.Setup(m => m.SaveCustomInputSchemeAsync(It.IsAny<string>(), It.IsAny<InputScheme>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string _, InputScheme _, CancellationToken _) =>
            {
                return _outputFactory.Fail<InputScheme>("A bad day", OutputSpecificityCode.BadGateway);
            });

        _mockValidationService.Setup(m => m.ValidateCustomInputScheme(It.IsAny<InputSystemConfiguration>(), It.IsAny<InputScheme>()))
            .Returns(InputValidationContext.Success);

        // Act
        var saveOutput = await _4UserManagerWithCustomSchemes.SaveCustomInputSchemeAsync(scheme);

        // Assert
        Assert.False(saveOutput.IsSuccessful);
        Assert.Equal(OutputSpecificityCode.BadGateway, saveOutput.StatusCode.SpecificityCode);
    }

    #endregion

    #region DeleteCustomInputSchemeAsync

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public async Task DeleteCustomInputSchemeAsync_EmptyInputDefinitionName_ThrowsArgumentNullException(string? definitionName)
    {
        // Arrange/Act/Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _2UserManagerWithNoCustomSchemes.DeleteCustomInputSchemeAsync(definitionName!, "abc", "def"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public async Task DeleteCustomInputSchemeAsync_EmptyInputControllerName_ThrowsArgumentNullException(string? controllerName)
    {
        // Arrange/Act/Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _2UserManagerWithNoCustomSchemes.DeleteCustomInputSchemeAsync("abc", controllerName!, "def"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public async Task DeleteCustomInputSchemeAsync_EmptyInputSchemeName_ThrowsArgumentNullException(string? schemeName)
    {
        // Arrange/Act/Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _2UserManagerWithNoCustomSchemes.DeleteCustomInputSchemeAsync("abc", "def", schemeName!));
    }

    [Fact]
    public async Task DeleteCustomInputSchemeAsync_InputSchemeIsBuiltIn_ReturnsError()
    {
        // Arrange
        var definitionName = _4UserManagerWithCustomSchemes.Configuration.InputDefinitions.First().Name;
        var scheme = _4UserManagerWithCustomSchemes.Configuration.InputDefinitions.First().InputSchemes.First();

        // Act
        var deleteOutput = await _4UserManagerWithCustomSchemes.DeleteCustomInputSchemeAsync(definitionName, scheme.ControllerId, scheme.Name);

        // Assert
        Assert.False(deleteOutput.IsSuccessful);
    }

    [Fact]
    public async Task DeleteCustomInputSchemeAsync_InputDefinitionDoesNotExist_ReturnsSuccessfully()
    {
        // Arrange/Act
        var deleteOutput = await _4UserManagerWithCustomSchemes.DeleteCustomInputSchemeAsync("abc", "abc", "abc");

        // Assert
        Assert.True(deleteOutput.IsSuccessful);
    }

    [Fact]
    public async Task DeleteCustomInputSchemeAsync_InputControllerDoesNotExist_ReturnsSuccessfully()
    {
        // Arrange
        var definitionName = _4UserManagerWithCustomSchemes.Configuration.InputDefinitions.First().Name;

        // Act
        var deleteOutput = await _2UserManagerWithNoCustomSchemes.DeleteCustomInputSchemeAsync(definitionName, "abc", "abc");

        // Assert
        Assert.True(deleteOutput.IsSuccessful);
    }

    [Fact]
    public async Task DeleteCustomInputSchemeAsync_InputSchemeDoesNotExist_DefinitionDoesNotAllowCustomSchemes_ReturnsSuccessfully()
    {
        // Arrange
        var definitionName = _2UserManagerWithNoCustomSchemes.Configuration.InputDefinitions.First().Name;
        var controller = _2UserManagerWithNoCustomSchemes.Configuration.InputControllers.First();

        // Act
        var deleteOutput = await _2UserManagerWithNoCustomSchemes.DeleteCustomInputSchemeAsync(definitionName, controller.ControllerName, "notreal");

        // Assert
        Assert.True(deleteOutput.IsSuccessful);
    }

    [Fact]
    public async Task DeleteCustomInputSchemeAsync_InputSchemeDoesNotExistOnDefinition_DefinitionAllowsCustomSchemes_CallsRepository_ReturnsOutput()
    {
        // Arrange
        var definitionName = _4UserManagerWithCustomSchemes.Configuration.InputDefinitions.First().Name;
        var controller = _2UserManagerWithNoCustomSchemes.Configuration.InputControllers.First();
        var schemeName = _4UserManagerWithCustomSchemes.Configuration.InputDefinitions.First().InputSchemes.First().Name;

        _mockInputSchemeRepository.Setup(m => m.DeleteCustomInputSchemeAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(_outputFactory.Succeed());

        // Act
        var deleteOutput = await _4UserManagerWithCustomSchemes.DeleteCustomInputSchemeAsync(definitionName, controller.ControllerName, "notreal");

        // Assert
        Assert.True(deleteOutput.IsSuccessful);
    }

    [Fact]
    public async Task DeleteCustomInputSchemeAsync_InputSchemeDoesNotExistOnDefinition_DefinitionAllowsCustomSchemes_CallsRepository_MasksNotFoundAsSuccess()
    {
        // Arrange
        var definitionName = _4UserManagerWithCustomSchemes.Configuration.InputDefinitions.First().Name;
        var controller = _4UserManagerWithCustomSchemes.Configuration.InputControllers.First();
        var schemeName = _4UserManagerWithCustomSchemes.Configuration.InputDefinitions.First().InputSchemes.First().Name;

        _mockInputSchemeRepository.Setup(m => m.DeleteCustomInputSchemeAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(_outputFactory.NotFound());

        // Act
        var deleteOutput = await _4UserManagerWithCustomSchemes.DeleteCustomInputSchemeAsync(definitionName, controller.ControllerName, schemeName);

        // Assert
        Assert.True(deleteOutput.IsSuccessful);
    }

    #endregion

    #region JoinUserAsync

    [Fact]
    public async Task JoinUserAsync_NullJoinOptions_ThrowsArgumentNullException()
    {
        // Arrange/Act/Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _2UserManagerWithNoCustomSchemes.JoinUserAsync(1, null!));
    }

    [Fact]
    public async Task JoinUserAsync_InputControllerIdentifierDoesNotExist_ThrowsInvalidOperationException()
    {
        // Arrange
        var definition = _2UserManagerWithNoCustomSchemes.Configuration.InputDefinitions.First();
        var device = _2UserManagerWithNoCustomSchemes.Configuration.SupportedInputDevices.First();

        _mockInputSchemeRepository.Setup(m => m.GetActiveInputSchemesAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_outputFactory.Succeed((IEnumerable<ActiveInputScheme>)[]));

        // Act/Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _2UserManagerWithNoCustomSchemes.JoinUserAsync(1, new JoinUserOptions()
        {
            DeviceIdentifiers = [new InputDeviceIdentifier(1, new InputDeviceName("notreal"))]
        }));
    }

    [Fact]
    public async Task JoinUserAsync_PreferredInputDefinitionDoesNotExist_ReturnsNotFound()
    {
        // Arrange/Act
        var result = await _2UserManagerWithNoCustomSchemes.JoinUserAsync(1, new JoinUserOptions()
        {
            ActiveInputDefinitionName = "notreal"
        });

        // Assert
        Assert.False(result.IsSuccessful);
        Assert.Equal(OutputSpecificityCode.DataNotFound, result.StatusCode.SpecificityCode);
    }

    [Fact]
    public async Task JoinUserAsync_GetActiveInputSchemesFails_ReturnsError()
    {
        // Arrange
        var definition = _2UserManagerWithNoCustomSchemes.Configuration.InputDefinitions.First();
        _mockInputSchemeRepository.Setup(m => m.GetActiveInputSchemesAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_outputFactory.Fail<IEnumerable<ActiveInputScheme>>("Bad day", OutputSpecificityCode.NotAuthenticated));
        
        // Act
        var result = await _2UserManagerWithNoCustomSchemes.JoinUserAsync(1, new JoinUserOptions()
        {
            ActiveInputDefinitionName = definition.Name
        });

        // Assert
        Assert.False(result.IsSuccessful);
        Assert.Equal(OutputSpecificityCode.NotAuthenticated, result.StatusCode.SpecificityCode);
    }

    [Fact]
    public async Task JoinUserAsync_Valid_ReturnsSuccessfully()
    {
        // Arrange
        var definition = _2UserManagerWithNoCustomSchemes.Configuration.InputDefinitions.First();
        _mockInputSchemeRepository.Setup(m => m.GetActiveInputSchemesAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_outputFactory.Succeed((IEnumerable<ActiveInputScheme>)[]));

        _mockInputReaderProvider.Setup(m => m.GetInputReader(It.IsAny<IInputDeviceConfiguration>(), It.IsAny<InputDeviceIdentifier>()))
            .Returns(Mock.Of<IInputReader>());

        // Act
        var result = await _2UserManagerWithNoCustomSchemes.JoinUserAsync(1, new JoinUserOptions()
        {
            ActiveInputDefinitionName = definition.Name,
            DeviceIdentifiers =[ new InputDeviceIdentifier(1, _2UserManagerWithNoCustomSchemes.Configuration.SupportedInputDevices.First().DeviceName) ]
        });

        // Assert
        Assert.True(result.IsSuccessful);
        Assert.True(result.Value.ActiveInputDefinition.DeepEquals(definition));
    }

    [Fact]
    public async Task JoinUserAsync_UserAlreadyJoined_ShortCircuits_ReturnsSuccessfully()
    {
        // Arrange
        var definition = _2UserManagerWithNoCustomSchemes.Configuration.InputDefinitions.First();
        var device = _2UserManagerWithNoCustomSchemes.Configuration.SupportedInputDevices.First();

        _mockInputSchemeRepository.Setup(m => m.GetActiveInputSchemesAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_outputFactory.Succeed((IEnumerable<ActiveInputScheme>)[]));

        var joinUserOutput = await _2UserManagerWithNoCustomSchemes.JoinUserAsync(1, JoinUserOptions.Default);

        // Act
        var joinUserOutput2 = await _2UserManagerWithNoCustomSchemes.JoinUserAsync(1, new JoinUserOptions()
        {
            ActiveInputDefinitionName = "Bad",
            DeviceIdentifiers = [new InputDeviceIdentifier(1, new InputDeviceName("Bad"))]
        });

        // Assert
        Assert.True(joinUserOutput2.IsSuccessful);
        Assert.Equal(joinUserOutput.Value, joinUserOutput2.Value);
    }

    #endregion

    #region RemoveUser

    [Fact]
    public void RemoveUser_UserDoesNotExist_ReturnsSuccessfully()
    {
        // Arrange/Act/Assert
        _2UserManagerWithNoCustomSchemes.RemoveUser(1);
    }

    [Fact]
    public async Task RemoveUser_UserExists_UserRemoved_ReturnsSuccessfully()
    {
        // Arrange
        _mockInputReaderProvider.Setup(m => m.GetInputReader(It.IsAny<IInputDeviceConfiguration>(),
            It.IsAny<InputDeviceIdentifier>()))
            .Returns(Mock.Of<IInputReader>());

        _mockInputSchemeRepository.Setup(m => m.GetCustomInputSchemesAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_outputFactory.Succeed(Enumerable.Empty<InputScheme>()));

        _mockInputSchemeRepository.Setup(m => m.GetActiveInputSchemesAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_outputFactory.Succeed(Enumerable.Empty<ActiveInputScheme>()));

        await _2UserManagerWithNoCustomSchemes.JoinUserAsync(1, JoinUserOptions.Default);
        Assert.Single(_2UserManagerWithNoCustomSchemes.GetApplicationInputUsers());

        // Act
        _2UserManagerWithNoCustomSchemes.RemoveUser(1);

        // Assert
        Assert.Empty(_2UserManagerWithNoCustomSchemes.GetApplicationInputUsers());
    }

    #endregion

    #region PairController

    [Fact]
    public void PairController_UserIdDoesNotExist_ThrowsInvalidOperationException()
    {
        // Arrange/Act/Assert
        Assert.Throws<InvalidOperationException>(() => _4UserManagerWithCustomSchemes.PairDevice(117, new InputDeviceIdentifier()));
    }

    [Fact]
    public async Task PairController_ControllerAddedToDifferentUser_ThrowsInvalidOperationException()
    {
        // Arrange
        _mockInputReaderProvider.Setup(m => m.GetInputReader(It.IsAny<IInputDeviceConfiguration>(),
            It.IsAny<InputDeviceIdentifier>()))
            .Returns(Mock.Of<IInputReader>());

        _mockInputSchemeRepository.Setup(m => m.GetCustomInputSchemesAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_outputFactory.Succeed(Enumerable.Empty<InputScheme>()));

        _mockInputSchemeRepository.Setup(m => m.GetActiveInputSchemesAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_outputFactory.Succeed(Enumerable.Empty<ActiveInputScheme>()));

        _mockInputSchemeRepository.Setup(m => m.GetCustomInputSchemesAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_outputFactory.Succeed(Enumerable.Empty<InputScheme>()));

        _mockInputSchemeRepository.Setup(m => m.GetActiveInputSchemesAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_outputFactory.Succeed(Enumerable.Empty<ActiveInputScheme>()));

        var deviceName = _4UserManagerWithCustomSchemes.Configuration.SupportedInputDevices.First().DeviceName;
        var deviceIdentifier = new InputDeviceIdentifier(1, deviceName);

        await _4UserManagerWithCustomSchemes.JoinUserAsync(1, JoinUserOptions.Default);
        await _4UserManagerWithCustomSchemes.JoinUserAsync(2, JoinUserOptions.Default);

        _4UserManagerWithCustomSchemes.PairDevice(1, deviceIdentifier);

        // Act/Assert
        Assert.Throws<InvalidOperationException>(() => _4UserManagerWithCustomSchemes.PairDevice(2, deviceIdentifier));
    }

    [Fact]
    public async Task PairController_AddNewController_ReturnsSuccessfully()
    {
        // Arrange
        _mockInputReaderProvider.Setup(m => m.GetInputReader(It.IsAny<IInputDeviceConfiguration>(),
            It.IsAny<InputDeviceIdentifier>()))
            .Returns(Mock.Of<IInputReader>());

        _mockInputSchemeRepository.Setup(m => m.GetCustomInputSchemesAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_outputFactory.Succeed(Enumerable.Empty<InputScheme>()));

        _mockInputSchemeRepository.Setup(m => m.GetActiveInputSchemesAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_outputFactory.Succeed(Enumerable.Empty<ActiveInputScheme>()));

        var deviceName = _4UserManagerWithCustomSchemes.Configuration.SupportedInputDevices.First().DeviceName;
        var deviceIdentifier = new InputDeviceIdentifier(1, deviceName);

        var user = await _4UserManagerWithCustomSchemes.JoinUserAsync(1, JoinUserOptions.Default);

        // Act
        _4UserManagerWithCustomSchemes.PairDevice(1, deviceIdentifier);

        // Assert
        Assert.Single(user.Value.DeviceIdentifiers, identifier => identifier == deviceIdentifier);
    }

    [Fact]
    public async Task PairController_AddPreviousControllerToSameUser_ReturnsSuccessfully()
    {
        // Arrange
        _mockInputReaderProvider.Setup(m => m.GetInputReader(It.IsAny<IInputDeviceConfiguration>(),
            It.IsAny<InputDeviceIdentifier>()))
            .Returns(Mock.Of<IInputReader>());

        _mockInputSchemeRepository.Setup(m => m.GetCustomInputSchemesAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_outputFactory.Succeed(Enumerable.Empty<InputScheme>()));

        _mockInputSchemeRepository.Setup(m => m.GetActiveInputSchemesAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_outputFactory.Succeed(Enumerable.Empty<ActiveInputScheme>()));

        var deviceName = _4UserManagerWithCustomSchemes.Configuration.SupportedInputDevices.First().DeviceName;
        var deviceIdentifier = new InputDeviceIdentifier(1, deviceName);

        var user = await _4UserManagerWithCustomSchemes.JoinUserAsync(1, JoinUserOptions.Default);
        _4UserManagerWithCustomSchemes.PairDevice(1, deviceIdentifier);

        // Act
        _4UserManagerWithCustomSchemes.PairDevice(1, deviceIdentifier);

        // Assert
        Assert.Single(user.Value.DeviceIdentifiers, identifier => identifier == deviceIdentifier);
    }

    #endregion

    #region GetApplicationInputUser

    [Fact]
    public void GetApplicationInputUser_UserIdDoesNotExist_ThrowsInvalidOperationException()
    {
        // Arrnage/Act/Assert
        Assert.Throws<InvalidOperationException>(() => _4UserManagerWithCustomSchemes.GetApplicationInputUser(117));
    }

    [Fact]
    public async Task GetApplicationInputUser_UserIdExists_ReturnsUser()
     {
        // Arrnage
        _mockInputSchemeRepository.Setup(m => m.GetActiveInputSchemesAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_outputFactory.Succeed(Enumerable.Empty<ActiveInputScheme>()));

        var user = await _2UserManagerWithNoCustomSchemes.JoinUserAsync(1, JoinUserOptions.Default);

        // Act
        var getUser = _2UserManagerWithNoCustomSchemes.GetApplicationInputUser(user.Value.Id);

        // Assert
        Assert.Equal(user.Value, getUser);
    }

    #endregion

    #region GetApplicationInputUsers

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public async Task GetApplicationInputUsers_ReturnsListOfAddedUsers_ReturnsSuccessfully(int totalUsers)
    {
        // Arrange        
        _mockInputSchemeRepository.Setup(m => m.GetActiveInputSchemesAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_outputFactory.Succeed(Enumerable.Empty<ActiveInputScheme>()));
        
        for (var i = 0; i < totalUsers; i++)
        {
            await _2UserManagerWithNoCustomSchemes.JoinUserAsync(i, JoinUserOptions.Default);
        }

        // Act
        var users = _2UserManagerWithNoCustomSchemes.GetApplicationInputUsers();

        // Assert
        Assert.Equal(totalUsers, users.Count());
        for (var i = 0; i < totalUsers; i++)
        {
            Assert.Contains(users, user => user.Id == i);
        }
    }

    #endregion

    #region ReadInputsAsync



    #endregion
}
