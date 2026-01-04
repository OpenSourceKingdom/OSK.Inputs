using Moq;
using OSK.Functions.Outputs.Abstractions;
using OSK.Functions.Outputs.Logging.Abstractions;
using OSK.Functions.Outputs.Mocks;
using OSK.Inputs.Abstractions;
using OSK.Inputs.Abstractions.Configuration;
using OSK.Inputs.Abstractions.Notifications;
using OSK.Inputs.Internal;
using OSK.Inputs.Internal.Services;
using OSK.Inputs.Models;
using OSK.Inputs.Ports;
using Xunit;

namespace OSK.Inputs.UnitTests.Internal.Services;

public class InputSystemTests
{
    #region Variables

    private readonly Mock<IInputSchemeRepository> _mockSchemeRepository;
    private readonly Mock<IInputProcessor> _mockInputProcessor;
    private readonly Mock<IInputNotificationPublisher> _mockInputNotificationPublisher;
    private readonly Mock<IInputUserManager> _mockUserManager;
    private readonly Mock<IInputConfigurationProvider> _mockConfigurationProvider;
    private readonly Mock<IInputSystemConfigurationValidator> _mockValidator;
    private readonly IOutputFactory<InputSystem> _outputFactory;

    private readonly InputSystem _inputSystem;

    #endregion

    #region Constructors

    public InputSystemTests()
    {
        _mockSchemeRepository = new();
        _mockInputProcessor = new();
        _mockInputNotificationPublisher = new();
        _mockUserManager = new();
        _mockConfigurationProvider = new();
        _mockValidator = new();
        _outputFactory = new MockOutputFactory<InputSystem>();

        _inputSystem = new InputSystem(_mockConfigurationProvider.Object, _mockUserManager.Object,
            _mockInputProcessor.Object, _mockInputNotificationPublisher.Object, _mockSchemeRepository.Object, _mockValidator.Object, _outputFactory);
    }

    #endregion

    #region AllowCustomSchemes

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void AllowCustomSchemes_ReturnsSchemeRepositoryValue(bool allowCustomSchemes)
    {
        // Arrange
        _mockSchemeRepository.SetupGet(m => m.AllowCustomSchemes)
            .Returns(allowCustomSchemes);

        // Act/Assert
        Assert.Equal(allowCustomSchemes, _inputSystem.AllowCustomSchemes);
    }

    #endregion

    #region ToggleInputProcessing

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ToggleInputProcessing_PausedStateMatchesCurrentState_DoesNotCallProcessorAndNotifier(bool paused)
    {
        // Arrange
        _inputSystem.PausedInput = paused;

        // Act
        _inputSystem.ToggleInputProcessing(paused);

        // Assert
        Assert.Equal(paused, _inputSystem.PausedInput);
        _mockInputProcessor.Verify(m => m.ToggleInputProcessing(It.IsAny<bool>()), Times.Never);
        _mockInputNotificationPublisher.Verify(m => m.Notify(It.IsAny<IInputNotification>()), Times.Never);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ToggleInputProcessing_PausedStateDiffersFromCurrentState_CallsInputProcessorAndNotifier_SetsExpectedValue(bool paused)
    {
        // Arrange
        _inputSystem.PausedInput = !paused;

        // Act
        _inputSystem.ToggleInputProcessing(paused);

        // Assert
        Assert.Equal(paused, _inputSystem.PausedInput);
        _mockInputProcessor.Verify(m => m.ToggleInputProcessing(paused), Times.Once);
        _mockInputNotificationPublisher.Verify(m => m.Notify(It.Is<IInputNotification>(n => n is InputSystemNotification)), Times.Once);
    }

    #endregion

    #region InitializeAsync

    [Fact]
    public async Task InitializeAsync_CallsUserManagerToLoadData()
    {
        // Arrange/Act
        await _inputSystem.InitializeAsync();

        // Assert
        _mockUserManager.Verify(m => m.LoadUserConfigurationAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region DeleteCustomSchemeAsync

    [Fact]
    public async Task DeleteCustomSchemeAsync_SchemeRepositoryDoesNotSupportCustomSchemes_DoesNotCallRepository_ReturnsSuccessfully()
    {
        // Arrange/Act
        var output = await _inputSystem.DeleteCustomSchemeAsync("Abc", "Abc");

        // Assert
        Assert.True(output.IsSuccessful);
        _mockSchemeRepository.Verify(m => m.DeleteCustomSchemeAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task DeleteCustomSchemeAsync_InvalidDefinitionName_DoesNotCallRepository(string? name)
    {
        // Arrange
        _mockSchemeRepository.SetupGet(m => m.AllowCustomSchemes)
            .Returns(true);

        // Act
        var output = await _inputSystem.DeleteCustomSchemeAsync(name!, "Abc");

        // Assert
        Assert.True(output.IsSuccessful);
        _mockSchemeRepository.Verify(m => m.DeleteCustomSchemeAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task DeleteCustomSchemeAsync_InvalidSchemeName_DoesNotCallRepository(string? name)
    {
        // Arrange
        _mockSchemeRepository.SetupGet(m => m.AllowCustomSchemes)
            .Returns(true);

        // Act
        var output = await _inputSystem.DeleteCustomSchemeAsync("Abc", name!);

        // Assert
        Assert.True(output.IsSuccessful);
        _mockSchemeRepository.Verify(m => m.DeleteCustomSchemeAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task DeleteCustomSchemeAsync_DefinitionNotFound_DoesNotCallRepository()
    {
        // Arrange
        _mockSchemeRepository.SetupGet(m => m.AllowCustomSchemes)
            .Returns(true);

        _mockConfigurationProvider.SetupGet(m => m.Configuration)
            .Returns(new InputSystemConfiguration([], [], new(), new()));

        // Act
        var output = await _inputSystem.DeleteCustomSchemeAsync("Abc", "Abc");

        // Assert
        Assert.True(output.IsSuccessful);
        _mockSchemeRepository.Verify(m => m.DeleteCustomSchemeAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task DeleteCustomSchemeAsync_Valid_CallsRepository()
    {
        // Arrange
        _mockSchemeRepository.SetupGet(m => m.AllowCustomSchemes)
            .Returns(true);
        _mockSchemeRepository.Setup(m => m.DeleteCustomSchemeAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_outputFactory.Succeed());

        _mockConfigurationProvider.SetupGet(m => m.Configuration)
            .Returns(new InputSystemConfiguration([], [new InputDefinition("Abc", [], [], false)], new(), new()));

        // Act
        var output = await _inputSystem.DeleteCustomSchemeAsync("Abc", "Abc");

        // Assert
        Assert.True(output.IsSuccessful);
        _mockSchemeRepository.Verify(m => m.DeleteCustomSchemeAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region SaveCustomSchemeAsync

    [Fact]
    public async Task SaveCustomSchemeAsync_NullScheme_ThrowsArgumentNullException()
    {
        // Arrange/Act/Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _inputSystem.SaveCustomSchemeAsync(null!, SchemeSaveFlags.None));
    }

    [Fact]
    public async Task SaveCustomSchemeAsync_DoesNotAllowCustomSchemes_ReturnsBadRequest()
    {
        // Arrange/Act
        var output = await _inputSystem.SaveCustomSchemeAsync(new CustomInputScheme() { DefinitionName = "Abc", Name = "Abc", DeviceMaps = [] },
            SchemeSaveFlags.None);

        // Assert
        Assert.False(output.IsSuccessful);
    }

    [Fact]
    public async Task SaveCustomSchemeAsync_ValidatorIndicatesConflict_ReturnsDuplicate()
    {
        // Arrange
        _mockSchemeRepository.SetupGet(m => m.AllowCustomSchemes)
            .Returns(true);

        _mockValidator.Setup(m => m.ValidateCustomScheme(It.IsAny<InputSystemConfiguration>(), It.IsAny<CustomInputScheme>(),
            It.IsAny<bool>()))
            .Returns(InputConfigurationValidationResult.ForScheme(s => s.Name, InputConfigurationValidation.DuplicateData));

        // Act
        var output = await _inputSystem.SaveCustomSchemeAsync(new CustomInputScheme() { DefinitionName = "Abc", Name = "Abc", DeviceMaps = [] }, 
            SchemeSaveFlags.None);

        // Assert
        Assert.False(output.IsSuccessful);
        Assert.Equal(OutputSpecificityCode.DuplicateData, output.StatusCode.SpecificityCode);
    }

    [Theory]
    [InlineData(InputConfigurationValidation.MissingData)]
    [InlineData(InputConfigurationValidation.InvalidData)]
    public async Task SaveCustomSchemeAsync_ValidatorIndicatesBadOrMissingData_InvalidParameter(InputConfigurationValidation validation)
    {
        // Arrange
        _mockSchemeRepository.SetupGet(m => m.AllowCustomSchemes)
            .Returns(true);

        _mockValidator.Setup(m => m.ValidateCustomScheme(It.IsAny<InputSystemConfiguration>(), It.IsAny<CustomInputScheme>(),
            It.IsAny<bool>()))
            .Returns(InputConfigurationValidationResult.ForScheme(s => s.Name, validation));

        // Act
        var output = await _inputSystem.SaveCustomSchemeAsync(new CustomInputScheme() { DefinitionName = "Abc", Name = "Abc", DeviceMaps = [] },
            SchemeSaveFlags.None);

        // Assert
        Assert.False(output.IsSuccessful);
        Assert.Equal(OutputSpecificityCode.InvalidParameter, output.StatusCode.SpecificityCode);
    }

    [Fact]
    public async Task SaveCustomSchemeAsync_SchemeRepositoryReturnsError_ReturnsErrorAndDoesNotCallUserManager()
    {
        // Arrange
        _mockSchemeRepository.SetupGet(m => m.AllowCustomSchemes)
            .Returns(true);
        _mockSchemeRepository.Setup(m => m.SaveCustomInputScheme(It.IsAny<CustomInputScheme>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((CustomInputScheme scheme, CancellationToken _) => _outputFactory.Fail<CustomInputScheme>("Bad day"));

        _mockValidator.Setup(m => m.ValidateCustomScheme(It.IsAny<InputSystemConfiguration>(), It.IsAny<CustomInputScheme>(),
            It.IsAny<bool>()))
            .Returns(InputConfigurationValidationResult.Success);

        _mockConfigurationProvider.SetupGet(m => m.Configuration)
            .Returns(new InputSystemConfiguration([], [new InputDefinition("Abc", [], [], false)], new(), new()));

        // Act
        var output = await _inputSystem.SaveCustomSchemeAsync(new CustomInputScheme() { DefinitionName = "Abc", Name = "Abc", DeviceMaps = [] },
            SchemeSaveFlags.None);

        // Assert
        Assert.False(output.IsSuccessful);
        _mockUserManager.Verify(m => m.LoadUserConfigurationAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task SaveCustomSchemeAsync_Valid_ReturnsSuccessfully()
    {
        // Arrange
        _mockSchemeRepository.SetupGet(m => m.AllowCustomSchemes)
            .Returns(true);
        _mockSchemeRepository.Setup(m => m.SaveCustomInputScheme(It.IsAny<CustomInputScheme>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((CustomInputScheme scheme, CancellationToken _) => _outputFactory.Succeed(scheme));

        _mockUserManager.Setup(m => m.LoadUserConfigurationAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(_outputFactory.Succeed());

        _mockValidator.Setup(m => m.ValidateCustomScheme(It.IsAny<InputSystemConfiguration>(), It.IsAny<CustomInputScheme>(),
            It.IsAny<bool>()))
            .Returns(InputConfigurationValidationResult.Success);

        _mockConfigurationProvider.SetupGet(m => m.Configuration)
            .Returns(new InputSystemConfiguration([], [new InputDefinition("Abc", [], [], false)], new(), new()));

        // Act
        var output = await _inputSystem.SaveCustomSchemeAsync(new CustomInputScheme() { DefinitionName = "Abc", Name = "Abc", DeviceMaps = [] },
            SchemeSaveFlags.None);

        // Assert
        Assert.True(output.IsSuccessful);
        _mockUserManager.Verify(m => m.LoadUserConfigurationAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region Update

    [Fact]
    public void Update_ProcessingPaused_DoesCallsInputProcessor_ReturnsSuccessfully()
    {
        // Arrange
        _inputSystem.PausedInput = true;

        // Act
        _inputSystem.Update(TimeSpan.FromSeconds(1));

        // Assert
        _mockInputProcessor.Verify(m => m.Update(It.IsAny<TimeSpan>()), Times.Never);
    }

    [Fact]
    public void Update_ProcessingNotPaused_CallsInputProcessor_ReturnsSuccessfully()
    {
        // Arrange
        _inputSystem.PausedInput = false;

        // Act
        _inputSystem.Update(TimeSpan.FromSeconds(1));

        // Assert
        _mockInputProcessor.Verify(m => m.Update(It.IsAny<TimeSpan>()), Times.Once);
    }

    #endregion
}
