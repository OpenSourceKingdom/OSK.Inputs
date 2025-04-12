using Moq;
using OSK.Inputs.Internal.Services;
using OSK.Inputs.Models.Configuration;
using OSK.Inputs.Models.Inputs;
using OSK.Inputs.Models.Runtime;
using OSK.Inputs.UnitTests._Helpers;
using Xunit;

namespace OSK.Inputs.UnitTests.Internal.Services;
public class DefaultInputReaderProviderTests
{
    #region Variables

    private readonly Mock<IServiceProvider> _mockServiceProvider;

    private readonly DefaultInputReaderProvider _provider;

    #endregion

    #region Constructors

    public DefaultInputReaderProviderTests()
    {
        _mockServiceProvider = new();

        _provider = new DefaultInputReaderProvider(_mockServiceProvider.Object);
    }

    #endregion

    #region GetInputReader

    [Fact]
    public void GetInputReader_NullInputConfiguration_ThrowsArgumentNullException()
    {
        // Arrange/Act/Assert
        Assert.Throws<ArgumentNullException>(() => _provider.GetInputReader(null!, new InputControllerIdentifier()));
    }

    [Fact]
    public void GetInputReader_Valid_ReturnsInputReader()
    {
        // Arrange
        var inputControllerIdentifier = new InputControllerIdentifier(117, new InputDeviceName("Spartan"));
        var mockControllerConfiguration = new Mock<IInputDeviceConfiguration>();
        mockControllerConfiguration.SetupGet(m => m.InputReaderType)
            .Returns(typeof(TestInputReader));

        var inputs = new List<IInput>();
        mockControllerConfiguration.SetupGet(m => m.Inputs)
            .Returns(inputs);

        // Act
        var reader = _provider.GetInputReader(mockControllerConfiguration.Object, inputControllerIdentifier);

        // Assert
        Assert.IsType<TestInputReader>(reader);

        var testReader = (TestInputReader)reader;

        Assert.Equal(inputControllerIdentifier.ControllerName, testReader.ControllerIdentifier.ControllerName);
        Assert.Equal(inputControllerIdentifier.ControllerId, testReader.ControllerIdentifier.ControllerId);

        Assert.Equal(inputs, testReader.Inputs);
    }

    #endregion
}
