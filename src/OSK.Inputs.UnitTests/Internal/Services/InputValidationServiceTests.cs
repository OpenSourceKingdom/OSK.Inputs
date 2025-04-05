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
    public void ValidateInputSystemConfiguration_EmptyControllerConfiguration_ReturnsInputControllerMissingDataError(bool useNull)
    {
        // Arrange
        List<InputDefinition> definitions = [];
        List<IInputControllerConfiguration>? controllerConfigurations = useNull ? null : [];

        var inputSystemConfiguration = new InputSystemConfiguration(definitions, controllerConfigurations!, false, 1);

        // Act
        var validationContext = _service.ValidateInputSystemConfiguration(inputSystemConfiguration);

        // Assert
        Assert.Equal(InputValidationService.InputControllerError, validationContext.ErrorCategory);
        Assert.True(validationContext.CheckErrorExists(InputValidationService.ValidationError_CollectionMissingData));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void ValidateInputSystemConfiguration_InvalidControllerConfigurationNames_ReturnsInputControllerMissingIdentifierError(string? controllerName)
    {
        // Arrange
        List<InputDefinition> definitions = [];

        var mockControllerConfiguration1 = new Mock<IInputControllerConfiguration>();
        mockControllerConfiguration1.SetupGet(m => m.ControllerName)
            .Returns(new InputControllerName(controllerName));

        List<IInputControllerConfiguration>? controllerConfigurations = [ mockControllerConfiguration1.Object ];

        var inputSystemConfiguration = new InputSystemConfiguration(definitions, controllerConfigurations, false, 1);

        // Act
        var validationContext = _service.ValidateInputSystemConfiguration(inputSystemConfiguration);

        // Assert
        Assert.Equal(InputValidationService.InputControllerError, validationContext.ErrorCategory);
        Assert.True(validationContext.CheckErrorExists(InputValidationService.ValidationError_MissingIdentifier));
    }

    [Fact]
    public void ValidateInputSystemConfiguration_DuplicateControllerConfigurationNames_ReturnsInputControllerDuplicateIdentifierError()
    {
        // Arrange
        List<InputDefinition> definitions = [];

        var mockControllerConfiguration1 = new Mock<IInputControllerConfiguration>();
        mockControllerConfiguration1.SetupGet(m => m.ControllerName)
            .Returns(new InputControllerName("default"));

        var mockControllerConfiguration2 = new Mock<IInputControllerConfiguration>();
        mockControllerConfiguration2.SetupGet(m => m.ControllerName)
            .Returns(new InputControllerName("default"));

        List<IInputControllerConfiguration>? controllerConfigurations = [mockControllerConfiguration1.Object, mockControllerConfiguration2.Object];

        var inputSystemConfiguration = new InputSystemConfiguration(definitions, controllerConfigurations, false, 1);

        // Act
        var validationContext = _service.ValidateInputSystemConfiguration(inputSystemConfiguration);

        // Assert
        Assert.Equal(InputValidationService.InputControllerError, validationContext.ErrorCategory);
        Assert.True(validationContext.CheckErrorExists(InputValidationService.ValidationError_DuplicateIdentifier));
    }

    [Theory]
    [InlineData(null)]
    [InlineData(typeof(int))]
    public void ValidateInputSystemConfiguration_InvalidControllerConfigurationInputReaderType_ReturnsInputControllerInvalidDataError(Type? type)
    {
        // Arrange
        List<InputDefinition> definitions = [];

        var mockControllerConfiguration1 = new Mock<IInputControllerConfiguration>();
        mockControllerConfiguration1.SetupGet(m => m.ControllerName)
            .Returns(new InputControllerName("default"));
        mockControllerConfiguration1.SetupGet(m => m.InputReaderType)
            .Returns(type!);

        List<IInputControllerConfiguration>? controllerConfigurations = [mockControllerConfiguration1.Object];

        var inputSystemConfiguration = new InputSystemConfiguration(definitions, controllerConfigurations, false, 1);

        // Act
        var validationContext = _service.ValidateInputSystemConfiguration(inputSystemConfiguration);

        // Assert
        Assert.Equal(InputValidationService.InputControllerError, validationContext.ErrorCategory);
        Assert.True(validationContext.CheckErrorExists(InputValidationService.ValidationError_InvalidData));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void ValidateInputSystemConfiguration_ControllerConfigurationEmptyInputs_ReturnsInputControllerMissingDataError(bool useNull)
    {
        // Arrange
        List<InputDefinition> definitions = [];

        var mockControllerConfiguration1 = new Mock<IInputControllerConfiguration>();
        mockControllerConfiguration1.SetupGet(m => m.ControllerName)
            .Returns(new InputControllerName("default"));

        mockControllerConfiguration1.SetupGet(m => m.InputReaderType)
            .Returns(typeof(TestInputReader));

        mockControllerConfiguration1.SetupGet(m => m.Inputs)
            .Returns(useNull ? null! : []);

        List<IInputControllerConfiguration>? controllerConfigurations = [mockControllerConfiguration1.Object];

        var inputSystemConfiguration = new InputSystemConfiguration(definitions, controllerConfigurations, false, 1);

        // Act
        var validationContext = _service.ValidateInputSystemConfiguration(inputSystemConfiguration);

        // Assert
        Assert.Equal(InputValidationService.InputControllerError, validationContext.ErrorCategory);
        Assert.True(validationContext.CheckErrorExists(InputValidationService.ValidationError_CollectionMissingData));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void ValidateInputSystemConfiguration_ControllerConfigurationInvalidInputNames_ReturnsInputControllerInvalidDataError(string? inputName)
    {
        // Arrange
        List<InputDefinition> definitions = [];

        var mockControllerConfiguration1 = new Mock<IInputControllerConfiguration>();
        mockControllerConfiguration1.SetupGet(m => m.ControllerName)
            .Returns(new InputControllerName("default"));
        mockControllerConfiguration1.SetupGet(m => m.InputReaderType)
            .Returns(typeof(TestInputReader));

        var mockInput = new Mock<IInput>();
        mockInput.SetupGet(m => m.Name)
            .Returns(inputName!);

        mockControllerConfiguration1.SetupGet(m => m.Inputs)
            .Returns([ mockInput.Object ]);

        List<IInputControllerConfiguration>? controllerConfigurations = [mockControllerConfiguration1.Object];

        var inputSystemConfiguration = new InputSystemConfiguration(definitions, controllerConfigurations, false, 1);

        // Act
        var validationContext = _service.ValidateInputSystemConfiguration(inputSystemConfiguration);

        // Assert
        Assert.Equal(InputValidationService.InputControllerError, validationContext.ErrorCategory);
        Assert.True(validationContext.CheckErrorExists(InputValidationService.ValidationError_InvalidData));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void ValidateInputSystemConfiguration_EmptyInputDefinitions_ReturnsInputDefinitionMissingDataError(bool useNull)
    {
        // Arrange
        List<InputDefinition>? definitions = useNull ? null : [];

        var mockControllerConfiguration = new Mock<IInputControllerConfiguration>();
        mockControllerConfiguration.SetupGet(m => m.ControllerName)
            .Returns(new InputControllerName("abc"));
        mockControllerConfiguration.SetupGet(m => m.InputReaderType)
            .Returns(typeof(TestInputReader));
        
        var mockInput = new Mock<IInput>();
        mockInput.SetupGet(m => m.Name)
            .Returns("abc");
        mockControllerConfiguration.SetupGet(m => m.Inputs)
            .Returns([mockInput.Object]);

        List<IInputControllerConfiguration> controllerConfigurations = [mockControllerConfiguration.Object];

        var inputSystemConfiguration = new InputSystemConfiguration(definitions!, controllerConfigurations, false, 1);

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

        var mockControllerConfiguration = new Mock<IInputControllerConfiguration>();
        mockControllerConfiguration.SetupGet(m => m.ControllerName)
            .Returns(new InputControllerName("abc"));
        mockControllerConfiguration.SetupGet(m => m.InputReaderType)
            .Returns(typeof(TestInputReader));

        var mockInput = new Mock<IInput>();
        mockInput.SetupGet(m => m.Name)
            .Returns("abc");
        mockControllerConfiguration.SetupGet(m => m.Inputs)
            .Returns([mockInput.Object]);

        List<IInputControllerConfiguration> controllerConfigurations = [mockControllerConfiguration.Object];

        var inputSystemConfiguration = new InputSystemConfiguration(definitions, controllerConfigurations, false, 1);

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

        var mockControllerConfiguration = new Mock<IInputControllerConfiguration>();
        mockControllerConfiguration.SetupGet(m => m.ControllerName)
            .Returns(new InputControllerName("abc"));
        mockControllerConfiguration.SetupGet(m => m.InputReaderType)
            .Returns(typeof(TestInputReader));

        var mockInput = new Mock<IInput>();
        mockInput.SetupGet(m => m.Name)
            .Returns("abc");
        mockControllerConfiguration.SetupGet(m => m.Inputs)
            .Returns([mockInput.Object]);

        List<IInputControllerConfiguration> controllerConfigurations = [mockControllerConfiguration.Object];

        var inputSystemConfiguration = new InputSystemConfiguration(definitions, controllerConfigurations, false, 1);

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

        var mockControllerConfiguration = new Mock<IInputControllerConfiguration>();
        mockControllerConfiguration.SetupGet(m => m.ControllerName)
            .Returns(new InputControllerName("abc"));
        mockControllerConfiguration.SetupGet(m => m.InputReaderType)
            .Returns(typeof(TestInputReader));

        var mockInput = new Mock<IInput>();
        mockInput.SetupGet(m => m.Name)
            .Returns("abc");
        mockControllerConfiguration.SetupGet(m => m.Inputs)
            .Returns([mockInput.Object]);

        List<IInputControllerConfiguration> controllerConfigurations = [mockControllerConfiguration.Object];

        var inputSystemConfiguration = new InputSystemConfiguration(definitions, controllerConfigurations, false, 1);

        // Act
        var validationContext = _service.ValidateInputSystemConfiguration(inputSystemConfiguration);

        // Assert
        Assert.Equal(InputValidationService.InputDefinitionError, validationContext.ErrorCategory);
        Assert.True(validationContext.CheckErrorExists(InputValidationService.ValidationError_CollectionMissingData));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void ValidateInputSystemConfiguration_InputDefinitionWithEmptyInputActionKeys_ReturnsInputDefinitionInvalidDataError(string? inputActionKey)
    {
        // Arrange
        List<InputDefinition> definitions = [new InputDefinition("abc", [new InputAction(inputActionKey!, null)], [])];

        var mockControllerConfiguration = new Mock<IInputControllerConfiguration>();
        mockControllerConfiguration.SetupGet(m => m.ControllerName)
            .Returns(new InputControllerName("abc"));
        mockControllerConfiguration.SetupGet(m => m.InputReaderType)
            .Returns(typeof(TestInputReader));

        var mockInput = new Mock<IInput>();
        mockInput.SetupGet(m => m.Name)
            .Returns("abc");
        mockControllerConfiguration.SetupGet(m => m.Inputs)
            .Returns([mockInput.Object]);

        List<IInputControllerConfiguration> controllerConfigurations = [mockControllerConfiguration.Object];

        var inputSystemConfiguration = new InputSystemConfiguration(definitions, controllerConfigurations, false, 1);

        // Act
        var validationContext = _service.ValidateInputSystemConfiguration(inputSystemConfiguration);

        // Assert
        Assert.Equal(InputValidationService.InputDefinitionError, validationContext.ErrorCategory);
        Assert.True(validationContext.CheckErrorExists(InputValidationService.ValidationError_InvalidData));
    }

    [Fact]
    public void ValidateInputSystemConfiguration_InputDefinitionWithDuplicateInputActionKeys_ReturnsInputDefinitionInvalidDataError()
    {
        // Arrange
        List<InputDefinition> definitions = [new InputDefinition("abc", [new InputAction("a", null), new InputAction("a", null)], [])];

        var mockControllerConfiguration = new Mock<IInputControllerConfiguration>();
        mockControllerConfiguration.SetupGet(m => m.ControllerName)
            .Returns(new InputControllerName("abc"));
        mockControllerConfiguration.SetupGet(m => m.InputReaderType)
            .Returns(typeof(TestInputReader));

        var mockInput = new Mock<IInput>();
        mockInput.SetupGet(m => m.Name)
            .Returns("abc");
        mockControllerConfiguration.SetupGet(m => m.Inputs)
            .Returns([mockInput.Object]);

        List<IInputControllerConfiguration> controllerConfigurations = [mockControllerConfiguration.Object];

        var inputSystemConfiguration = new InputSystemConfiguration(definitions, controllerConfigurations, false, 1);

        // Act
        var validationContext = _service.ValidateInputSystemConfiguration(inputSystemConfiguration);

        // Assert
        Assert.Equal(InputValidationService.InputDefinitionError, validationContext.ErrorCategory);
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
                [ new InputAction("a", null) ], 
                [ new InputScheme("abc", "abc", "abc", false, [ new InputActionMap("a", "abc", InputPhase.Start) ]) ])
         ];

        var mockControllerConfiguration = new Mock<IInputControllerConfiguration>();
        mockControllerConfiguration.SetupGet(m => m.ControllerName)
            .Returns(new InputControllerName("abc"));
        mockControllerConfiguration.SetupGet(m => m.InputReaderType)
            .Returns(typeof(TestInputReader));

        var mockInput = new Mock<IInput>();
        mockInput.SetupGet(m => m.Name)
            .Returns("abc");
        mockControllerConfiguration.SetupGet(m => m.Inputs)
            .Returns([mockInput.Object]);

        List<IInputControllerConfiguration> controllerConfigurations = [mockControllerConfiguration.Object];

        var inputSystemConfiguration = new InputSystemConfiguration(definitions, controllerConfigurations, false, maxLocalusers);

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
        List<InputDefinition> definitions = [
            new InputDefinition("abc",
                [ new InputAction("a", null) ],
                [ new InputScheme("abc", "abc", "abc", false, [ new InputActionMap("a", "abc", InputPhase.Start) ]) ])
         ];

        var mockControllerConfiguration = new Mock<IInputControllerConfiguration>();
        mockControllerConfiguration.SetupGet(m => m.ControllerName)
            .Returns(new InputControllerName("abc"));
        mockControllerConfiguration.SetupGet(m => m.InputReaderType)
            .Returns(typeof(TestInputReader));

        var mockInput = new Mock<IInput>();
        mockInput.SetupGet(m => m.Name)
            .Returns("abc");
        mockControllerConfiguration.SetupGet(m => m.Inputs)
            .Returns([mockInput.Object]);

        List<IInputControllerConfiguration> controllerConfigurations = [mockControllerConfiguration.Object];

        var inputSystemConfiguration = new InputSystemConfiguration(definitions, controllerConfigurations, false, 1);

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
        Assert.Throws<ArgumentNullException>(() => _service.ValidateCustomInputScheme(null!, new InputScheme("abc", "abc", "abc", false, [])));
    }

    [Fact]
    public void ValidateCustomInputScheme_NullInputScheme_ThrowsArgumentNullException()
    {
        // Arrange/Act/Assert
        Assert.Throws<ArgumentNullException>(() => _service.ValidateCustomInputScheme(new InputSystemConfiguration([], [], false, 2), null!));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void ValidateCustomInputScheme_InvalidInputSchemeDefinitionName_AddsInvalidDataErrorToContext(string? definitionName)
    {
        // Arrange/Act
        var validationContext = _service.ValidateCustomInputScheme(new InputSystemConfiguration([], [], false, 2), 
            new InputScheme(definitionName!, "abc", "abc", false, []));

        // Assert
        Assert.Equal(InputValidationService.InputSchemeError, validationContext.ErrorCategory);
        Assert.True(validationContext.CheckErrorExists(InputValidationService.ValidationError_InvalidData));
    }

    [Fact]
    public void ValidateCustomInputScheme_InputSchemeDefinitionNameDoesNotMatchInputDefinition_AddsMismatchedTenantErrorToContext()
    {
        // Arrange/Act
        var validationContext = _service.ValidateCustomInputScheme(new InputSystemConfiguration([], [], false, 2), new InputScheme("def", "abc", "abc", false, []));

        // Assert
        Assert.Equal(InputValidationService.InputSchemeError, validationContext.ErrorCategory);
        Assert.True(validationContext.CheckErrorExists(InputValidationService.ValidationError_MismatchedTenant));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void ValidateCustomInputScheme_InputSchemeMissingControllerName_AddsInvalidDataErrorToContext(string? controllerName)
    {
        // Arrange/Act
        var validationContext = _service.ValidateCustomInputScheme(new InputSystemConfiguration([ new InputDefinition("abc", [], [])], [], false, 2), 
            new InputScheme("abc", controllerName!, "abc", false, []));

        // Assert
        Assert.Equal(InputValidationService.InputSchemeError, validationContext.ErrorCategory);
        Assert.True(validationContext.CheckErrorExists(InputValidationService.ValidationError_InvalidData));
    }

    [Fact]
    public void ValidateCustomInputScheme_InputSchemeControllerNameNotSupportedByConfiguration_AddsInvalidDataErrorToContext()
    {
        // Arrange/Act
        var validationContext = _service.ValidateCustomInputScheme(
            new InputSystemConfiguration([new InputDefinition("abc", [], [])], [], false, 2),
            new InputScheme("abc", "controller", "abc", false, []));

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
        var mockControllerConfiguration = new Mock<IInputControllerConfiguration>();
        mockControllerConfiguration.SetupGet(m => m.ControllerName)
            .Returns(new InputControllerName("controller"));

        // Act
        var validationContext = _service.ValidateCustomInputScheme(
            new InputSystemConfiguration([new InputDefinition("abc", [], [])], [ mockControllerConfiguration.Object ], false, 2),
            new InputScheme("abc", mockControllerConfiguration.Object.ControllerName.Name, schemeName!, false, []));

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
        var mockControllerConfiguration = new Mock<IInputControllerConfiguration>();
        mockControllerConfiguration.SetupGet(m => m.ControllerName)
            .Returns(new InputControllerName("controller"));

        // Act
        var validationContext = _service.ValidateCustomInputScheme(
            new InputSystemConfiguration([new InputDefinition("abc", [], [ new InputScheme("abc", "controller", schemeName1, false, []) ])], [ mockControllerConfiguration.Object ], false, 2),
            new InputScheme("abc", "controller", schemeName2, false, []));

        // Assert
        Assert.Equal(InputValidationService.InputSchemeError, validationContext.ErrorCategory);
        Assert.True(validationContext.CheckErrorExists(InputValidationService.ValidationError_DuplicateIdentifier));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void ValidateCustomInputScheme_InvalidInputControllerConfigurationActionMapInputKey_AddsInvalidDataErrorToContext(string? inputKey)
    {
        // Arrange
        var mockControllerConfiguration = new Mock<IInputControllerConfiguration>();
        mockControllerConfiguration.SetupGet(m => m.ControllerName)
            .Returns(new InputControllerName("controller"));

        // Act
        var validationContext = _service.ValidateCustomInputScheme(
            new InputSystemConfiguration([new InputDefinition("abc", [], [])], [ mockControllerConfiguration.Object ], false, 2),
            new InputScheme("abc", "controller", "abc", false, [new InputActionMap("actionKey", inputKey!, InputPhase.Start)]));

        // Assert
        Assert.Equal(InputValidationService.InputActionMapError, validationContext.ErrorCategory);
        Assert.True(validationContext.CheckErrorExists(InputValidationService.ValidationError_InvalidData));
    }

    [Theory]
    [InlineData("aBcD", "abcd")]
    [InlineData("efgh", "efgh")]
    [InlineData("IJKL", "IJKL")]
    public void ValidateCustomInputScheme_DuplicateInputControllerConfigurationActionMapInputKey_AddsInvalidDataErrorToContext(string inputKey1, string inputKey2)
    {
        // Arrange
        var mockControllerConfiguration = new Mock<IInputControllerConfiguration>();
        mockControllerConfiguration.SetupGet(m => m.ControllerName)
            .Returns(new InputControllerName("controller"));

        // Act
        var validationContext = _service.ValidateCustomInputScheme(
            new InputSystemConfiguration([new InputDefinition("abc", [], [])], [mockControllerConfiguration.Object], false, 2),
            new InputScheme("abc", "controller", "abc", false, [
                    new InputActionMap("actionKey", inputKey1, InputPhase.Start),
                    new InputActionMap("actionKey2", inputKey2, InputPhase.Start)
             ]));

        // Assert
        Assert.Equal(InputValidationService.InputActionMapError, validationContext.ErrorCategory);
        Assert.True(validationContext.CheckErrorExists(InputValidationService.ValidationError_InvalidData));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void ValidateCustomInputScheme_InvalidInputControllerConfigurationActionMapActionKey_AddsInvalidDataErrorToContext(string? actionKey)
    {
        // Arrange
        var mockControllerConfiguration = new Mock<IInputControllerConfiguration>();
        mockControllerConfiguration.SetupGet(m => m.ControllerName)
            .Returns(new InputControllerName("controller"));

        // Act
        var validationContext = _service.ValidateCustomInputScheme(
            new InputSystemConfiguration([new InputDefinition("abc", [], [])], [mockControllerConfiguration.Object], false, 2),
            new InputScheme("abc", "controller", "abc", false, [new InputActionMap(actionKey!, "inputKey", InputPhase.Start)]));

        // Assert
        Assert.Equal(InputValidationService.InputActionMapError, validationContext.ErrorCategory);
        Assert.True(validationContext.CheckErrorExists(InputValidationService.ValidationError_InvalidData));
    }

    [Theory]
    [InlineData("aBcD", "abcd")]
    [InlineData("efgh", "efgh")]
    [InlineData("IJKL", "IJKL")]
    public void ValidateCustomInputScheme_DuplicateInputControllerConfigurationActionMapActionKey_AddsInvalidDataErrorToContext(string actionKey1, string actionKey2)
    {        
        // Arrange
        var mockControllerConfiguration = new Mock<IInputControllerConfiguration>();
        mockControllerConfiguration.SetupGet(m => m.ControllerName)
            .Returns(new InputControllerName("controller"));

        // Act
        var validationContext = _service.ValidateCustomInputScheme(
            new InputSystemConfiguration([new InputDefinition("abc", [], [])], [mockControllerConfiguration.Object], false, 2),
            new InputScheme("abc", "controller", "abc", false, [
                    new InputActionMap(actionKey1, "abc", InputPhase.Start),
                    new InputActionMap(actionKey2, "def", InputPhase.Start)
             ]));

        // Assert
        Assert.Equal(InputValidationService.InputActionMapError, validationContext.ErrorCategory);
        Assert.True(validationContext.CheckErrorExists(InputValidationService.ValidationError_InvalidData));
    }

    [Fact]
    public void ValidateCustomInputScheme_SchemeMissingActionKeysInDefinition_AddsMissingDataErrorToContext()
    {
        // Arrange
        var mockControllerConfiguration = new Mock<IInputControllerConfiguration>();
        mockControllerConfiguration.SetupGet(m => m.ControllerName)
            .Returns(new InputControllerName("controller"));

        // Act
        var validationContext = _service.ValidateCustomInputScheme(
            new InputSystemConfiguration([new InputDefinition("abc", [new InputAction("abc", null), new InputAction("def", null)], [])], 
                [mockControllerConfiguration.Object], false, 2),
            new InputScheme("abc", "controller", "abc", false, [new InputActionMap("abc", "abc", InputPhase.Start)]));

        // Assert
        Assert.Equal(InputValidationService.InputSchemeError, validationContext.ErrorCategory);
        Assert.True(validationContext.CheckErrorExists(InputValidationService.ValidationError_CollectionMissingData));
    }

    [Fact]
    public void ValidateCustomInputScheme_Valid_ReturnsSuccessfully()
    {
        // Arrange
        var mockControllerConfiguration = new Mock<IInputControllerConfiguration>();
        mockControllerConfiguration.SetupGet(m => m.ControllerName)
            .Returns(new InputControllerName("controller"));

        // Act
        var validationContext = _service.ValidateCustomInputScheme(
            new InputSystemConfiguration([new InputDefinition("abc", [new InputAction("abc", null), new InputAction("def", null)], [])],
                [mockControllerConfiguration.Object], false, 2),
            new InputScheme("abc", "controller", "abc", false, 
            [ new InputActionMap("abc", "abc", InputPhase.Start), new InputActionMap("def", "def", InputPhase.Start)]));

        // Assert
        Assert.Empty(validationContext.Errors);
    }

    #endregion
}
