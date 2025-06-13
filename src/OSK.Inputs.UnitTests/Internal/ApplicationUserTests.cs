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
    private readonly RuntimeInputDevice _testDevice;

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
        _controllerConfigurations.Add(new InputControllerConfiguration("NewDevice", [mockDevice.Object.DeviceName]));
        _controllerConfigurations.Add(new InputControllerConfiguration("abc", [mockDevice2.Object.DeviceName]));

        _testScheme = new InputScheme("Abc", "abc", false, [ new InputDeviceActionMap(mockDevice.Object.DeviceName, []) ]);
        _testDefinition = new InputDefinition("Abc", [new InputAction("abc", _ => ValueTask.CompletedTask, null)], [ _testScheme ]);

        _user = new ApplicationInputUser(1, new InputSystemConfiguration([_testDefinition], _controllerConfigurations, _deviceConfigurations, false, 1));

        _testDevice = new RuntimeInputDevice(new InputDeviceIdentifier(1, mockDevice.Object.DeviceName),
            mockDevice.Object, Mock.Of<IInputDeviceReader>());
        var runtimeInputDevice2 = new RuntimeInputDevice(new InputDeviceIdentifier(2, mockDevice2.Object.DeviceName),
            mockDevice2.Object, Mock.Of<IInputDeviceReader>());
        _user.AddInputDevices(_testDevice, runtimeInputDevice2);
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
        // Arrange
        var user = new ApplicationInputUser(1, new InputSystemConfiguration([_testDefinition], _controllerConfigurations, _deviceConfigurations, false, 1));

        // Act/Assert
        Assert.Empty(user.DeviceIdentifiers);
    }

    [Fact]
    public void DeviceIdentifiers_HasInputDevices_ReturnsDeviceIdentifiers()
    {
        // Arrange
        RuntimeInputDevice[] devices = [
            new RuntimeInputDevice(new InputDeviceIdentifier(123, new InputDeviceName("test")), Mock.Of<IInputDeviceConfiguration>(), Mock.Of<IInputDeviceReader>()),
            new RuntimeInputDevice(new InputDeviceIdentifier(234, new InputDeviceName("test")), Mock.Of<IInputDeviceConfiguration>(), Mock.Of<IInputDeviceReader>()),
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
        // Arrange
        var controller = new RuntimeInputController(_controllerConfigurations.First(), _testScheme, []);
        _user._inputControllers[controller.ControllerId] = controller;

        // Act
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
        var device = new RuntimeInputDevice(new InputDeviceIdentifier(123, new InputDeviceName("test")), Mock.Of<IInputDeviceConfiguration>(), Mock.Of<IInputDeviceReader>());

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
        var runtimeController = new RuntimeInputController(_controllerConfigurations.First(), _testScheme, [
            _testDevice]);

        _user._inputControllers[runtimeController.ControllerId] = runtimeController;

        // Act
        _user.RemoveInputController(runtimeController);

        // Assert
        Assert.False(_user.TryGetDevice(1, out _));
    }

    #endregion

    #region SetActiveInputSchemes

    [Fact]
    public void SetActiveInputSchemes_OverwritesOriginalData()
        {
        // Arrange
        _controllerConfigurations.Add(new InputControllerConfiguration("NewDevice", [new InputDeviceName("NewDevice")]));
        var newTestScheme = new InputScheme("whatdayaknow", "NewScheme", false, [ new InputDeviceActionMap(new InputDeviceName("NewDevice"), []) ]);
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
        var result = await _user.ReadInputsAsync(1, cancellationTokenSource.Token);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task ReadInputsAsync_NoActiveInputDevice_NoDevicesReadInput_ReturnsEmptyList()
    {
        // Arrange
        var noInputReader = new Mock<IInputDeviceReader>();
        RuntimeInputDevice[] devices = [
            new RuntimeInputDevice(new InputDeviceIdentifier(), Mock.Of<IInputDeviceConfiguration>(), noInputReader.Object)
        ];

        _user.AddInputDevices(devices);

        // Act
        var result = await _user.ReadInputsAsync(1);

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
        var inputReader = new Mock<IInputDeviceReader>();
        inputReader.Setup(m => m.ReadInputAsync(It.IsAny<DeviceInputReadContext>(), It.IsAny<IInput>(), It.IsAny<CancellationToken>()))
            .Callback((DeviceInputReadContext readContext, IInput input, CancellationToken _) =>
            {
                readContext.SetInputState(input, InputPhase.Start);
            });

        var device = new RuntimeInputDevice(new InputDeviceIdentifier(1, new InputDeviceName("abc")), Mock.Of<IInputDeviceConfiguration>(), inputReader.Object);
        device.SetActiveInputs(
            new InputDeviceActionMap(deviceIdentifier.DeviceName, 
                [new InputActionMap("abc", mockInput.Object.Id, new HashSet<InputPhase>() { InputPhase.Start })]), 
            [mockInput.Object]);

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
        _user._inputControllers["abc"] = new RuntimeInputController(_controllerConfigurations.First(),
            new InputScheme("abc", "abc", false, []), devices);

        // Act
        var result = await _user.ReadInputsAsync(1);

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
        var inputReader1 = new Mock<IInputDeviceReader>();
        inputReader1.Setup(m => m.ReadInputAsync(It.IsAny<DeviceInputReadContext>(), It.IsAny<IInput>(), It.IsAny<CancellationToken>()))
            .Callback((DeviceInputReadContext readContext, IInput input, CancellationToken _) =>
            {
                readContext.SetInputState(input, InputPhase.Start);
            });

        var device1 = new RuntimeInputDevice(new InputDeviceIdentifier(1, new InputDeviceName("abc")), Mock.Of<IInputDeviceConfiguration>(), inputReader1.Object);
        device1.SetActiveInputs(
            new InputDeviceActionMap(deviceIdentifier.DeviceName,
                [new InputActionMap(_testDefinition.InputActions.First().ActionKey, mockInput.Object.Id, new HashSet<InputPhase>() { InputPhase.Start })]),
            [mockInput.Object]);
        _user.AddInputDevices(device1);

        var device2 = new RuntimeInputDevice(new InputDeviceIdentifier(2, new InputDeviceName("def")), Mock.Of<IInputDeviceConfiguration>(), Mock.Of<IInputDeviceReader>());
        device2.SetActiveInputs(
            new InputDeviceActionMap(deviceIdentifier.DeviceName,
                [new InputActionMap(_testDefinition.InputActions.First().ActionKey, mockInput.Object.Id, new HashSet<InputPhase>() { InputPhase.Start })]),
            [mockInput.Object]);
        _user.AddInputDevices(device2);

        _user._inputControllers["abc"] = new RuntimeInputController(_controllerConfigurations.First(),
            new InputScheme("abc", "abc", false, []), [device1]);
        _user._inputControllers["def"] = new RuntimeInputController(_controllerConfigurations.First(),
            new InputScheme("abc", "abc", false, []), [device2]);

        await _user.ReadInputsAsync(1);

        var eventCalled = false;
        _user.OnActiveInputControllerChanged += (userId, inputDevice) =>
        {
            throw new InvalidOperationException("Should not be hit");
        };

        // Act
        var result = await _user.ReadInputsAsync(1);

        // Assert
        Assert.Contains(result, input => input.ActivatedInput.Input.Id == 1);
        Assert.False(eventCalled);
    }

    [Fact]
    public async Task ReadInputsAsync_ActiveInputDevice_DeviceDoesNotReturnInput_OtherDevicesReturnInputSwitchesActiveDevice_ReturnsExpectedList()
    {
        // Arrange
        var inputReader1 = new Mock<IInputDeviceReader>();
        inputReader1.Setup(m => m.ReadInputAsync(It.IsAny<DeviceInputReadContext>(), It.IsAny<IInput>(), It.IsAny<CancellationToken>()))
            .Callback((DeviceInputReadContext readContext, IInput input, CancellationToken _) =>
            {
                readContext.SetInputState(input, InputPhase.Start);
            });
        var device1 = new RuntimeInputDevice(new InputDeviceIdentifier(1, new InputDeviceName("abc")), Mock.Of<IInputDeviceConfiguration>(), inputReader1.Object);
        device1.SetActiveInputs(
            new InputDeviceActionMap(new InputDeviceName("abc"),
                [new InputActionMap(_testDefinition.InputActions.First().ActionKey, 0, new HashSet<InputPhase>() { InputPhase.Start })]),
            [ Mock.Of<IInput>()]); _user.AddInputDevices(device1);

        var inputReader2 = new Mock<IInputDeviceReader>();
        inputReader2.Setup(m => m.ReadInputAsync(It.IsAny<DeviceInputReadContext>(), It.IsAny<IInput>(), It.IsAny<CancellationToken>()))
            .Callback((DeviceInputReadContext readContext, IInput input, CancellationToken _) =>
            {
                readContext.SetInputState(input, InputPhase.Start);
            });
        var device2 = new RuntimeInputDevice(new InputDeviceIdentifier(2, new InputDeviceName("abc")), Mock.Of<IInputDeviceConfiguration>(), inputReader2.Object);
        device2.SetActiveInputs(
            new InputDeviceActionMap(new InputDeviceName("abc"),
                [new InputActionMap(_testDefinition.InputActions.First().ActionKey, 0, new HashSet<InputPhase>() { InputPhase.Start })]),
            [Mock.Of<IInput>()]); _user.AddInputDevices(device1); _user.AddInputDevices(device2);

        _user._inputControllers["abc"] = new RuntimeInputController(_controllerConfigurations.First(),
            new InputScheme("abc", "abc", false, []), [device1]);
        _user._inputControllers["def"] = new RuntimeInputController(_controllerConfigurations.Last(),
            new InputScheme("abc", "abc", false, []), [device2]);

        await _user.ReadInputsAsync(1);

        inputReader1.Setup(m => m.ReadInputAsync(It.IsAny<DeviceInputReadContext>(), It.IsAny<IInput>(), It.IsAny<CancellationToken>()));
        var eventCalled = false;
        _user.OnActiveInputControllerChanged += (userId, controllerConfiguration) =>
        {
            Assert.Equal(userId, _user.Id);
            Assert.Equal(_controllerConfigurations.Last(), controllerConfiguration);

            eventCalled = true;
        };

        // Act
        var result = await _user.ReadInputsAsync(1);

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
        var device = new RuntimeInputDevice(new InputDeviceIdentifier(123, new InputDeviceName("abc")), Mock.Of<IInputDeviceConfiguration>(), testInputReader);
        var inputController = new RuntimeInputController(_controllerConfigurations.First(), new InputScheme("abc", "abc", false, []),
            [device]);

        _user.AddInputDevices(device);
        _user._inputControllers[inputController.ControllerId] = inputController;

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
        var device = new RuntimeInputDevice(new InputDeviceIdentifier(123, new InputDeviceName("abc")), Mock.Of<IInputDeviceConfiguration>(), testInputReader);
        var inputController = new RuntimeInputController(_controllerConfigurations.First(), new InputScheme("abc", "abc", false, []),
            [device]);

        _user.AddInputDevices(device);
        _user._inputControllers[inputController.ControllerId] = inputController;

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
