using Moq;
using OSK.Inputs.Internal;
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
    private readonly InputScheme _testScheme;

    private ApplicationInputUser _user;

    #endregion

    #region Constructors

    public ApplicationUserTests()
    {
        _testScheme = new InputScheme("Abc", new InputControllerName("abc"), "abc", false, []);
        _testDefinition = new InputDefinition("Abc", [], [ _testScheme ]);

        _user = new ApplicationInputUser(1, _testDefinition, [_testScheme]);
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

    #region ControllerIdentifiers

    [Fact]
    public void ControllerIdentifiers_NoInputControllers_ReturnsEmptyList()
    {
        // Arrange/Act/Assert
        Assert.Empty(_user.ControllerIdentifiers);
    }

    [Fact]
    public void ControllerIdentifiers_HasInputControllers_ReturnsControllerIdentifiers()
    {
        // Arrange
        InputController[] controllers = [
            new InputController(new InputControllerIdentifier(123, new InputControllerName("test")), Mock.Of<IInputControllerConfiguration>(), Mock.Of<IInputReader>()),
            new InputController(new InputControllerIdentifier(234, new InputControllerName("test")), Mock.Of<IInputControllerConfiguration>(), Mock.Of<IInputReader>()),
        ];

        _user.AddInputControllers(controllers);

        // Act
        var result = _user.ControllerIdentifiers;

        // Assert
        foreach (var controller in controllers)
        {
            Assert.Contains(result, identifier => identifier == controller.ControllerIdentifier);
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
    public void GetActiveInputScheme_NoSchemeForControllerName_ReturnsNull()
    {
        // Arrange/Act/Assert
        Assert.Null(_user.GetActiveInputScheme(new InputControllerName("nope")));
    }

    [Fact]
    public void GetActiveInputScheme_ValidInputScheme_ReturnsScheme()
    {
        // Arrange/Act
        var result = _user.GetActiveInputScheme(_testScheme.ControllerName);

        // Assert
        Assert.Equal(_testScheme, result);
    }

    #endregion

    #region TryGetController

    [Fact]
    public void TryGetController_ControllerNameNotFound_ReturnsFalse()
    {
        // Arrange/Act
        var result = _user.TryGetController(123, out var controller);

        // Assert
        Assert.False(result);
        Assert.Null(controller);
    }

    [Fact]
    public void TryGetController_ValidController_ReturnsTrue()
    {
        // Arrange
        var controller = new InputController(new InputControllerIdentifier(123, new InputControllerName("test")), Mock.Of<IInputControllerConfiguration>(), Mock.Of<IInputReader>());

        _user.AddInputControllers(controller);

        // Act
        var result = _user.TryGetController(controller.ControllerIdentifier.ControllerId, out var actualController);

        // Assert
        Assert.True(result);
        Assert.Equal(controller, actualController);
    }

    #endregion

    #region RemoveInputController

    [Fact]
    public void RemoveInputController_ControllerIdNotInDataSet_ReturnsSuccessfully()
    {
        // Arrange/Act
        _user.RemoveInputController(new InputControllerIdentifier());
    }

    [Fact]
    public void RemoveInputController_ValidControllerId_ReturnsSuccessfully()
    {
        // Arrange
        var mockInputReader = new Mock<IInputReader>();
        var controller = new InputController(new InputControllerIdentifier(123, new InputControllerName("test")), Mock.Of<IInputControllerConfiguration>(), mockInputReader.Object);

        _user.AddInputControllers(controller);

        // Act
        _user.RemoveInputController(controller.ControllerIdentifier);

        // Assert
        Assert.False(_user.TryGetController(controller.ControllerIdentifier.ControllerId, out _));
        mockInputReader.Verify(m => m.Dispose(), Times.Once);
    }

    #endregion

    #region SetActiveInputSchemes

    [Fact]
    public void SetActiveInputSchemes_OverwritesOriginalData()
    {
        // Arrange
        var newTestScheme = new InputScheme("whatdayaknow", "NewController", "NewScheme", false, []);
        var newTestDefinition = new InputDefinition("whatdayaknow", [], [ newTestScheme ]);

        // Act
        _user.SetActiveInputSchemes(newTestDefinition, [newTestScheme]);

        // Assert
        Assert.Equal(newTestDefinition, _user.ActiveInputDefinition);
        Assert.Null(_user.GetActiveInputScheme(_testScheme.ControllerName));
        Assert.Equal(newTestScheme, _user.GetActiveInputScheme(newTestScheme.ControllerName));
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
    public async Task ReadInputsAsync_NoActiveInputController_NoControllersReadInput_ReturnsEmptyList()
    {
        // Arrange
        var noInputReader = new Mock<IInputReader>();
        noInputReader.Setup(m => m.ReadInputsAsync(It.IsAny<InputReadContext>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        InputController[] controllers = [
            new InputController(new InputControllerIdentifier(), Mock.Of<IInputControllerConfiguration>(), noInputReader.Object)
        ];

        _user.AddInputControllers(controllers);

        // Act
        var result = await _user.ReadInputsAsync();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task ReadInputsAsync_NoActiveInputController_ControllerReturnsInput_ReturnsExpectedList_TriggersActiveControllerChangedEvent()
    {
        // Arrange
        var controllerIdentifier = new InputControllerIdentifier(1, new InputControllerName("abc"));
        ActivatedInput[] readInputs = [
            new ActivatedInput(controllerIdentifier, Mock.Of<IInput>(), "a", InputPhase.Start, InputPower.FullPower(1))
        ];

        var inputReader = new Mock<IInputReader>();
        inputReader.Setup(m => m.ReadInputsAsync(It.IsAny<InputReadContext>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(readInputs);

        var controller = new InputController(new InputControllerIdentifier(1, new InputControllerName("abc")), Mock.Of<IInputControllerConfiguration>(), inputReader.Object);
        InputController[] controllers = [
            controller
        ];

        var eventCalled = false;
        _user.OnActiveInputControllerChanged += (userId, inputController) =>
        {
            Assert.Equal(userId, _user.Id);
            Assert.Equal(controller, inputController);

            eventCalled = true;
        };

        _user.AddInputControllers(controllers);

        // Act
        var result = await _user.ReadInputsAsync();

        // Assert
        Assert.True(readInputs.All(input => result.Any(r => r.Input.Name == input.Input.Name)));
        Assert.True(eventCalled);
    }

    [Fact]
    public async Task ReadInputsAsync_ActiveInputController_ControllerReturnsInputSkipsOtherControllers_ReturnsExpectedList()
    {
        // Arrange
        var controllerIdentifier = new InputControllerIdentifier(1, new InputControllerName("abc"));
        ActivatedInput[] readInputs1 = [
            new ActivatedInput(controllerIdentifier, Mock.Of<IInput>(), "a", InputPhase.Start, InputPower.FullPower(1))
        ];

        var inputReader1 = new Mock<IInputReader>();
        inputReader1.Setup(m => m.ReadInputsAsync(It.IsAny<InputReadContext>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(readInputs1);

        var controller1 = new InputController(new InputControllerIdentifier(1, new InputControllerName("abc")), Mock.Of<IInputControllerConfiguration>(), inputReader1.Object);
        _user.AddInputControllers(controller1);
        await _user.ReadInputsAsync();

        var controller2 = new InputController(new InputControllerIdentifier(2, new InputControllerName("abc")), Mock.Of<IInputControllerConfiguration>(), Mock.Of<IInputReader>());
        _user.AddInputControllers(controller2);

        var eventCalled = false;
        _user.OnActiveInputControllerChanged += (userId, inputController) =>
        {
            throw new InvalidOperationException("Should not be hit");
        };

        // Act
        var result = await _user.ReadInputsAsync();

        // Assert
        Assert.True(readInputs1.All(input => result.Any(r => r.Input.Name == input.Input.Name)));
        Assert.False(eventCalled);
    }

    [Fact]
    public async Task ReadInputsAsync_ActiveInputController_ControllerDoesNotReturnInput_OtherControllersReturnInputSwitchesActiveController_ReturnsExpectedList()
    {
        // Arrange
        var inputReader1 = new Mock<IInputReader>();
        inputReader1.Setup(m => m.ReadInputsAsync(It.IsAny<InputReadContext>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([
                new ActivatedInput(new InputControllerIdentifier(), Mock.Of<IInput>(), "a", InputPhase.Start, InputPower.FullPower(1))
            ]);
        var controller1 = new InputController(new InputControllerIdentifier(1, new InputControllerName("abc")), Mock.Of<IInputControllerConfiguration>(), inputReader1.Object);
        _user.AddInputControllers(controller1);
        
        await _user.ReadInputsAsync();

        // Reset reader to return nothing
        inputReader1.Setup(m => m.ReadInputsAsync(It.IsAny<InputReadContext>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var inputReader2 = new Mock<IInputReader>();
        ActivatedInput[] activatedInputs = [
                new ActivatedInput(new InputControllerIdentifier(), Mock.Of<IInput>(), "a", InputPhase.Start, InputPower.FullPower(1))
        ];
        inputReader2.Setup(m => m.ReadInputsAsync(It.IsAny<InputReadContext>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(activatedInputs);
        var controller2 = new InputController(new InputControllerIdentifier(2, new InputControllerName("abc")), Mock.Of<IInputControllerConfiguration>(), inputReader2.Object);
        _user.AddInputControllers(controller2);

        var eventCalled = false;
        _user.OnActiveInputControllerChanged += (userId, inputController) =>
        {
            Assert.Equal(userId, _user.Id);
            Assert.Equal(controller2, inputController);

            eventCalled = true;
        };

        // Act
        var result = await _user.ReadInputsAsync();

        // Assert
        Assert.True(activatedInputs.All(input => result.Any(r => r.Input.Name == input.Input.Name)));
        Assert.True(eventCalled);
    }

    #endregion

    #region OnInputControllerConnected

    [Fact]
    public void OnInputControllerConnected_InputReaderEventTriggered_TriggersUserEvent()
    {
        // Arrange
        var inputParameters = new InputReaderParameters(new InputControllerIdentifier(123, new InputControllerName("abc")), Mock.Of<IEnumerable<InputConfiguration>>());
        var testInputReader = new TestInputReader(inputParameters);
        var controller = new InputController(new InputControllerIdentifier(123, new InputControllerName("test")), Mock.Of<IInputControllerConfiguration>(), testInputReader);

        _user.AddInputControllers(controller);

        var eventCalled = false;

        _user.OnInputControllerConnected += (userId, inputController) =>
        {
            eventCalled = true;

            Assert.Equal(controller, inputController);
            Assert.Equal(_user.Id, userId);
        };

        // Act
        testInputReader.TriggerConnectionEvent();

        // Assert
        Assert.True(eventCalled);
    }

    #endregion

    #region OnInputControllerDisconnected

    [Fact]
    public void OnInputControllerDisconnected_InputReaderEventTriggered_TriggersUserEvent()
    {
        // Arrange
        var inputParameters = new InputReaderParameters(new InputControllerIdentifier(123, new InputControllerName("abc")), Mock.Of<IEnumerable<InputConfiguration>>());
        var testInputReader = new TestInputReader(inputParameters);
        var controller = new InputController(new InputControllerIdentifier(123, new InputControllerName("test")), Mock.Of<IInputControllerConfiguration>(), testInputReader);

        _user.AddInputControllers(controller);

        var eventCalled = false;

        _user.OnInputControllerDisconnected += (userId, inputController) =>
        {
            eventCalled = true;

            Assert.Equal(controller, inputController);
            Assert.Equal(_user.Id, userId);
        };

        // Act
        testInputReader.TriggerDisconnectedEvent();

        // Assert
        Assert.True(eventCalled);
    }

    #endregion
}
