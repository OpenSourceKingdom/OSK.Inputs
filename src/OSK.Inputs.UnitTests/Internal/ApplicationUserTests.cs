using System.Numerics;
using Moq;
using OSK.Inputs.Internal;
using OSK.Inputs.Internal.Services;
using OSK.Inputs.Models.Configuration;
using OSK.Inputs.Models.Inputs;
using OSK.Inputs.Models.Runtime;
using OSK.Inputs.Ports;
using OSK.Inputs.UnitTests._Helpers;
using Xunit;

namespace OSK.Inputs.UnitTests.Internal;
public class ApplicationUserTests
{
    #region Variables

    private readonly InputDefinition _testDefinition;
    private readonly List<IInputDeviceConfiguration> _deviceConfigurations;
    private readonly List<InputControllerConfiguration> _controllerConfigurations;
    private readonly InputScheme _testScheme;

    private ApplicationInputUser _user;

    #endregion

    #region Constructors

    public ApplicationUserTests()
    {
        _deviceConfigurations = [];

        var mockDevice = new Mock<IInputDeviceConfiguration>();
        mockDevice.SetupGet(m => m.DeviceName)
            .Returns(new InputDeviceName("abc"));
        mockDevice.SetupGet(m => m.Inputs)
            .Returns([]);

        _deviceConfigurations.Add(mockDevice.Object);

        var mockDevice2 = new Mock<IInputDeviceConfiguration>();
        mockDevice2.SetupGet(m => m.DeviceName)
            .Returns(new InputDeviceName("NewDevice"));
        mockDevice2.SetupGet(m => m.Inputs)
            .Returns([]);
        
        _deviceConfigurations.Add(mockDevice2.Object);

        _controllerConfigurations = [];
        _controllerConfigurations.Add(new InputControllerConfiguration("test", [new InputDeviceName("test")]));
        _controllerConfigurations.Add(new InputControllerConfiguration("abc", [new InputDeviceName("abc")]));

        _testScheme = new InputScheme("Abc", "abc", false, []);
        _testDefinition = new InputDefinition("Abc", [new InputAction("abc", _ => ValueTask.CompletedTask, null)], [ _testScheme ]);

        _user = new ApplicationInputUser(1, new InputSystemConfiguration([_testDefinition], [], _deviceConfigurations, false, 1));
        _user.SetActiveInputDefinition(_testDefinition, [ _testScheme ]);
    }

    #endregion

    #region Id

    [Fact]
    public void Get_Id_ReturnsExpectedId()
    {
        //Arrange/Act/Assert
        Assert.Equal(1, _user.Id);
    }

    #endregion

    #region DeviceIdentifiers

    [Fact]
    public void DeviceIdentifiers_NoInputDevices_ReturnsEmptyList()
    {
        // Arrange/Act/Assert
        Assert.Empty(_user.DeviceIdentifiers);
    }

    [Fact]
    public void DeviceIdentifiers_HasInputDevices_ReturnsDeviceIdentifiers()
    {
        // Arrange
        RuntimeInputDevice[] devices = [
            new RuntimeInputDevice(1, new InputDeviceIdentifier(123, new InputDeviceName("test")), Mock.Of<IInputDeviceConfiguration>(), Mock.Of<IInputReader>()),
            new RuntimeInputDevice(1, new InputDeviceIdentifier(234, new InputDeviceName("test")), Mock.Of<IInputDeviceConfiguration>(), Mock.Of<IInputReader>()),
        ];

        _user.AddInputDevices(devices);

        // Act
        var result = _user.DeviceIdentifiers;

        // Assert
        foreach (var device in devices)
        {
            Assert.Contains(result, identifier => identifier == device.DeviceIdentifier);
        }
    }

    #endregion

    #region ActiveInputDefiniton

    [Fact]
    public void Get_ActiveInputDefinition_ReturnsExpectedDefinition()
    {
        //Arrange/Act/Assert
        Assert.Equal(_testDefinition, _user.ActiveInputDefinition);
    }

    #endregion

    #region GetActiveInputScheme

    [Fact]
    public void GetActiveInputScheme_NoSchemeForDeviceName_ReturnsNull()
    {
        // Arrange/Act/Assert
        Assert.Null(_user.GetActiveInputScheme("nope"));
    }

    [Fact]
    public void GetActiveInputScheme_ValidInputScheme_ReturnsScheme()
    {
        // Arrange/Act
        var result = _user.GetActiveInputScheme(_testScheme.ControllerId);

        // Assert
        Assert.Equal(_testScheme, result);
    }

    #endregion

    #region TryGetDevice

    [Fact]
    public void TryGetDevice_DeviceNameNotFound_ReturnsFalse()
    {
        // Arrange/Act
        var result = _user.TryGetDevice(123, out var device);

        // Assert
        Assert.False(result);
        Assert.Null(device);
    }

    [Fact]
    public void TryGetDevice_ValidDevice_ReturnsTrue()
    {
        // Arrange
        var device = new RuntimeInputDevice(1, new InputDeviceIdentifier(123, new InputDeviceName("test")), Mock.Of<IInputDeviceConfiguration>(), Mock.Of<IInputReader>());

        _user.AddInputDevices(device);

        // Act
        var result = _user.TryGetDevice(device.DeviceIdentifier.DeviceId, out var actualDevice);

        // Assert
        Assert.True(result);
        Assert.Equal(device, actualDevice);
    }

    #endregion

    #region RemoveInputController

    [Fact]
    public void RemoveInputController_DeviceIdNotInDataSet_ReturnsSuccessfully()
    {
        // Arrange
        var controller = new RuntimeInputController(new InputControllerConfiguration("test", []),
            new InputScheme(string.Empty, string.Empty, false, []), []);

        // Act
        _user.RemoveInputController(controller);
    }

    [Fact]
    public void RemoveInputController_ValidControllerId_ReturnsSuccessfully()
    {
        // Arrange
        var mockInputReader = new Mock<IInputReader>();
        
        _user.SetActiveInputDefinition(_testDefinition, [_testScheme]);

        // Act
        _user.RemoveInputController(new RuntimeInputController(_controllerConfigurations.First(), _testScheme, []));

        // Assert
        Assert.False(_user.TryGetDevice(1, out _));
        mockInputReader.Verify(m => m.Dispose(), Times.Once);
    }

    #endregion

    #region SetActiveInputSchemes

    [Fact]
    public void SetActiveInputSchemes_OverwritesOriginalData()
    {
        // Arrange
        var newTestScheme = new InputScheme("whatdayaknow", "NewScheme", false, []);
        var newTestDefinition = new InputDefinition("whatdayaknow", [], [ newTestScheme ]);

        // Act
        _user.SetActiveInputDefinition(newTestDefinition, [newTestScheme]);

        // Assert
        Assert.Equal(newTestDefinition, _user.ActiveInputDefinition);
        Assert.Null(_user.GetActiveInputScheme(_testScheme.ControllerId));
        Assert.Equal(newTestScheme, _user.GetActiveInputScheme(newTestScheme.ControllerId));
    }

    #endregion

    #region ReadInputsAsync

    [Fact]
    public async Task ReadInputsAsync_CancellationRequested_EndsEarly_ReturnsEmptyList()
    {
        // Arrange
        using var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel();

        // Act
        var result = await _user.ReadInputsAsync(cancellationTokenSource.Token);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task ReadInputsAsync_NoActiveInputDevice_NoDevicesReadInput_ReturnsEmptyList()
    {
        // Arrange
        var noInputReader = new Mock<IInputReader>();
        RuntimeInputDevice[] devices = [
            new RuntimeInputDevice(1, new InputDeviceIdentifier(), Mock.Of<IInputDeviceConfiguration>(), noInputReader.Object)
        ];

        _user.AddInputDevices(devices);

        // Act
        var result = await _user.ReadInputsAsync();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task ReadInputsAsync_NoActiveInputDevice_DeviceReturnsInput_ReturnsExpectedList_TriggersActiveDeviceChangedEvent()
    {
        // Arrange
        var mockInput = new Mock<IInput>();
        mockInput.SetupGet(m => m.Id).Returns(1);

        var deviceIdentifier = new InputDeviceIdentifier(1, new InputDeviceName("abc"));
        var inputReader = new Mock<IInputReader>();
        inputReader.Setup(m => m.ReadInputsAsync(It.IsAny<UserInputReadContext>(), It.IsAny<CancellationToken>()))
            .Callback((UserInputReadContext readContext, CancellationToken _) =>
            {
                readContext.ActivateInput(new InputActionMapPair(mockInput.Object, new InputActionMap("abc", 1, InputPhase.Start)),
                    InputPhase.Start, Vector2.Zero);
            });

        var device = new RuntimeInputDevice(1, new InputDeviceIdentifier(1, new InputDeviceName("abc")), Mock.Of<IInputDeviceConfiguration>(), inputReader.Object);
        RuntimeInputDevice[] devices = [
            device
        ];

        var eventCalled = false;
        _user.OnActiveInputControllerChanged += (userId, controllerConfiguration) =>
        {
            Assert.Equal(userId, _user.Id);
            Assert.Equal(_controllerConfigurations.First(), controllerConfiguration);

            eventCalled = true;
        };

        _user.AddInputDevices(devices);

        // Act
        var result = await _user.ReadInputsAsync();

        // Assert
        Assert.Contains(result, input => input.ActivatedInput.Input.Id == 1);
        Assert.True(eventCalled);
    }

    [Fact]
    public async Task ReadInputsAsync_ActiveInputDevice_DeviceReturnsInputSkipsOtherDevices_ReturnsExpectedList()
    {
        // Arrange
        var mockInput = new Mock<IInput>();
        mockInput.SetupGet(m => m.Id).Returns(1);

        var deviceIdentifier = new InputDeviceIdentifier(1, new InputDeviceName("abc"));
        var inputReader1 = new Mock<IInputReader>();
        inputReader1.Setup(m => m.ReadInputsAsync(It.IsAny<UserInputReadContext>(), It.IsAny<CancellationToken>()))
            .Callback((UserInputReadContext readContext, CancellationToken _) =>
            {
                readContext.ActivateInput(new InputActionMapPair(mockInput.Object, new InputActionMap("abc", 1, InputPhase.Start)),
                    InputPhase.Start, Vector2.Zero);
            });

        var device1 = new RuntimeInputDevice(1, new InputDeviceIdentifier(1, new InputDeviceName("abc")), Mock.Of<IInputDeviceConfiguration>(), inputReader1.Object);
        _user.AddInputDevices(device1);
        await _user.ReadInputsAsync();

        var device2 = new RuntimeInputDevice(1, new InputDeviceIdentifier(2, new InputDeviceName("abc")), Mock.Of<IInputDeviceConfiguration>(), Mock.Of<IInputReader>());
        _user.AddInputDevices(device2);

        var eventCalled = false;
        _user.OnActiveInputControllerChanged += (userId, inputDevice) =>
        {
            throw new InvalidOperationException("Should not be hit");
        };

        // Act
        var result = await _user.ReadInputsAsync();

        // Assert
        Assert.Contains(result, input => input.ActivatedInput.Input.Id == 1);
        Assert.False(eventCalled);
    }

    [Fact]
    public async Task ReadInputsAsync_ActiveInputDevice_DeviceDoesNotReturnInput_OtherDevicesReturnInputSwitchesActiveDevice_ReturnsExpectedList()
    {
        // Arrange
        var inputReader1 = new Mock<IInputReader>();
        inputReader1.Setup(m => m.ReadInputsAsync(It.IsAny<UserInputReadContext>(), It.IsAny<CancellationToken>()));
        var device1 = new RuntimeInputDevice(1, new InputDeviceIdentifier(1, new InputDeviceName("abc")), Mock.Of<IInputDeviceConfiguration>(), inputReader1.Object);
        _user.AddInputDevices(device1);

        var inputReader2 = new Mock<IInputReader>();
        inputReader2.Setup(m => m.ReadInputsAsync(It.IsAny<UserInputReadContext>(), It.IsAny<CancellationToken>()))
            .Callback((UserInputReadContext readContext, CancellationToken _) =>
            {
                readContext.ActivateInput(new InputActionMapPair(Mock.Of<IInput>(), new InputActionMap("abc", 1, InputPhase.Start)),
                    InputPhase.Start, Vector2.Zero);
            });
        var device2 = new RuntimeInputDevice(1, new InputDeviceIdentifier(2, new InputDeviceName("abc")), Mock.Of<IInputDeviceConfiguration>(), inputReader2.Object);
        _user.AddInputDevices(device2);

        var eventCalled = false;
        _user.OnActiveInputControllerChanged += (userId, controllerConfiguration) =>
        {
            Assert.Equal(userId, _user.Id);
            Assert.Equal(_controllerConfigurations.Last(), controllerConfiguration);

            eventCalled = true;
        };

        // Act
        var result = await _user.ReadInputsAsync();

        // Assert
        Assert.True(result.Select(r => r.ActivatedInput).All(input => result.Any(r => r.ActivatedInput.Input.Name == input.Input.Name)));
        Assert.True(eventCalled);
    }

    #endregion

    #region OnInputDeviceConnected

    [Fact]
    public void OnInputDeviceConnected_InputReaderEventTriggered_TriggersUserEvent()
    {
        // Arrange
        var inputParameters = new InputReaderParameters(new InputDeviceIdentifier(123, new InputDeviceName("abc")), Mock.Of<IEnumerable<IInput>>());
        var testInputReader = new TestInputReader(inputParameters);
        var device = new RuntimeInputDevice(1, new InputDeviceIdentifier(123, new InputDeviceName("test")), Mock.Of<IInputDeviceConfiguration>(), testInputReader);

        _user.AddInputDevices(device);

        var eventCalled = false;

        _user.OnInputDeviceReconnected += (userId, inputDevice) =>
        {
            eventCalled = true;

            Assert.Equal(device, inputDevice);
            Assert.Equal(_user.Id, userId);
        };

        // Act
        testInputReader.TriggerConnectionEvent();

        // Assert
        Assert.True(eventCalled);
    }

    #endregion

    #region OnInputDeviceDisconnected

    [Fact]
    public void OnInputDeviceDisconnected_InputReaderEventTriggered_TriggersUserEvent()
    {
        // Arrange
        var inputParameters = new InputReaderParameters(new InputDeviceIdentifier(123, new InputDeviceName("abc")), Mock.Of<IEnumerable<IInput>>());
        var testInputReader = new TestInputReader(inputParameters);
        var device = new RuntimeInputDevice(1, new InputDeviceIdentifier(123, new InputDeviceName("test")), Mock.Of<IInputDeviceConfiguration>(), testInputReader);

        _user.AddInputDevices(device);

        var eventCalled = false;

        _user.OnInputDeviceDisconnected += (userId, inputDevice) =>
        {
            eventCalled = true;

            Assert.Equal(device, inputDevice);
            Assert.Equal(_user.Id, userId);
        };

        // Act
        testInputReader.TriggerDisconnectedEvent();

        // Assert
        Assert.True(eventCalled);
    }

    #endregion
}
