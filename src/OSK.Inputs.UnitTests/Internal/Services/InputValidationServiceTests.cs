using Moq;
using OSK.Inputs.Internal.Services;
using OSK.Inputs.Models.Configuration;
using OSK.Inputs.Models.Inputs;
using OSK.Inputs.UnitTests._Helpers;
using Xunit;

namespace OSK.Inputs.UnitTests.Internal.Services;

public class InputValidationServiceTests
{
    #region Variables

    private readonly InputValidationService _service;

    #endregion

    #region Constructors

    public InputValidationServiceTests()
    {
        _service = new();
    }

    #endregion

    #region ValidateInputSystemConfiguration

    [Fact]
    public void ValidateInputSystemConfiguration_NullInputSystemConfiguration_ThrowsArgumentNullException()
    {
        // Arrange/Act/Assert
        Assert.Throws<ArgumentNullException>(() => _service.ValidateInputSystemConfiguration(null!));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void ValidateInputSystemConfiguration_EmptyDeviceConfiguration_ReturnsInputDeviceMissingDataError(bool useNull)
    {
        // Arrange
        List<InputDefinition> definitions = [];
        List<IInputDeviceConfiguration>? deviceConfigurations = useNull ? null : [];

        var inputSystemConfiguration = new InputSystemConfiguration(definitions, [], deviceConfigurations!, false, 1);

        // Act
        var validationContext = _service.ValidateInputSystemConfiguration(inputSystemConfiguration);

        // Assert
        Assert.Equal(InputValidationService.InputDeviceError, validationContext.ErrorCategory);
        Assert.True(validationContext.CheckErrorExists(InputValidationService.ValidationError_CollectionMissingData));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void ValidateInputSystemConfiguration_InvalidDeviceConfigurationNames_ReturnsInputDeviceMissingIdentifierError(string? deviceName)
    {
        // Arrange
        List<InputDefinition> definitions = [];

        var mockDeviceConfiguration1 = new Mock<IInputDeviceConfiguration>();
        mockDeviceConfiguration1.SetupGet(m => m.DeviceName)
            .Returns(new InputDeviceName(deviceName));

        List<IInputDeviceConfiguration>? deviceConfigurations = [ mockDeviceConfiguration1.Object ];

        var inputSystemConfiguration = new InputSystemConfiguration(definitions, [], deviceConfigurations, false, 1);

        // Act
        var validationContext = _service.ValidateInputSystemConfiguration(inputSystemConfiguration);

        // Assert
        Assert.Equal(InputValidationService.InputDeviceError, validationContext.ErrorCategory);
        Assert.True(validationContext.CheckErrorExists(InputValidationService.ValidationError_MissingIdentifier));
    }

    [Fact]
    public void ValidateInputSystemConfiguration_DuplicateDeviceConfigurationNames_ReturnsInputDeviceDuplicateIdentifierError()
    {
        // Arrange
        List<InputDefinition> definitions = [];

        var mockDeviceConfiguration1 = new Mock<IInputDeviceConfiguration>();
        mockDeviceConfiguration1.SetupGet(m => m.DeviceName)
            .Returns(new InputDeviceName("default"));

        var mockDeviceConfiguration2 = new Mock<IInputDeviceConfiguration>();
        mockDeviceConfiguration2.SetupGet(m => m.DeviceName)
            .Returns(new InputDeviceName("default"));

        List<IInputDeviceConfiguration>? deviceConfigurations = [mockDeviceConfiguration1.Object, mockDeviceConfiguration2.Object];

        var inputSystemConfiguration = new InputSystemConfiguration(definitions, [], deviceConfigurations, false, 1);

        // Act
        var validationContext = _service.ValidateInputSystemConfiguration(inputSystemConfiguration);

        // Assert
        Assert.Equal(InputValidationService.InputDeviceError, validationContext.ErrorCategory);
        Assert.True(validationContext.CheckErrorExists(InputValidationService.ValidationError_DuplicateIdentifier));
    }

    [Theory]
    [InlineData(null)]
    [InlineData(typeof(int))]
    public void ValidateInputSystemConfiguration_InvalidDeviceConfigurationInputReaderType_ReturnsInputDeviceInvalidDataError(Type? type)
    {
        // Arrange
        List<InputDefinition> definitions = [];

        var mockDeviceConfiguration1 = new Mock<IInputDeviceConfiguration>();
        mockDeviceConfiguration1.SetupGet(m => m.DeviceName)
            .Returns(new InputDeviceName("default"));
        mockDeviceConfiguration1.SetupGet(m => m.InputReaderType)
            .Returns(type!);

        List<IInputDeviceConfiguration>? deviceConfigurations = [mockDeviceConfiguration1.Object];

        var inputSystemConfiguration = new InputSystemConfiguration(definitions, [], deviceConfigurations, false, 1);

        // Act
        var validationContext = _service.ValidateInputSystemConfiguration(inputSystemConfiguration);

        // Assert
        Assert.Equal(InputValidationService.InputDeviceError, validationContext.ErrorCategory);
        Assert.True(validationContext.CheckErrorExists(InputValidationService.ValidationError_InvalidData));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void ValidateInputSystemConfiguration_DeviceConfigurationEmptyInputs_ReturnsInputDeviceMissingDataError(bool useNull)
    {
        // Arrange
        List<InputDefinition> definitions = [];

        var mockDeviceConfiguration1 = new Mock<IInputDeviceConfiguration>();
        mockDeviceConfiguration1.SetupGet(m => m.DeviceName)
            .Returns(new InputDeviceName("default"));

        mockDeviceConfiguration1.SetupGet(m => m.InputReaderType)
            .Returns(typeof(TestInputReader));

        mockDeviceConfiguration1.SetupGet(m => m.Inputs)
            .Returns(useNull ? null! : []);

        List<IInputDeviceConfiguration>? deviceConfigurations = [mockDeviceConfiguration1.Object];

        var inputSystemConfiguration = new InputSystemConfiguration(definitions, [], deviceConfigurations, false, 1);

        // Act
        var validationContext = _service.ValidateInputSystemConfiguration(inputSystemConfiguration);

        // Assert
        Assert.Equal(InputValidationService.InputDeviceError, validationContext.ErrorCategory);
        Assert.True(validationContext.CheckErrorExists(InputValidationService.ValidationError_CollectionMissingData));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void ValidateInputSystemConfiguration_DeviceConfigurationInvalidInputNames_ReturnsInputDeviceInvalidDataError(string? inputName)
    {
        // Arrange
        List<InputDefinition> definitions = [];

        var mockDeviceConfiguration1 = new Mock<IInputDeviceConfiguration>();
        mockDeviceConfiguration1.SetupGet(m => m.DeviceName)
            .Returns(new InputDeviceName("default"));
        mockDeviceConfiguration1.SetupGet(m => m.InputReaderType)
            .Returns(typeof(TestInputReader));

        var mockInput = new Mock<IInput>();
        mockInput.SetupGet(m => m.Name)
            .Returns(inputName!);

        mockDeviceConfiguration1.SetupGet(m => m.Inputs)
            .Returns([ mockInput.Object ]);

        List<IInputDeviceConfiguration>? deviceConfigurations = [mockDeviceConfiguration1.Object];

        var inputSystemConfiguration = new InputSystemConfiguration(definitions, 
            [ new InputControllerConfiguration("abc", [ mockDeviceConfiguration1.Object.DeviceName ]) ],
            deviceConfigurations, false, 1);

        // Act
        var validationContext = _service.ValidateInputSystemConfiguration(inputSystemConfiguration);

        // Assert
        Assert.Equal(InputValidationService.InputDeviceError, validationContext.ErrorCategory);
        Assert.True(validationContext.CheckErrorExists(InputValidationService.ValidationError_InvalidData));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void ValidateInputSystemConfiguration_EmptyInputDefinitions_ReturnsInputDefinitionMissingDataError(bool useNull)
    {
        // Arrange
        List<InputDefinition>? definitions = useNull ? null : [];

        var mockDeviceConfiguration = new Mock<IInputDeviceConfiguration>();
        mockDeviceConfiguration.SetupGet(m => m.DeviceName)
            .Returns(new InputDeviceName("abc"));
        mockDeviceConfiguration.SetupGet(m => m.InputReaderType)
            .Returns(typeof(TestInputReader));
        
        var mockInput = new Mock<IInput>();
        mockInput.SetupGet(m => m.Name)
            .Returns("abc");
        mockDeviceConfiguration.SetupGet(m => m.Inputs)
            .Returns([mockInput.Object]);

        List<IInputDeviceConfiguration> deviceConfigurations = [mockDeviceConfiguration.Object];

        var inputSystemConfiguration = new InputSystemConfiguration(definitions!,
            [ new InputControllerConfiguration("abc", [mockDeviceConfiguration.Object.DeviceName]) ],
            deviceConfigurations, false, 1);

        // Act
        var validationContext = _service.ValidateInputSystemConfiguration(inputSystemConfiguration);

        // Assert
        Assert.Equal(InputValidationService.InputDefinitionError, validationContext.ErrorCategory);
        Assert.True(validationContext.CheckErrorExists(InputValidationService.ValidationError_CollectionMissingData));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void ValidateInputSystemConfiguration_InvalidInputDefinitionNames_ReturnsInputDefinitionMissingIdentifierError(string? definitionName)
    {
        // Arrange
        List<InputDefinition> definitions = [new InputDefinition(definitionName!, [], [])];

        var mockDeviceConfiguration = new Mock<IInputDeviceConfiguration>();
        mockDeviceConfiguration.SetupGet(m => m.DeviceName)
            .Returns(new InputDeviceName("abc"));
        mockDeviceConfiguration.SetupGet(m => m.InputReaderType)
            .Returns(typeof(TestInputReader));

        var mockInput = new Mock<IInput>();
        mockInput.SetupGet(m => m.Name)
            .Returns("abc");
        mockDeviceConfiguration.SetupGet(m => m.Inputs)
            .Returns([mockInput.Object]);

        List<IInputDeviceConfiguration> deviceConfigurations = [mockDeviceConfiguration.Object];

        var inputSystemConfiguration = new InputSystemConfiguration(definitions, 
            [ new InputControllerConfiguration("abc", [ mockDeviceConfiguration.Object.DeviceName ]) ],
            deviceConfigurations, false, 1);

        // Act
        var validationContext = _service.ValidateInputSystemConfiguration(inputSystemConfiguration);

        // Assert
        Assert.Equal(InputValidationService.InputDefinitionError, validationContext.ErrorCategory);
        Assert.True(validationContext.CheckErrorExists(InputValidationService.ValidationError_MissingIdentifier));
    }

    [Fact]
    public void ValidateInputSystemConfiguration_DuplicateInputDefinitionNames_ReturnsInputDefinitionDuplicateIdentifierError()
    {
        // Arrange
        List<InputDefinition> definitions = [new InputDefinition("abc", [], []), new InputDefinition("abc", [], [])];

        var mockDeviceConfiguration = new Mock<IInputDeviceConfiguration>();
        mockDeviceConfiguration.SetupGet(m => m.DeviceName)
            .Returns(new InputDeviceName("abc"));
        mockDeviceConfiguration.SetupGet(m => m.InputReaderType)
            .Returns(typeof(TestInputReader));

        var mockInput = new Mock<IInput>();
        mockInput.SetupGet(m => m.Name)
            .Returns("abc");
        mockDeviceConfiguration.SetupGet(m => m.Inputs)
            .Returns([mockInput.Object]);

        List<IInputDeviceConfiguration> deviceConfigurations = [mockDeviceConfiguration.Object];

        var inputSystemConfiguration = new InputSystemConfiguration(definitions,
            [new InputControllerConfiguration("abc", [mockDeviceConfiguration.Object.DeviceName])],
            deviceConfigurations, false, 1);

        // Act  
        var validationContext = _service.ValidateInputSystemConfiguration(inputSystemConfiguration);

        // Assert
        Assert.Equal(InputValidationService.InputDefinitionError, validationContext.ErrorCategory);
        Assert.True(validationContext.CheckErrorExists(InputValidationService.ValidationError_DuplicateIdentifier));
    }

    [Fact]
    public void ValidateInputSystemConfiguration_InputDefinitionWithEmptyInputActions_ReturnsInputDefinitionMissingDataError()
    {
        // Arrange
        List<InputDefinition> definitions = [new InputDefinition("abc", [], [])];

        var mockDeviceConfiguration = new Mock<IInputDeviceConfiguration>();
        mockDeviceConfiguration.SetupGet(m => m.DeviceName)
            .Returns(new InputDeviceName("abc"));
        mockDeviceConfiguration.SetupGet(m => m.InputReaderType)
            .Returns(typeof(TestInputReader));

        var mockInput = new Mock<IInput>();
        mockInput.SetupGet(m => m.Name)
            .Returns("abc");
        mockDeviceConfiguration.SetupGet(m => m.Inputs)
            .Returns([mockInput.Object]);

        List<IInputDeviceConfiguration> deviceConfigurations = [mockDeviceConfiguration.Object];

        var inputSystemConfiguration = new InputSystemConfiguration(definitions,
            [new InputControllerConfiguration("abc", [mockDeviceConfiguration.Object.DeviceName])],
            deviceConfigurations, false, 1);

        // Act
        var validationContext = _service.ValidateInputSystemConfiguration(inputSystemConfiguration);

        // Assert
        Assert.Equal(InputValidationService.InputDefinitionError, validationContext.ErrorCategory);
        Assert.True(validationContext.CheckErrorExists(InputValidationService.ValidationError_CollectionMissingData));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void ValidateInputSystemConfiguration_InputDefinitionWithEmptyInputActionKeys_ReturnsInputDefinitionInvalidDataError(string? inputActionKey)
    {
        // Arrange
        List<InputDefinition> definitions = [new InputDefinition("abc", [new InputAction(inputActionKey!, _ => ValueTask.CompletedTask, null)], [])];

        var mockDeviceConfiguration = new Mock<IInputDeviceConfiguration>();
        mockDeviceConfiguration.SetupGet(m => m.DeviceName)
            .Returns(new InputDeviceName("abc"));
        mockDeviceConfiguration.SetupGet(m => m.InputReaderType)
            .Returns(typeof(TestInputReader));

        var mockInput = new Mock<IInput>();
        mockInput.SetupGet(m => m.Name)
            .Returns("abc");
        mockDeviceConfiguration.SetupGet(m => m.Inputs)
            .Returns([mockInput.Object]);

        List<IInputDeviceConfiguration> deviceConfigurations = [mockDeviceConfiguration.Object];

        var inputSystemConfiguration = new InputSystemConfiguration(definitions,
            [new InputControllerConfiguration("abc", [mockDeviceConfiguration.Object.DeviceName])],
            deviceConfigurations, false, 1);

        // Act
        var validationContext = _service.ValidateInputSystemConfiguration(inputSystemConfiguration);

        // Assert
        Assert.Equal(InputValidationService.InputDefinitionError, validationContext.ErrorCategory);
        Assert.True(validationContext.CheckErrorExists(InputValidationService.ValidationError_InvalidData));
    }

    [Fact]
    public void ValidateInputSystemConfiguration_InputActionWithNullActionExecutor_ReturnsInputDefinitionInvalidDataError()
    {
        // Arrange
        List<InputDefinition> definitions = [
            new InputDefinition("abc", [
                new InputAction("a", null!, null)
                ], [])
            ];

        var mockDeviceConfiguration = new Mock<IInputDeviceConfiguration>();
        mockDeviceConfiguration.SetupGet(m => m.DeviceName)
            .Returns(new InputDeviceName("abc"));
        mockDeviceConfiguration.SetupGet(m => m.InputReaderType)
            .Returns(typeof(TestInputReader));

        var mockInput = new Mock<IInput>();
        mockInput.SetupGet(m => m.Name)
            .Returns("abc");
        mockDeviceConfiguration.SetupGet(m => m.Inputs)
            .Returns([mockInput.Object]);

        List<IInputDeviceConfiguration> deviceConfigurations = [mockDeviceConfiguration.Object];

        var inputSystemConfiguration = new InputSystemConfiguration(definitions,
            [new InputControllerConfiguration("abc", [mockDeviceConfiguration.Object.DeviceName])],
            deviceConfigurations, false, 1);

        // Act
        var validationContext = _service.ValidateInputSystemConfiguration(inputSystemConfiguration);

        // Assert
        Assert.Equal(InputValidationService.InputActionError, validationContext.ErrorCategory);
        Assert.True(validationContext.CheckErrorExists(InputValidationService.ValidationError_InvalidData));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    public void ValidateInputSystemConfiguration_InvalidMaxLocalUser_ReturnsInputSystemConfigurationInvalidDataError(int maxLocalusers)
    {
        // Arrange
        List<InputDefinition> definitions = [
            new InputDefinition("abc", 
                [ new InputAction("a", _ => ValueTask.CompletedTask, null) ], 
                [ 
                    new InputScheme("abc", "abc", false, 
                    [ 
                        new InputDeviceActionMap(new InputDeviceName("abc"), [new InputActionMap("a", 1, new HashSet<InputPhase>() { InputPhase.Start })]) 
                    ]) 
                ])
         ];

        var mockDeviceConfiguration = new Mock<IInputDeviceConfiguration>();
        mockDeviceConfiguration.SetupGet(m => m.DeviceName)
            .Returns(new InputDeviceName("abc"));
        mockDeviceConfiguration.SetupGet(m => m.InputReaderType)
            .Returns(typeof(TestInputReader));

        var mockInput = new Mock<IInput>();
        mockInput.SetupGet(m => m.Name)
            .Returns("abc");
        mockDeviceConfiguration.SetupGet(m => m.Inputs)
            .Returns([mockInput.Object]);

        List<IInputDeviceConfiguration> deviceConfigurations = [mockDeviceConfiguration.Object];

        var inputSystemConfiguration = new InputSystemConfiguration(definitions,
            [new InputControllerConfiguration("abc", [mockDeviceConfiguration.Object.DeviceName])],
            deviceConfigurations, false, maxLocalusers);

        // Act
        var validationContext = _service.ValidateInputSystemConfiguration(inputSystemConfiguration);

        // Assert
        Assert.Equal(InputValidationService.InputSystemConfigurationError, validationContext.ErrorCategory);
        Assert.True(validationContext.CheckErrorExists(InputValidationService.ValidationError_InvalidData));
    }

    [Fact]
    public void ValidateInputSystemConfiguration_Valid_ReturnsSuccessfully()
    {        
        // Arrange
        var mockDeviceConfiguration = new Mock<IInputDeviceConfiguration>();
        mockDeviceConfiguration.SetupGet(m => m.DeviceName)
            .Returns(new InputDeviceName("abc"));
        mockDeviceConfiguration.SetupGet(m => m.InputReaderType)
            .Returns(typeof(TestInputReader));

        List<InputDefinition> definitions = [
            new InputDefinition("abc",
                [ new InputAction("a", _ => ValueTask.CompletedTask, null) ],
                [ new InputScheme("abc", "abc", false,
                    [
                        new InputDeviceActionMap(mockDeviceConfiguration.Object.DeviceName, [new InputActionMap("a", 1, new HashSet<InputPhase>() { InputPhase.Start })])
                    ])
                ])
         ];

        var mockInput = new Mock<IInput>();
        mockInput.SetupGet(m => m.Name)
            .Returns("abc");
        mockDeviceConfiguration.SetupGet(m => m.Inputs)
            .Returns([mockInput.Object]);

        List<IInputDeviceConfiguration> deviceConfigurations = [mockDeviceConfiguration.Object];

        var inputSystemConfiguration = new InputSystemConfiguration(definitions,
            [new InputControllerConfiguration("abc", [mockDeviceConfiguration.Object.DeviceName])],
            deviceConfigurations, false, 1);

        // Act
        var validationContext = _service.ValidateInputSystemConfiguration(inputSystemConfiguration);

        // Assert
        Assert.Empty(validationContext.Errors);
    }

    #endregion

    #region ValidateCustomInputScheme

    [Fact]
    public void ValidateCustomInputScheme_NullInputSystemConfiguration_ThrowsArgumentNullException()
    {
        // Arrange/Act/Assert
        Assert.Throws<ArgumentNullException>(() => _service.ValidateCustomInputScheme(null!, new InputScheme("abc", "abc", false, [])));
    }

    [Fact]
    public void ValidateCustomInputScheme_NullInputScheme_ThrowsArgumentNullException()
    {
        // Arrange/Act/Assert
        Assert.Throws<ArgumentNullException>(() => _service.ValidateCustomInputScheme(new InputSystemConfiguration([], [], [], false, 2), null!));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void ValidateCustomInputScheme_InvalidInputSchemeDefinitionName_AddsInvalidDataErrorToContext(string? definitionName)
    {
        // Arrange/Act
        var validationContext = _service.ValidateCustomInputScheme(new InputSystemConfiguration([], [], [], false, 2), 
            new InputScheme(definitionName!, "abc", false, []));

        // Assert
        Assert.Equal(InputValidationService.InputSchemeError, validationContext.ErrorCategory);
        Assert.True(validationContext.CheckErrorExists(InputValidationService.ValidationError_InvalidData));
    }

    [Fact]
    public void ValidateCustomInputScheme_InputSchemeDefinitionNameDoesNotMatchInputDefinition_AddsMismatchedTenantErrorToContext()
    {
        // Arrange/Act
        var validationContext = _service.ValidateCustomInputScheme(new InputSystemConfiguration([], [], [], false, 2), new InputScheme("def", "abc", false, []));

        // Assert
        Assert.Equal(InputValidationService.InputSchemeError, validationContext.ErrorCategory);
        Assert.True(validationContext.CheckErrorExists(InputValidationService.ValidationError_MismatchedTenant));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void ValidateCustomInputScheme_InputSchemeMissingControllerName_AddsInvalidDataErrorToContext(string? controllereName)
    {
        // Arrange/Act
        var validationContext = _service.ValidateCustomInputScheme(new InputSystemConfiguration([ new InputDefinition("abc", [], [])], [], [], false, 2), 
            new InputScheme("abc", controllereName!, false, []));

        // Assert
        Assert.Equal(InputValidationService.InputSchemeError, validationContext.ErrorCategory);
        Assert.True(validationContext.CheckErrorExists(InputValidationService.ValidationError_InvalidData));
    }

    [Fact]
    public void ValidateCustomInputScheme_InputSchemeControllerNameNotSupportedByConfiguration_AddsInvalidDataErrorToContext()
    {
        // Arrange/Act
        var validationContext = _service.ValidateCustomInputScheme(
            new InputSystemConfiguration([new InputDefinition("abc", [], [])], [], [], false, 2),
            new InputScheme("abc", "device", false, []));

        // Assert
        Assert.Equal(InputValidationService.InputSchemeError, validationContext.ErrorCategory);
        Assert.True(validationContext.CheckErrorExists(InputValidationService.ValidationError_InvalidData));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void ValidateCustomInputScheme_InputSchemeInvalidSchemeName_AddsMissingIdentifierErrorToContext(string? schemeName)
    {
        // Arrange
        var mockDeviceConfiguration = new Mock<IInputDeviceConfiguration>();
        mockDeviceConfiguration.SetupGet(m => m.DeviceName)
            .Returns(new InputDeviceName("abc"));

        // Act
        var validationContext = _service.ValidateCustomInputScheme(
            new InputSystemConfiguration([new InputDefinition("abc", [], [])],
            [new InputControllerConfiguration("abc", [mockDeviceConfiguration.Object.DeviceName])],
            [mockDeviceConfiguration.Object], false, 2),
            new InputScheme("abc", schemeName!, false, [new InputDeviceActionMap(mockDeviceConfiguration.Object.DeviceName, [])]));

        // Assert
        Assert.Equal(InputValidationService.InputSchemeError, validationContext.ErrorCategory);
        Assert.True(validationContext.CheckErrorExists(InputValidationService.ValidationError_MissingIdentifier));
    }

    [Theory]
    [InlineData("aBcD", "abcd")]
    [InlineData("efgh", "efgh")]
    [InlineData("IJKL", "IJKL")]
    public void ValidateCustomInputScheme_DuplicateInputSchemeName_AddsDuplicateIdentifierErrorToContext(string schemeName1, string schemeName2)
    {
        // Arrange
        var mockDeviceConfiguration = new Mock<IInputDeviceConfiguration>();
        mockDeviceConfiguration.SetupGet(m => m.DeviceName)
            .Returns(new InputDeviceName("abc"));

        // Act
        var validationContext = _service.ValidateCustomInputScheme(
            new InputSystemConfiguration([new InputDefinition("abc", [], [ new InputScheme("abc", schemeName1, false, []) ])],
            [new InputControllerConfiguration("abc", [mockDeviceConfiguration.Object.DeviceName])],
            [ mockDeviceConfiguration.Object ], false, 2),
            new InputScheme("abc", schemeName2, false, [ new InputDeviceActionMap(mockDeviceConfiguration.Object.DeviceName, []) ]));

        // Assert
        Assert.Equal(InputValidationService.InputSchemeError, validationContext.ErrorCategory);
        Assert.True(validationContext.CheckErrorExists(InputValidationService.ValidationError_DuplicateIdentifier));
    }

    [Fact]
    public void ValidateCustomInputScheme_DuplicateInputIdOnSchemeDeviceActionMap_AddsInvalidDataErrorToContext()
    {
        // Arrange
        var mockDeviceConfiguration = new Mock<IInputDeviceConfiguration>();
        mockDeviceConfiguration.SetupGet(m => m.DeviceName)
            .Returns(new InputDeviceName("abc"));

        // Act
        var validationContext = _service.ValidateCustomInputScheme(
            new InputSystemConfiguration([new InputDefinition("abc", [], [])], 
                [ new InputControllerConfiguration("abc", [mockDeviceConfiguration.Object.DeviceName]) ], 
                [mockDeviceConfiguration.Object], 
                false, 2),
            new InputScheme("abc", "abc", false,
            [
                new InputDeviceActionMap(mockDeviceConfiguration.Object.DeviceName,
                [
                    new InputActionMap("actionKey", 1, new HashSet<InputPhase>() { InputPhase.Start }),
                    new InputActionMap("actionKey2", 1, new HashSet < InputPhase >() { InputPhase.Start })
                ])
             ]));

        // Assert
        Assert.Equal(InputValidationService.InputActionMapError, validationContext.ErrorCategory);
        Assert.True(validationContext.CheckErrorExists(InputValidationService.ValidationError_InvalidData));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void ValidateCustomInputScheme_InvalidInputDeviceConfigurationActionMapActionKey_AddsInvalidDataErrorToContext(string? actionKey)
    {
        // Arrange
        var mockDeviceConfiguration = new Mock<IInputDeviceConfiguration>();
        mockDeviceConfiguration.SetupGet(m => m.DeviceName)
            .Returns(new InputDeviceName("abc"));

        // Act
        var validationContext = _service.ValidateCustomInputScheme(
            new InputSystemConfiguration([new InputDefinition("abc", [], [])],
            [new InputControllerConfiguration("abc", [mockDeviceConfiguration.Object.DeviceName])],
            [mockDeviceConfiguration.Object], false, 2),
            new InputScheme("abc", "abc", false,
                    [
                        new InputDeviceActionMap(mockDeviceConfiguration.Object.DeviceName,
                        [
                            new InputActionMap(actionKey!, 1, new HashSet < InputPhase >() { InputPhase.Start })
                        ])
                    ]));

        // Assert
        Assert.Equal(InputValidationService.InputActionMapError, validationContext.ErrorCategory);
        Assert.True(validationContext.CheckErrorExists(InputValidationService.ValidationError_InvalidData));
    }

    [Theory]
    [InlineData("aBcD", "abcd")]
    [InlineData("efgh", "efgh")]
    [InlineData("IJKL", "IJKL")]
    public void ValidateCustomInputScheme_DuplicateInputDeviceConfigurationActionMapActionKey_AddsInvalidDataErrorToContext(string actionKey1, string actionKey2)
    {        
        // Arrange
        var mockDeviceConfiguration = new Mock<IInputDeviceConfiguration>();
        mockDeviceConfiguration.SetupGet(m => m.DeviceName)
            .Returns(new InputDeviceName("abc"));

        // Act
        var validationContext = _service.ValidateCustomInputScheme(
            new InputSystemConfiguration([new InputDefinition("abc", [], [])],
            [new InputControllerConfiguration("abc", [mockDeviceConfiguration.Object.DeviceName])],
            [mockDeviceConfiguration.Object], false, 2),
            new InputScheme("abc", "abc", false,
                    [
                        new InputDeviceActionMap(mockDeviceConfiguration.Object.DeviceName,
                        [
                            new InputActionMap(actionKey1, 1, new HashSet < InputPhase >() { InputPhase.Start }),
                            new InputActionMap(actionKey2, 2, new HashSet < InputPhase >() { InputPhase.Start })
                        ])
                    ]));

        // Assert
        Assert.Equal(InputValidationService.InputActionMapError, validationContext.ErrorCategory);
        Assert.True(validationContext.CheckErrorExists(InputValidationService.ValidationError_InvalidData));
    }

    [Fact]
    public void ValidateCustomInputScheme_SchemeMissingActionKeysInDefinition_AddsMissingDataErrorToContext()
    {
        // Arrange
        var mockDeviceConfiguration = new Mock<IInputDeviceConfiguration>();
        mockDeviceConfiguration.SetupGet(m => m.DeviceName)
            .Returns(new InputDeviceName("abc"));

        // Act
        var validationContext = _service.ValidateCustomInputScheme(
            new InputSystemConfiguration([
                new InputDefinition("abc", 
                [
                    new InputAction("abc", _ => ValueTask.CompletedTask, null), 
                    new InputAction("def", _ => ValueTask.CompletedTask, null)
                ], [])],
                [new InputControllerConfiguration("abc", [mockDeviceConfiguration.Object.DeviceName])],
                [mockDeviceConfiguration.Object], false, 2),
            new InputScheme("abc", "abc", false,
                    [
                        new InputDeviceActionMap(mockDeviceConfiguration.Object.DeviceName,
                        [
                            new InputActionMap("abc", 1, new HashSet<InputPhase>() { InputPhase.Start })
                        ])
                    ]));

        // Assert
        Assert.Equal(InputValidationService.InputSchemeError, validationContext.ErrorCategory);
        Assert.True(validationContext.CheckErrorExists(InputValidationService.ValidationError_CollectionMissingData));
    }

    [Fact]
    public void ValidateCustomInputScheme_Valid_ReturnsSuccessfully()
    {
        // Arrange
        var mockDeviceConfiguration = new Mock<IInputDeviceConfiguration>();
        mockDeviceConfiguration.SetupGet(m => m.DeviceName)
            .Returns(new InputDeviceName("abc"));

        // Act
        var validationContext = _service.ValidateCustomInputScheme(
            new InputSystemConfiguration([new InputDefinition("abc",
            [
                new InputAction("abc", _ => ValueTask.CompletedTask, null),
                new InputAction("def", _ => ValueTask.CompletedTask, null)],
                [])],
                [new InputControllerConfiguration("abc", [mockDeviceConfiguration.Object.DeviceName])],
                [mockDeviceConfiguration.Object], false, 2),
            new InputScheme("abc", "abc", false,
                    [
                        new InputDeviceActionMap(mockDeviceConfiguration.Object.DeviceName,
                        [
                            new InputActionMap("abc", 1, new HashSet<InputPhase>() {InputPhase.Start}),
                            new InputActionMap("def", 2, new HashSet<InputPhase>() {InputPhase.Start})
                        ])
                    ]));

        // Assert
        Assert.Empty(validationContext.Errors);
    }

    #endregion
}
