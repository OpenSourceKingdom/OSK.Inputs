using OSK.Inputs.Abstractions.Configuration;
using OSK.Inputs.Abstractions.Inputs;
using OSK.Inputs.Internal.Services;
using OSK.Inputs.Models;
using OSK.Inputs.UnitTests._Helpers;
using Xunit;

namespace OSK.Inputs.UnitTests.Internal.Services;

public class InputSystemConfigurationValidatorTests
{
    #region Validate

    [Fact]
    public void Validate_NullConfiguration_ThrowsArgumentNullException()
    {
        // Arrange
        var validator = new InputSystemConfigurationValidator();

        // Act/Assert
        Assert.Throws<ArgumentNullException>(() => validator.Validate(null!));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Validate_EmptyDefinitions_ReturnsMissingData(bool useEmpty)
    {
        // Arrange
        var validator = new InputSystemConfigurationValidator();
        var configuration = new InputSystemConfiguration([], useEmpty ? [] : null!, new(), new());

        // Act
        var validation = validator.Validate(configuration);

        // Assert
        Assert.False(validation.IsValid);
        Assert.Equal(InputConfigurationType.InputSystem, validation.ConfigurationType);
        Assert.Equal(nameof(InputSystemConfiguration.Definitions), validation.TargetName);
        Assert.Equal(InputConfigurationValidation.MissingData, validation.Result);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Validate_InvalidDefinitionNames_ReturnsMissingData(string? name)
    {
        // Arrange
        var validator = new InputSystemConfigurationValidator();
        var configuration = new InputSystemConfiguration([], 
            [
                new InputDefinition(name!, [], [], false)
            ], new(), new());

        // Act
        var validation = validator.Validate(configuration);

        // Assert
        Assert.False(validation.IsValid);
        
        // Expected type and name changes because when null, the current constructor for the configuration will filter out null values,
        // resulting in a slightly different validation
        Assert.Equal(name is null ? InputConfigurationType.InputSystem : InputConfigurationType.Definition, validation.ConfigurationType);
        Assert.Equal(name is null ? nameof(InputSystemConfiguration.Definitions) : nameof(InputDefinition.Name), validation.TargetName);
        Assert.Equal(InputConfigurationValidation.MissingData, validation.Result);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Validate_DefinitionsWithoutSchemes_ReturnsMissingData(bool useEmpty)
    {
        // Arrange
        var validator = new InputSystemConfigurationValidator();
        var configuration = new InputSystemConfiguration([],
            [
                new InputDefinition("abc", useEmpty ? [] : null!, [], false)
            ], new(), new());

        // Act
        var validation = validator.Validate(configuration);

        // Assert
        Assert.False(validation.IsValid);
        Assert.Equal(InputConfigurationType.Definition, validation.ConfigurationType);
        Assert.Equal(nameof(InputDefinition.Schemes), validation.TargetName);
        Assert.Equal(InputConfigurationValidation.MissingData, validation.Result);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Validate_DefinitionsWithoutActions_ReturnsMissingData(bool useEmpty)
    {
        // Arrange
        var validator = new InputSystemConfigurationValidator();
        var configuration = new InputSystemConfiguration([],
            [
                new InputDefinition("abc",
                    useEmpty ? [] : null!,
                    [
                        new InputScheme("Abc", [], false, false)
                    ], false)
            ], new(), new());

        // Act
        var validation = validator.Validate(configuration);

        // Assert
        Assert.False(validation.IsValid);
        Assert.Equal(InputConfigurationType.Definition, validation.ConfigurationType);
        Assert.Equal(nameof(InputDefinition.Actions), validation.TargetName);
        Assert.Equal(InputConfigurationValidation.MissingData, validation.Result);
    }

    [Theory]
    [InlineData(null!)]
    [InlineData("")]
    [InlineData("  ")]
    public void Validate_InputActionWithInvalidNames_ReturnsMissingData(string? name)
    {
        // Arrange
        var validator = new InputSystemConfigurationValidator();
        var configuration = new InputSystemConfiguration([],
            [
                new InputDefinition("abc",
                    [
                        new InputAction(name!, new HashSet<InputPhase>(), _ => { })
                    ],
                    [
                        new InputScheme("Abc", [], false, false)
                    ], false)
            ], new(), new());

        // Act
        var validation = validator.Validate(configuration);

        // Assert
        Assert.False(validation.IsValid);
        Assert.Equal(InputConfigurationType.Definition, validation.ConfigurationType);
        Assert.Equal(nameof(InputDefinition.Actions), validation.TargetName);
        Assert.Equal(InputConfigurationValidation.MissingData, validation.Result);
    }

    [Fact]
    public void Validate_InputActionWithoutTriggers_ReturnsMissingData()
    {
        // Arrange
        var validator = new InputSystemConfigurationValidator();
        var configuration = new InputSystemConfiguration([],
            [
                new InputDefinition("abc",
                    [
                        new InputAction("Abc", new HashSet<InputPhase>(), _ => { })
                    ],
                    [
                        new InputScheme("Abc", [], false, false)
                    ], false)
            ], new(), new());

        // Act
        var validation = validator.Validate(configuration);

        // Assert
        Assert.False(validation.IsValid);
        Assert.Equal(InputConfigurationType.InputAction, validation.ConfigurationType);
        Assert.Equal(nameof(InputAction.TriggerPhases), validation.TargetName);
        Assert.Equal(InputConfigurationValidation.MissingData, validation.Result);
    }

    [Fact]
    public void Validate_InputActionWithoutExecutor_ReturnsMissingData()
    {
        // Arrange
        var validator = new InputSystemConfigurationValidator();
        var configuration = new InputSystemConfiguration([],
            [
                new InputDefinition("abc",
                    [
                        new InputAction("Abc", new HashSet<InputPhase>() { InputPhase.Start }, null!)
                    ],
                    [
                        new InputScheme("Abc", [], false, false)
                    ], false)
            ], new(), new());

        // Act
        var validation = validator.Validate(configuration);

        // Assert
        Assert.False(validation.IsValid);
        Assert.Equal(InputConfigurationType.InputAction, validation.ConfigurationType);
        Assert.Equal(nameof(InputAction.ActionExecutor), validation.TargetName);
        Assert.Equal(InputConfigurationValidation.MissingData, validation.Result);
    }

    [Theory]
    [InlineData(null!)]
    [InlineData("")]
    [InlineData("  ")]
    public void Validate_InputSchemesWithInvalidNames_ReturnsMissingData(string? name)
    {
        // Arrange
        var validator = new InputSystemConfigurationValidator();
        var configuration = new InputSystemConfiguration([],
            [
                new InputDefinition("abc",
                    [
                        new InputAction("Abc", new HashSet<InputPhase>() { InputPhase.Start }, _ => { })
                    ],
                    [
                        new InputScheme(name!, [], false, false)
                    ], false)
            ], new(), new());

        // Act
        var validation = validator.Validate(configuration);

        // Assert
        Assert.False(validation.IsValid);

        // Expected type and name changes because when null, the current constructor for the configuration will filter out null values,
        // resulting in a slightly different validation
        Assert.Equal(name is null ? InputConfigurationType.Definition : InputConfigurationType.Scheme, validation.ConfigurationType);
        Assert.Equal(name is null ? nameof(InputDefinition.Schemes) : nameof(InputScheme.Name), validation.TargetName);
        Assert.Equal(InputConfigurationValidation.MissingData, validation.Result);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Validate_InputSchemesMissingDeviceMaps_ReturnsMissingData(bool isEmpty)
    {
        // Arrange
        var validator = new InputSystemConfigurationValidator();
        var configuration = new InputSystemConfiguration([],
            [
                new InputDefinition("abc",
                    [
                        new InputAction("Abc", new HashSet<InputPhase>() { InputPhase.Start }, _ => { })
                    ],
                    [
                        new InputScheme("Abc", isEmpty ? [] : null!, false, false)
                    ], false)
            ], new(), new());

        // Act
        var validation = validator.Validate(configuration);

        // Assert
        Assert.False(validation.IsValid);

        Assert.Equal(InputConfigurationType.Scheme, validation.ConfigurationType);
        Assert.Equal(nameof(InputScheme.DeviceMaps), validation.TargetName);
        Assert.Equal(InputConfigurationValidation.MissingData, validation.Result);
    }

    [Fact]
    public void Validate_InputSchemeDeviceMapsMissingInputMaps_ReturnsMissingData()
    {
        // Arrange
        var validator = new InputSystemConfigurationValidator();
        var configuration = new InputSystemConfiguration([],
            [
                new InputDefinition("abc",
                    [
                        new InputAction("Abc", new HashSet<InputPhase>() { InputPhase.Start }, _ => { })
                    ],
                    [
                        new InputScheme("Abc", 
                            [
                                new DeviceInputMap() { DeviceIdentity = TestIdentity.Identity1, InputMaps = [] }
                            ], false, false)
                    ], false)
            ], new(), new());

        // Act
        var validation = validator.Validate(configuration);

        // Assert
        Assert.False(validation.IsValid);

        Assert.Equal(InputConfigurationType.Scheme, validation.ConfigurationType);
        Assert.Equal(nameof(InputScheme.DeviceMaps), validation.TargetName);
        Assert.Equal(InputConfigurationValidation.MissingData, validation.Result);
    }

    [Fact]
    public void Validate_DeviceMapsDeviceIdentityIsNotInConfigurationDevices_ReturnsInvalidData()
    {
        // Arrange
        var validator = new InputSystemConfigurationValidator();
        var configuration = new InputSystemConfiguration([],
            [
                new InputDefinition("abc",
                    [
                        new InputAction("Abc", new HashSet<InputPhase>() { InputPhase.Start }, _ => { })
                    ],
                    [
                        new InputScheme("Abc",
                            [
                                new DeviceInputMap() { DeviceIdentity = TestIdentity.Identity1, InputMaps = [ new InputMap() { ActionName = "Abc", InputId = 1} ] }
                            ], false, false)
                    ], false)
            ], new(), new());

        // Act
        var validation = validator.Validate(configuration);

        // Assert
        Assert.False(validation.IsValid);

        Assert.Equal(InputConfigurationType.DeviceMap, validation.ConfigurationType);
        Assert.Equal(nameof(DeviceInputMap.DeviceIdentity), validation.TargetName);
        Assert.Equal(InputConfigurationValidation.InvalidData, validation.Result);
    }

    [Fact]
    public void Validate_DeviceMapInputNotInDeviceSpecification_ReturnsInvalidData()
    {
        // Arrange
        var validator = new InputSystemConfigurationValidator();
        var configuration = new InputSystemConfiguration([new TestDeviceSpecification(TestIdentity.Identity1, new TestPhysicalInput(2))],
            [
                new InputDefinition("abc",
                    [
                        new InputAction("Abc", new HashSet<InputPhase>() { InputPhase.Start }, _ => { })
                    ],
                    [
                        new InputScheme("Abc",
                            [
                                new DeviceInputMap() { DeviceIdentity = TestIdentity.Identity1, InputMaps = [ new InputMap() { ActionName = "Abc", InputId = 1} ] }
                            ], false, false)
                    ], false)
            ], new(), new());

        // Act
        var validation = validator.Validate(configuration);

        // Assert
        Assert.False(validation.IsValid);

        Assert.Equal(InputConfigurationType.DeviceMap, validation.ConfigurationType);
        Assert.Equal(nameof(DeviceInputMap.InputMaps), validation.TargetName);
        Assert.Equal(InputConfigurationValidation.InvalidData, validation.Result);
    }

    [Fact]
    public void Validate_DeviceMapInputMapsWithDuplicateInputIds_ReturnsDuplicateData()
    {
        // Arrange
        var validator = new InputSystemConfigurationValidator();
        var configuration = new InputSystemConfiguration([new TestDeviceSpecification(TestIdentity.Identity1, new TestPhysicalInput(2))],
            [
                new InputDefinition("abc",
                    [
                        new InputAction("Abc", new HashSet<InputPhase>() { InputPhase.Start }, _ => { })
                    ],
                    [
                        new InputScheme("Abc",
                            [
                                new DeviceInputMap() { DeviceIdentity = TestIdentity.Identity1, 
                                    InputMaps = [ new InputMap() { ActionName = "Abc", InputId = 2 }, new InputMap() { ActionName = "Def", InputId = 2 }] }
                            ], false, false)
                    ], false)
            ], new(), new());

        // Act
        var validation = validator.Validate(configuration);

        // Assert
        Assert.False(validation.IsValid);

        Assert.Equal(InputConfigurationType.DeviceMap, validation.ConfigurationType);
        Assert.Equal(nameof(DeviceInputMap.InputMaps), validation.TargetName);
        Assert.Equal(InputConfigurationValidation.InvalidData, validation.Result);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Validate_DeviceMapInputMapsWithInvalidActionNames_ReturnsMissingData(string? actionName)
    {
        // Arrange
        var validator = new InputSystemConfigurationValidator();
        var configuration = new InputSystemConfiguration([new TestDeviceSpecification(TestIdentity.Identity1, new TestPhysicalInput(1))],
            [
                new InputDefinition("abc",
                    [
                        new InputAction("Abc", new HashSet<InputPhase>() { InputPhase.Start }, _ => { })
                    ],
                    [
                        new InputScheme("Abc",
                            [
                                new DeviceInputMap() { DeviceIdentity = TestIdentity.Identity1,
                                    InputMaps = [ new InputMap() { ActionName = actionName!, InputId = 1 }] }
                            ], false, false)
                    ], false)
            ], new(), new());

        // Act
        var validation = validator.Validate(configuration);

        // Assert
        Assert.False(validation.IsValid);

        Assert.Equal(InputConfigurationType.DeviceMap, validation.ConfigurationType);
        Assert.Equal(nameof(DeviceInputMap.InputMaps), validation.TargetName);
        Assert.Equal(InputConfigurationValidation.MissingData, validation.Result);
    }

    [Fact]
    public void Validate_DeviceMapInputMapsWithActionNameNotInInputDefinition_ReturnsInvalidData()
    {
        // Arrange
        var validator = new InputSystemConfigurationValidator();
        var configuration = new InputSystemConfiguration([new TestDeviceSpecification(TestIdentity.Identity1, new TestPhysicalInput(1))],
            [
                new InputDefinition("abc",
                    [
                        new InputAction("Abc", new HashSet<InputPhase>() { InputPhase.Start }, _ => { })
                    ],
                    [
                        new InputScheme("Abc",
                            [
                                new DeviceInputMap() { DeviceIdentity = TestIdentity.Identity1,
                                    InputMaps = [ new InputMap() { ActionName = "Def", InputId = 1 }] }
                            ], false, false)
                    ], false)
            ], new(), new());

        // Act
        var validation = validator.Validate(configuration);

        // Assert
        Assert.False(validation.IsValid);

        Assert.Equal(InputConfigurationType.DeviceMap, validation.ConfigurationType);
        Assert.Equal(nameof(DeviceInputMap.InputMaps), validation.TargetName);
        Assert.Equal(InputConfigurationValidation.InvalidData, validation.Result);
    }

    [Fact]
    public void Validate_DeviceMapInputMapsWithDuplicateActionNames_ReturnsDuplicateData()
    {
        // Arrange
        var validator = new InputSystemConfigurationValidator();
        var configuration = new InputSystemConfiguration(
            [
                new TestDeviceSpecification(TestIdentity.Identity1, 
                    new TestPhysicalInput(1), new TestPhysicalInput(2))
            ],
            [
                new InputDefinition("abc",
                    [
                        new InputAction("Abc", new HashSet<InputPhase>() { InputPhase.Start }, _ => { })
                    ],
                    [
                        new InputScheme("Abc",
                            [
                                new DeviceInputMap() { DeviceIdentity = TestIdentity.Identity1,
                                    InputMaps = [ new InputMap() { ActionName = "Abc", InputId = 1 }, new InputMap() { ActionName = "Abc", InputId = 2 }] }
                            ], false, false)
                    ], false)
            ], new(), new());

        // Act
        var validation = validator.Validate(configuration);

        // Assert
        Assert.False(validation.IsValid);

        Assert.Equal(InputConfigurationType.DeviceMap, validation.ConfigurationType);
        Assert.Equal(nameof(DeviceInputMap.InputMaps), validation.TargetName);
        Assert.Equal(InputConfigurationValidation.DuplicateData, validation.Result);
    }

    [Fact]
    public void Validate_InputSchemeWithDuplicateActionNamesAcrossDevices_ReturnsDuplicateData()
    {
        // Arrange
        var validator = new InputSystemConfigurationValidator();
        var configuration = new InputSystemConfiguration(
            [
                new TestDeviceSpecification(TestIdentity.Identity1,
                    new TestPhysicalInput(1), new TestPhysicalInput(2)),
                new TestDeviceSpecification(TestIdentity.Identity2,
                    new TestPhysicalInput(1))
            ],
            [
                new InputDefinition("abc",
                    [
                        new InputAction("Abc", new HashSet<InputPhase>() { InputPhase.Start }, _ => { })
                    ],
                    [
                        new InputScheme("Abc",
                            [
                                new DeviceInputMap() { DeviceIdentity = TestIdentity.Identity1,
                                    InputMaps = [ new InputMap() { ActionName = "Abc", InputId = 1 }] },
                                new DeviceInputMap() { DeviceIdentity = TestIdentity.Identity2,
                                    InputMaps = [ new InputMap() { ActionName = "Abc", InputId = 1 }] }
                            ], false, false)
                    ], false)
            ], new(), new());

        // Act
        var validation = validator.Validate(configuration);

        // Assert
        Assert.False(validation.IsValid);

        Assert.Equal(InputConfigurationType.Scheme, validation.ConfigurationType);
        Assert.Equal(nameof(InputScheme.DeviceMaps), validation.TargetName);
        Assert.Equal(InputConfigurationValidation.DuplicateData, validation.Result);
    }

    [Fact]
    public void Validate_NullInputProcessConfiguration_ReturnsMissingData()
    {
        // Arrange
        var validator = new InputSystemConfigurationValidator();
        var configuration = new InputSystemConfiguration(
            [
                new TestDeviceSpecification(TestIdentity.Identity1,
                    new TestPhysicalInput(1), new TestPhysicalInput(2)),
                new TestDeviceSpecification(TestIdentity.Identity2,
                    new TestPhysicalInput(1))
            ],
            [
                new InputDefinition("abc",
                    [
                        new InputAction("Abc", new HashSet<InputPhase>() { InputPhase.Start }, _ => { }),
                        new InputAction("Def", new HashSet<InputPhase>() { InputPhase.Start }, _ => { })
                    ],
                    [
                        new InputScheme("Abc",
                            [
                                new DeviceInputMap() { DeviceIdentity = TestIdentity.Identity1,
                                    InputMaps = [ new InputMap() { ActionName = "Abc", InputId = 1 }] },
                                new DeviceInputMap() { DeviceIdentity = TestIdentity.Identity2,
                                    InputMaps = [ new InputMap() { ActionName = "Def", InputId = 1 }] }
                            ], false, false)
                    ], false)
            ], null!, new());

        // Act
        var validation = validator.Validate(configuration);

        // Assert
        Assert.False(validation.IsValid);
        Assert.Equal(InputConfigurationType.InputSystem, validation.ConfigurationType);
        Assert.Equal(nameof(InputSystemConfiguration.ProcessorConfiguration), validation.TargetName);
        Assert.Equal(InputConfigurationValidation.MissingData, validation.Result);
    }

    [Fact]
    public void Validate_InputProcessConfigurationTapDelayLessThan0_ReturnsInvalidData()
    {
        // Arrange
        var validator = new InputSystemConfigurationValidator();
        var configuration = new InputSystemConfiguration(
            [
                new TestDeviceSpecification(TestIdentity.Identity1,
                    new TestPhysicalInput(1), new TestPhysicalInput(2)),
                new TestDeviceSpecification(TestIdentity.Identity2,
                    new TestPhysicalInput(1))
            ],
            [
                new InputDefinition("abc",
                    [
                        new InputAction("Abc", new HashSet<InputPhase>() { InputPhase.Start }, _ => { }),
                        new InputAction("Def", new HashSet<InputPhase>() { InputPhase.Start }, _ => { })
                    ],
                    [
                        new InputScheme("Abc",
                            [
                                new DeviceInputMap() { DeviceIdentity = TestIdentity.Identity1,
                                    InputMaps = [ new InputMap() { ActionName = "Abc", InputId = 1 }] },
                                new DeviceInputMap() { DeviceIdentity = TestIdentity.Identity2,
                                    InputMaps = [ new InputMap() { ActionName = "Def", InputId = 1 }] }
                            ], false, false)
                    ], false)
            ], new() { TapReactivationTime = TimeSpan.FromSeconds(-1) }, new());

        // Act
        var validation = validator.Validate(configuration);

        // Assert
        Assert.False(validation.IsValid);
        Assert.Equal(InputConfigurationType.InputProcessor, validation.ConfigurationType);
        Assert.Equal(nameof(InputProcessorConfiguration.TapReactivationTime), validation.TargetName);
        Assert.Equal(InputConfigurationValidation.InvalidData, validation.Result);
    }

    [Fact]
    public void Validate_InputProcessConfigurationStartPhaseBeforeActiveLessThan0_ReturnsInvalidData()
    {
        // Arrange
        var validator = new InputSystemConfigurationValidator();
        var configuration = new InputSystemConfiguration(
            [
                new TestDeviceSpecification(TestIdentity.Identity1,
                    new TestPhysicalInput(1), new TestPhysicalInput(2)),
                new TestDeviceSpecification(TestIdentity.Identity2,
                    new TestPhysicalInput(1))
            ],
            [
                new InputDefinition("abc",
                    [
                        new InputAction("Abc", new HashSet<InputPhase>() { InputPhase.Start }, _ => { }),
                        new InputAction("Def", new HashSet<InputPhase>() { InputPhase.Start }, _ => { })
                    ],
                    [
                        new InputScheme("Abc",
                            [
                                new DeviceInputMap() { DeviceIdentity = TestIdentity.Identity1,
                                    InputMaps = [ new InputMap() { ActionName = "Abc", InputId = 1 }] },
                                new DeviceInputMap() { DeviceIdentity = TestIdentity.Identity2,
                                    InputMaps = [ new InputMap() { ActionName = "Def", InputId = 1 }] }
                            ], false, false)
                    ], false)
            ], new() { TapReactivationTime = TimeSpan.FromSeconds(1), ActiveTimeThreshold = TimeSpan.FromSeconds(-1) }, new());

        // Act
        var validation = validator.Validate(configuration);

        // Assert
        Assert.False(validation.IsValid);
        Assert.Equal(InputConfigurationType.InputProcessor, validation.ConfigurationType);
        Assert.Equal(nameof(InputProcessorConfiguration.ActiveTimeThreshold), validation.TargetName);
        Assert.Equal(InputConfigurationValidation.InvalidData, validation.Result);
    }

    [Fact]
    public void Validate_NullJoinPolicy_ReturnsMissingData()
    {
        // Arrange
        var validator = new InputSystemConfigurationValidator();
        var configuration = new InputSystemConfiguration(
            [
                new TestDeviceSpecification(TestIdentity.Identity1,
                    new TestPhysicalInput(1), new TestPhysicalInput(2)),
                new TestDeviceSpecification(TestIdentity.Identity2,
                    new TestPhysicalInput(1))
            ],
            [
                new InputDefinition("abc",
                    [
                        new InputAction("Abc", new HashSet<InputPhase>() { InputPhase.Start }, _ => { }),
                        new InputAction("Def", new HashSet<InputPhase>() { InputPhase.Start }, _ => { })
                    ],
                    [
                        new InputScheme("Abc",
                            [
                                new DeviceInputMap() { DeviceIdentity = TestIdentity.Identity1,
                                    InputMaps = [ new InputMap() { ActionName = "Abc", InputId = 1 }] },
                                new DeviceInputMap() { DeviceIdentity = TestIdentity.Identity2,
                                    InputMaps = [ new InputMap() { ActionName = "Def", InputId = 1 }] }
                            ], false, false)
                    ], false)
            ], new() { TapReactivationTime = TimeSpan.FromSeconds(1), ActiveTimeThreshold = TimeSpan.FromSeconds(1) }, null!);

        // Act
        var validation = validator.Validate(configuration);

        // Assert
        Assert.False(validation.IsValid);
        Assert.Equal(InputConfigurationType.InputSystem, validation.ConfigurationType);
        Assert.Equal(nameof(InputSystemConfiguration.JoinPolicy), validation.TargetName);
        Assert.Equal(InputConfigurationValidation.MissingData, validation.Result);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_JoinPolicyHasUsersAtOrLessThan0_ReturnsInvalidData(int userCount)
    {
        // Arrange
        var validator = new InputSystemConfigurationValidator();
        var configuration = new InputSystemConfiguration(
            [
                new TestDeviceSpecification(TestIdentity.Identity1,
                    new TestPhysicalInput(1), new TestPhysicalInput(2)),
                new TestDeviceSpecification(TestIdentity.Identity2,
                    new TestPhysicalInput(1))
            ],
            [
                new InputDefinition("abc",
                    [
                        new InputAction("Abc", new HashSet<InputPhase>() { InputPhase.Start }, _ => { }),
                        new InputAction("Def", new HashSet<InputPhase>() { InputPhase.Start }, _ => { })
                    ],
                    [
                        new InputScheme("Abc",
                            [
                                new DeviceInputMap() { DeviceIdentity = TestIdentity.Identity1,
                                    InputMaps = [ new InputMap() { ActionName = "Abc", InputId = 1 }] },
                                new DeviceInputMap() { DeviceIdentity = TestIdentity.Identity2,
                                    InputMaps = [ new InputMap() { ActionName = "Def", InputId = 1 }] }
                            ], false, false)
                    ], false)
            ], new() { TapReactivationTime = TimeSpan.FromSeconds(1), ActiveTimeThreshold = TimeSpan.FromSeconds(1) }, 
            new() { MaxUsers = userCount });

        // Act
        var validation = validator.Validate(configuration);

        // Assert
        Assert.False(validation.IsValid);
        Assert.Equal(InputConfigurationType.JoinPolicy, validation.ConfigurationType);
        Assert.Equal(nameof(InputSystemJoinPolicy.MaxUsers), validation.TargetName);
        Assert.Equal(InputConfigurationValidation.InvalidData, validation.Result);
    }

    [Fact]
    public void Validate_Valid_ReturnsSuccess()
    {
        // Arrange
        var validator = new InputSystemConfigurationValidator();
        var configuration = new InputSystemConfiguration(
            [
                new TestDeviceSpecification(TestIdentity.Identity1,
                    new TestPhysicalInput(1), new TestPhysicalInput(2)),
                new TestDeviceSpecification(TestIdentity.Identity2,
                    new TestPhysicalInput(1))
            ],
            [
                new InputDefinition("abc",
                    [
                        new InputAction("Abc", new HashSet<InputPhase>() { InputPhase.Start }, _ => { }),
                        new InputAction("Def", new HashSet<InputPhase>() { InputPhase.Start }, _ => { })
                    ],
                    [
                        new InputScheme("Abc",
                            [
                                new DeviceInputMap() { DeviceIdentity = TestIdentity.Identity1,
                                    InputMaps = [ new InputMap() { ActionName = "Abc", InputId = 1 }] },
                                new DeviceInputMap() { DeviceIdentity = TestIdentity.Identity2,
                                    InputMaps = [ new InputMap() { ActionName = "Def", InputId = 1 }] }
                            ], false, false)
                    ], false)
            ], new(), new() { MaxUsers = 1 });

        // Act
        var validation = validator.Validate(configuration);

        // Assert
        Assert.True(validation.IsValid);
        Assert.Equal(InputConfigurationValidation.Ok, validation.Result);
    }

    #endregion

    #region ValidateCustomScheme

    [Fact]
    public void ValidateCustomScheme_NullConfiguration_ThrowsArgumentNullException()
    {
        // Arrange
        var validator = new InputSystemConfigurationValidator();

        // Act/Assert
        Assert.Throws<ArgumentNullException>(() => validator.ValidateCustomScheme(null!, new CustomInputScheme()
        {
            DefinitionName = "Abc",
            Name = "Name",
            DeviceMaps = []
        }, false));
    }

    [Fact]
    public void ValidateCustomScheme_NullScheme_ThrowsArgumentNullException()
    {
        // Arrange
        var validator = new InputSystemConfigurationValidator();

        // Act/Assert
        Assert.Throws<ArgumentNullException>(() => validator.ValidateCustomScheme(new InputSystemConfiguration([], [], new(), new()), null!, false));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void ValidateCustomScheme_InvalidDefinitionName_ReturnsError(string? definitionName)
    {
        // Arrange
        var validator = new InputSystemConfigurationValidator();

        // Act
        var validation = validator.ValidateCustomScheme(new InputSystemConfiguration([], [], new(), new()),
            new CustomInputScheme() { DefinitionName = definitionName!, Name = "Abc", DeviceMaps = [] },
            false);

        // Assert
        Assert.False(validation.IsValid);
        Assert.Equal(InputConfigurationValidation.MissingData, validation.Result);
        Assert.Equal(InputConfigurationType.Scheme, validation.ConfigurationType);
        Assert.Equal("Name", validation.TargetName);
    }

    [Fact]
    public void ValidateCustomScheme_DefinitionNameNotFound_ReturnsError()
    {
        // Arrange
        var validator = new InputSystemConfigurationValidator();

        // Act
        var validation = validator.ValidateCustomScheme(new InputSystemConfiguration([], [], new(), new()),
            new CustomInputScheme() { DefinitionName = "Abc", Name = "Abc", DeviceMaps = [] },
            false);

        // Assert
        Assert.False(validation.IsValid);
        Assert.Equal(InputConfigurationValidation.InvalidData, validation.Result);
        Assert.Equal(InputConfigurationType.Definition, validation.ConfigurationType);
        Assert.Equal("Name", validation.TargetName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void ValidateCustomScheme_InvalidSchemeName_ReturnsError(string? schemeName)
    {
        // Arrange
        var validator = new InputSystemConfigurationValidator();

        // Act
        var validation = validator.ValidateCustomScheme(new InputSystemConfiguration([], [new InputDefinition("Abc", [], [], false)], new(), new()),
            new CustomInputScheme() { DefinitionName = "Abc", Name = schemeName!, DeviceMaps = [] },
            false);

        // Assert
        Assert.False(validation.IsValid);
        Assert.Equal(InputConfigurationValidation.MissingData, validation.Result);
        Assert.Equal(InputConfigurationType.Scheme, validation.ConfigurationType);
        Assert.Equal("Name", validation.TargetName);
    }

    [Fact]
    public void ValidateCustomScheme_DuplicateSchemeName_OriginalSchemeBuiltIn_DontSkipDuplicateCustomSchemeGuard_ReturnsDuplicateError()
    {
        // Arrange
        var validator = new InputSystemConfigurationValidator();

        // Act
        var validation = validator.ValidateCustomScheme(new InputSystemConfiguration([], 
            [
                new InputDefinition("Abc", [],
                    [
                        new InputScheme("Abc", [], false, false)
                    ], false)
            ], new(), new()),
            new CustomInputScheme() { DefinitionName = "Abc", Name = "Abc", DeviceMaps = [] },
            false);

        // Assert
        Assert.False(validation.IsValid);
        Assert.Equal(InputConfigurationValidation.DuplicateData, validation.Result);
        Assert.Equal(InputConfigurationType.Scheme, validation.ConfigurationType);
        Assert.Equal("Name", validation.TargetName);
    }

    [Fact]
    public void ValidateCustomScheme_DuplicateSchemeName_OriginalSchemeBuiltIn_SkipDuplicateCustomSchemeGuard_ReturnsDuplicateError()
    {
        // Arrange
        var validator = new InputSystemConfigurationValidator();

        // Act
        var validation = validator.ValidateCustomScheme(new InputSystemConfiguration([],
            [
                new InputDefinition("Abc", [],
                    [
                        new InputScheme("Abc", [], false, false)
                    ], false)
            ], new(), new()),
            new CustomInputScheme() { DefinitionName = "Abc", Name = "Abc", DeviceMaps = [] },
            true);

        // Assert
        Assert.False(validation.IsValid);
        Assert.Equal(InputConfigurationValidation.DuplicateData, validation.Result);
        Assert.Equal(InputConfigurationType.Scheme, validation.ConfigurationType);
        Assert.Equal("Name", validation.TargetName);
    }

    [Fact]
    public void ValidateCustomScheme_DuplicateSchemeName_OriginalSchemeCustom_DontSkipDuplicateCustomSchemeGuard_ReturnsDuplicateError()
    {
        // Arrange
        var validator = new InputSystemConfigurationValidator();

        // Act
        var validation = validator.ValidateCustomScheme(new InputSystemConfiguration([],
            [
                new InputDefinition("Abc", [],
                    [
                        new InputScheme("Abc", [], false, true)
                    ], false)
            ], new(), new()),
            new CustomInputScheme() { DefinitionName = "Abc", Name = "Abc", DeviceMaps = [] },
            false);

        // Assert
        Assert.False(validation.IsValid);
        Assert.Equal(InputConfigurationValidation.DuplicateData, validation.Result);
        Assert.Equal(InputConfigurationType.Scheme, validation.ConfigurationType);
        Assert.Equal("Name", validation.TargetName);
    }

    [Fact]
    public void ValidateCustomScheme_DuplicateSchemeName_OriginalSchemeCustom_SkipDuplicateCustomSchemeGuard_InvalidDeviceMaps_ReturnsError()
    {
        // Arrange
        var validator = new InputSystemConfigurationValidator();

        // Act
        var validation = validator.ValidateCustomScheme(new InputSystemConfiguration([],
            [
                new InputDefinition("Abc", [],
                    [
                        new InputScheme("Abc", [], false, true)
                    ], false)
            ], new(), new()),
            new CustomInputScheme() { DefinitionName = "Abc", Name = "Abc", DeviceMaps = [] },
            true);

        // Assert
        Assert.False(validation.IsValid);
        Assert.Equal(InputConfigurationValidation.MissingData, validation.Result);
        Assert.Equal(InputConfigurationType.Scheme, validation.ConfigurationType);
        Assert.Equal(nameof(CustomInputScheme.DeviceMaps), validation.TargetName);
    }

    #endregion
}
