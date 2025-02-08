using OSK.Inputs.Internal;
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

    #region ValidateInputDefinition

    [Fact]
    public void ValidateInputDefinition_NullInputDefinition_ThrowsArgumentNullException()
    {
        // Arrange/Act/Assert
        Assert.Throws<ArgumentNullException>(() => _service.ValidateInputDefinition(null!));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void ValidateInputDefinition_InvalidInputDefinitionName_AddsMissingIdentifierToErrorContext(string? definitionName)
    {
        // Arrange/Act
        var validationContext = _service.ValidateInputDefinition(new InputDefinition(definitionName!, false, [], []));

        // Assert
        Assert.True(validationContext.CheckErrorExists(InputValidationService.InputDefinitionError, InputValidationService.ValidationError_MissingIdentifier));
    }

    [Fact]
    public void ValidateInputDefinition_EmptyInputActions_AddsMissingDataToErrorContext()
    {
        // Arrange/Act
        var validationContext = _service.ValidateInputDefinition(
            new InputDefinition("abc", false, [new InputControllerConfiguration("abc", [], [])], []));

        // Assert
        Assert.True(validationContext.CheckErrorExists(InputValidationService.InputDefinitionError, InputValidationService.ValidationError_MissingData));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void ValidateInputDefinition_InvalidInputActionKey_AddsInvalidDataToErrorContext(string? actionKey)
    {
        // Arrange/Act
        var validationContext = _service.ValidateInputDefinition(new InputDefinition("abc", false,
            [new InputControllerConfiguration("abc", [], [])],
            [new InputAction(actionKey!, null)]));

        // Assert
        Assert.True(validationContext.CheckErrorExists(InputValidationService.InputDefinitionError, InputValidationService.ValidationError_InvalidData));
    }

    [Theory]
    [InlineData("aBcD", "abcd")]
    [InlineData("efgh", "efgh")]
    [InlineData("IJKL", "IJKL")]
    public void ValidateInputDefinition_DuplicateInputActionKey_ThrowsArgumentException(string actionKey1, string actionKey2)
    {
        // Arrange/Act
        var validationContext = _service.ValidateInputDefinition(new InputDefinition("abc", false,
            [new InputControllerConfiguration("abc", [], [])],
            [new InputAction(actionKey1, null), new InputAction(actionKey2, null)]));
    }

    [Fact]
    public void ValidateInputDefinition_EmptyControllers_AddsMissingDataToErrorContext()
    {
        // Arrange/Act
        var validationContext = _service.ValidateInputDefinition(new InputDefinition("abc", false, [], [new InputAction()]));

        // Assert
        Assert.True(validationContext.CheckErrorExists(InputValidationService.InputDefinitionError, InputValidationService.ValidationError_MissingData2));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void ValidateInputDefinition_InvalidControllerName_AddsInvalidDataToErrorContext(string? controllerName)
    {
        // Arrange/Act
        var validationContext = _service.ValidateInputDefinition(new InputDefinition("abc", false,
            [new InputControllerConfiguration(controllerName!, [], [])],
            [new InputAction("abc", null)]));

        // Assert
        Assert.True(validationContext.CheckErrorExists(InputValidationService.InputDefinitionError, InputValidationService.ValidationError_InvalidData2));
    }

    [Theory]
    [InlineData("aBcD", "abcd")]
    [InlineData("efgh", "efgh")]
    [InlineData("IJKL", "IJKL")]
    public void ValidateInputDefinition_DuplicateControllerName_AddsInvalidDataToErrorContext(string controllerName1, string controllerName2)
    {
        // Arrange/Act
        var validationContext = _service.ValidateInputDefinition(new InputDefinition("abc", false,
            [new InputControllerConfiguration(controllerName1, [], []), new InputControllerConfiguration(controllerName2, [], [])],
            [new InputAction("abc", null)]));

        // Assert
        Assert.True(validationContext.CheckErrorExists(InputValidationService.InputDefinitionError, InputValidationService.ValidationError_InvalidData2));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void ValidateInputDefinition_InvalidControllerInputReceiverDescriptorName_AddsInvalidDataToErrorContext(string? descriptorName)
    {
        // Arrange/Act
        var validationContext = _service.ValidateInputDefinition(new InputDefinition("abc", false,
            [new InputControllerConfiguration("abc", [new InputReceiverDescriptor(descriptorName!, typeof(TestInputReceiver), _ => true)], [])],
            [new InputAction("abc", null)]));

        // Assert
        Assert.True(validationContext.CheckErrorExists(InputValidationService.InputControllerError, InputValidationService.ValidationError_InvalidData));
    }

    [Theory]
    [InlineData("aBcD", "abcd")]
    [InlineData("efgh", "efgh")]
    [InlineData("IJKL", "IJKL")]
    public void ValidateInputDefinition_DuplicateControllerInputReceiverName_AddsInvalidDataToErrorContext(string inputReceiver1, string inputReceiver2)
    {
        // Arrange/Act
        var validationContext = _service.ValidateInputDefinition(new InputDefinition("abc", false,
            [new InputControllerConfiguration("abc", [
                new InputReceiverDescriptor(inputReceiver1, typeof(TestInputReceiver), _ => true),
                new InputReceiverDescriptor(inputReceiver2, typeof(TestInputReceiver), _ => true)
               ], [])
            ],
            [new InputAction("abc", null)]));

        // Assert
        Assert.True(validationContext.CheckErrorExists(InputValidationService.InputControllerError, InputValidationService.ValidationError_InvalidData));
    }

    [Fact]
    public void ValidateInputDefinition_InvalidControllerInputReceiverType_DoesNotUseIInputReceiver_AddsInvalidDataToErrorContext()
    {
        // Arrange/Act
        var validationContext = _service.ValidateInputDefinition(new InputDefinition("abc", false,
            [new InputControllerConfiguration("abc", [
                new InputReceiverDescriptor("abc", typeof(TestSchemeRepository), _ => true)
               ], [])
            ],
            [new InputAction("abc", null)]));

        // Assert
        Assert.True(validationContext.CheckErrorExists(InputValidationService.InputReceiverDescriptorError, InputValidationService.ValidationError_InvalidData2));
    }

    [Fact]
    public void ValidateInputDefinition_InvalidControllerInputScheme_AddsInvalidDataToErrorContext()
    {
        // Arrange/Act
        var validationContext = _service.ValidateInputDefinition(new InputDefinition("abc", false,
            [
            new InputControllerConfiguration("abc", 
               [
                new InputReceiverDescriptor("abc", typeof(TestInputReceiver), _ => true)
               ], 
               // Invalid action key
               [ new InputScheme("", "abc", "abc", [], false) ])
            ],
            [new InputAction("abc", null)]));

        // Assert
        Assert.True(validationContext.CheckErrorExists(InputValidationService.InputReceiverError, InputValidationService.ValidationError_MissingData));
    }

    [Fact]
    public void ValidateInputDefinition_Valid_ReturnsSuccessfully()
    {
        // Arrange/Act
        var validationContext = _service.ValidateInputDefinition(new InputDefinition("abc", false,
            [
                new InputControllerConfiguration("abc",
                [
                    new InputReceiverDescriptor("Motion1", typeof(TestInputReceiver), _ => true),
                    new InputReceiverDescriptor("Motion2", typeof(TestInputReceiver), _ => true)
                ],
                [
                    new InputScheme("abc", "abc", "abc",
                        [
                            new InputReceiverConfiguration("Motion1",
                                [
                                    new ("Reload", "A", InputPhase.Start),
                                    new ("Fire", "B", InputPhase.Start),
                                    new ("Restart", "C", InputPhase.Start)
                                ])
                        ],
                        false)
                ])
            ],
            [
                new InputAction("Reload", "Reload the air power"),
                new InputAction("Fire", null),
                new InputAction("Restart", string.Empty)
            ]));

        // Assert
        Assert.Empty(validationContext.Errors);
    }

    #endregion

    #region ValidateInputScheme

    [Fact]
    public void ValidateInputScheme_NullInputDefinition_ThrowsArgumentNullException()
    {
        // Arrange/Act/Assert
        Assert.Throws<ArgumentNullException>(() => _service.ValidateInputScheme(null!, new InputScheme("abc", "abc", "abc", [], false)));
    }

    [Fact]
    public void ValidateInputScheme_NullInputScheme_ThrowsArgumentNullException()
    {
        // Arrange/Act/Assert
        Assert.Throws<ArgumentNullException>(() => _service.ValidateInputScheme(new InputDefinition("abc", false, [], []), null!));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void ValidateInputScheme_InvalidInputSchemeDefinitionName_AddsMissingDataToErrorContext(string? definitionName)
    {
        // Arrange/Act
        var validationContext = _service.ValidateInputScheme(new InputDefinition("abc", false, [], []), new InputScheme(definitionName!, "abc", "abc", [], false));

        // Assert
        Assert.True(validationContext.CheckErrorExists(InputValidationService.InputSchemeError, InputValidationService.ValidationError_MissingData));
    }

    [Fact]
    public void ValidateInputScheme_InputSchemeDefinitionNameDoesNotMatchInputDefinition_AddsMismatchedTenantToErrorContext()
    {
        // Arrange/Act
        var validationContext = _service.ValidateInputScheme(new InputDefinition("abc", false, [], []), new InputScheme("def", "abc", "abc", [], false));

        // Assert
        Assert.True(validationContext.CheckErrorExists(InputValidationService.InputSchemeError, InputValidationService.ValidationError_MismatchedTenant));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void ValidateInputScheme_InputSchemeMissingControllerName_AddsMissingDataToErrorContext(string? controllerName)
    {
        // Arrange/Act
        var validationContext = _service.ValidateInputScheme(new InputDefinition("abc", false, [], []), new InputScheme("abc", controllerName!, "abc", [], false));

        // Assert
        Assert.True(validationContext.CheckErrorExists(InputValidationService.InputSchemeError, InputValidationService.ValidationError_MissingData));
    }

    [Fact]
    public void ValidateInputScheme_InputSchemeControllerNameNotSupportedByInputDefinition_AddsInvalidDataToErrorContext()
    {
        // Arrange/Act
        var validationContext = _service.ValidateInputScheme(
            new InputDefinition("abc", false, [ new InputControllerConfiguration("amazing", [], []) ], []), 
            new InputScheme("abc", "controller", "abc", [], false));

        // Assert
        Assert.True(validationContext.CheckErrorExists(InputValidationService.InputSchemeError, InputValidationService.ValidationError_InvalidData));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void ValidateInputScheme_InputSchemeInvalidSchemeName_AddsMissingIdentifierToErrorContext(string? schemeName)
    {
        // Arrange/Act
        var validationContext = _service.ValidateInputScheme(
            new InputDefinition("abc", false, [new InputControllerConfiguration("controller", [], [])], []),
            new InputScheme("abc", "controller", schemeName!, [], false));

        // Assert
        Assert.True(validationContext.CheckErrorExists(InputValidationService.InputSchemeError, InputValidationService.ValidationError_MissingIdentifier));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void ValidateInputScheme_InvalidControllerInputReceiverConfigurationName_AddsInvalidDataToErrorContext(string? configurationName)
    {
        // Arrange/Act
        var validationContext = _service.ValidateInputScheme(
            new InputDefinition("abc", false, [new InputControllerConfiguration("controller", [], [])], []),
            new InputScheme("abc", "controller", "abc", [new InputReceiverConfiguration(configurationName!, [])], false));

        // Assert
        Assert.True(validationContext.CheckErrorExists(InputValidationService.InputReceiverError, InputValidationService.ValidationError_InvalidData));
    }

    [Theory]
    [InlineData("aBcD", "abcd")]
    [InlineData("efgh", "efgh")]
    [InlineData("IJKL", "IJKL")]
    public void ValidateInputScheme_DuplicateControllerInputReceiverConfigurationName_AddsInvalidDataToErrorContext(string inputReceiver1, string inputReceiver2)
    {
        // Arrange/Act
        var validationContext = _service.ValidateInputScheme(
            new InputDefinition("abc", false, [new InputControllerConfiguration("controller", [], [])], []),
            new InputScheme("abc", "controller", "abc",
            [
                new InputReceiverConfiguration(inputReceiver1, []),
                new InputReceiverConfiguration(inputReceiver2, [])
            ], false));

        // Assert
        Assert.True(validationContext.CheckErrorExists(InputValidationService.InputReceiverError, InputValidationService.ValidationError_InvalidData));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void ValidateInputScheme_InvalidControllerInputReceiverConfigurationActionMapInputKey_AddsInvalidDataToErrorContext(string? inputKey)
    {
        // Arrange/Act
        var validationContext = _service.ValidateInputScheme(
            new InputDefinition("abc", false, [new InputControllerConfiguration("controller", [], [])], []),
            new InputScheme("abc", "controller", "abc", 
            [
                new InputReceiverConfiguration("Mouse", [ new InputActionMap("actionKey", inputKey!, InputPhase.Start) ])
            ], false));

        // Assert
        Assert.True(validationContext.CheckErrorExists(InputValidationService.InputActionMapError, InputValidationService.ValidationError_InvalidData));
    }

    [Theory]
    [InlineData("aBcD", "abcd")]
    [InlineData("efgh", "efgh")]
    [InlineData("IJKL", "IJKL")]
    public void ValidateInputScheme_DuplicateControllerInputReceiverConfigurationActionMapInputKey_AddsInvalidDataToErrorContext(string inputKey1, string inputKey2)
    {
        // Arrange/Act
        var validationContext = _service.ValidateInputScheme(
            new InputDefinition("abc", false, [new InputControllerConfiguration("controller", [], [])], []),
            new InputScheme("abc", "controller", "abc",
            [
                new InputReceiverConfiguration("Mouse", 
                [ 
                    new InputActionMap("actionKey", inputKey1, InputPhase.Start), 
                    new InputActionMap("actionKey2", inputKey2, InputPhase.Start) 
                ])
            ], false));

        // Assert
        Assert.True(validationContext.CheckErrorExists(InputValidationService.InputActionMapError, InputValidationService.ValidationError_InvalidData));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void ValidateInputScheme_InvalidControllerInputReceiverConfigurationActionMapActionKey_AddsInvalidDataToErrorContext(string? actionKey)
    {
        // Arrange/Act
        var validationContext = _service.ValidateInputScheme(
            new InputDefinition("abc", false, [new InputControllerConfiguration("controller", [], [])], []),
            new InputScheme("abc", "controller", "abc",
            [
                new InputReceiverConfiguration("Mouse", [ new InputActionMap(actionKey!, "inputKey", InputPhase.Start) ])
            ], false));

        // Assert
        Assert.True(validationContext.CheckErrorExists(InputValidationService.InputActionMapError, InputValidationService.ValidationError_InvalidData2));
    }

    [Theory]
    [InlineData("aBcD", "abcd")]
    [InlineData("efgh", "efgh")]
    [InlineData("IJKL", "IJKL")]
    public void ValidateInputScheme_DuplicateControllerInputReceiverConfigurationActionMapActionKey_AddsInvalidDataToErrorContext(string actionKey1, string actionKey2)
    {
        // Arrange/Act
        var validationContext = _service.ValidateInputScheme(
            new InputDefinition("abc", false, [new InputControllerConfiguration("controller", [], [])], []),
            new InputScheme("abc", "controller", "abc",
            [
                new InputReceiverConfiguration("Mouse",
                [
                    new InputActionMap(actionKey1, "abc", InputPhase.Start),
                    new InputActionMap(actionKey2, "def", InputPhase.Start)
                ])
            ], false));

        // Assert
        Assert.True(validationContext.CheckErrorExists(InputValidationService.InputActionMapError, InputValidationService.ValidationError_InvalidData2));
    }

    [Fact]
    public void ValidateInputScheme_SchemeMissingActionKeysInDefinition_AddsMissingDataToErrorContext()
    {
        // Arrange/Act
        var validationContext = _service.ValidateInputScheme(
            new InputDefinition("abc", false, [new InputControllerConfiguration("controller", [], [])], 
                [new InputAction("abc", null), new InputAction("def", null)]),
            new InputScheme("abc", "controller", "abc",
            [
                new InputReceiverConfiguration("Mouse",
                [
                    new InputActionMap("abc", "abc", InputPhase.Start)
                ])
            ], false));

        // Assert
        Assert.True(validationContext.CheckErrorExists(InputValidationService.InputReceiverError, InputValidationService.ValidationError_MissingData));
    }

    [Fact]
    public void ValidateInputScheme_Valid_ReturnsSuccessfully()
    {
        // Arrange/Act
        var validationContext = _service.ValidateInputScheme(
            new InputDefinition("abc", false, [new InputControllerConfiguration("controller", [], [])],
                [new InputAction("abc", null), new InputAction("def", null)]),
            new InputScheme("abc", "controller", "abc",
            [
                new InputReceiverConfiguration("Mouse",
                [
                    new InputActionMap("abc", "abc", InputPhase.Start)
                ]),
                new InputReceiverConfiguration("Keyboard",
                [
                    new InputActionMap("def", "def", InputPhase.Start)
                ])
            ], false));

        // Assert
        Assert.Empty(validationContext.Errors);
    }

    #endregion
}
