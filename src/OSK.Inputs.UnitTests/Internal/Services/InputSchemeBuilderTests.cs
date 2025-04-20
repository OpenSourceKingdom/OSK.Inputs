using System.Data;
using Moq;
using OSK.Inputs.Internal.Services;
using OSK.Inputs.Models.Configuration;
using OSK.Inputs.Models.Inputs;
using Xunit;

namespace OSK.Inputs.UnitTests.Internal.Services;

public class InputSchemeBuilderTests
{
    #region Variables

    private const string DefinitionName = "testDefinition";
    private static InputDeviceName DeviceName = new InputDeviceName("testController");
    private const string SchemeName = "testScheme";

    private readonly Mock<IInputDeviceConfiguration> _mockDeviceConfiguration;

    private readonly InputSchemeBuilder _builder;

    #endregion

    #region Constructors

    public InputSchemeBuilderTests()
    {
        _mockDeviceConfiguration = new();
        _mockDeviceConfiguration.SetupGet(m => m.DeviceName)
            .Returns(DeviceName);

        _builder = new(DefinitionName, [ _mockDeviceConfiguration.Object ], SchemeName);
    }

    #endregion

    #region MakeDefault

    [Fact]
    public void MakeDefault_Valid_ReturnsSuccessfully()
    {
        // Arrange/Act/Assert
        _builder.MakeDefault();
    }

    #endregion
}
