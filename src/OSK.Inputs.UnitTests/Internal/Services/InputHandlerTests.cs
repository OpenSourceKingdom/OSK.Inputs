using Microsoft.Extensions.Options;
using Moq;
using OSK.Inputs.Internal;
using OSK.Inputs.Internal.Services;
using OSK.Inputs.Models.Inputs;
using OSK.Inputs.Models.Runtime;
using OSK.Inputs.Options;
using OSK.Inputs.UnitTests._Helpers;
using Xunit;

namespace OSK.Inputs.UnitTests.Internal.Services;

public class InputHandlerTests
{
    #region Variables

    private readonly Mock<IInputControllerListener> _mockControllerA;
    private readonly Mock<IInputControllerListener> _mockControllerB;

    private readonly InputHandler _handler;

    #endregion

    #region Constructors

    public InputHandlerTests()
    {
        _mockControllerA = new Mock<IInputControllerListener>();
        _mockControllerB = new Mock<IInputControllerListener>();
        var mockOptions = new Mock<IOptions<InputHandlerOptions>>();

        _handler = new([_mockControllerA.Object, _mockControllerB.Object], mockOptions.Object);
    }

    #endregion

    #region ReadInputsAsync

    [Fact]
    public async Task ReadInputsAsync_ActiveControllerReturnsInputs_DoesNotCheckInactiveControllers_ReturnsSuccessfully()
    {
        // Arrange
        var activatedInput = new ActivatedInput(1, "test", new TestInputA(), "A", InputPhase.Start, new InputPower([]));

        _mockControllerA.Setup(m => m.ReadInputsAsync(It.IsAny<InputHandlerOptions>()))
            .ReturnsAsync([activatedInput]);

        // Act
        var activatedInputs = await _handler.ReadInputsAsync();

        // Assert
        Assert.Single(activatedInputs);
        Assert.Equal(activatedInput, activatedInputs.First());

        _mockControllerA.Verify(m => m.ReadInputsAsync(It.IsAny<InputHandlerOptions>()), Times.Once);
        _mockControllerB.Verify(m => m.ReadInputsAsync(It.IsAny<InputHandlerOptions>()), Times.Never);
    }

    [Fact]
    public async Task ReadInputsAsync_ActiveControllerReturnsNoInputs_ChecksInactiveController_NoInput_DoesNotTriggerControllerChange_ReturnsSuccessfully()
    {
        // Arrange
        var activatedInput = new ActivatedInput(1, "test", new TestInputA(), "A", InputPhase.Start, new InputPower([]));

        _mockControllerA.Setup(m => m.ReadInputsAsync(It.IsAny<InputHandlerOptions>()))
            .ReturnsAsync([]);
        _mockControllerB.Setup(m => m.ReadInputsAsync(It.IsAny<InputHandlerOptions>()))
            .ReturnsAsync([]);

        var changedController = false;
        _handler.OnControllerChanged += _ => changedController = true; 

        // Act
        var activatedInputs = await _handler.ReadInputsAsync();

        // Assert
        Assert.Empty(activatedInputs);
        Assert.False(changedController);
        
        _mockControllerA.Verify(m => m.ReadInputsAsync(It.IsAny<InputHandlerOptions>()), Times.Once);
        _mockControllerB.Verify(m => m.ReadInputsAsync(It.IsAny<InputHandlerOptions>()), Times.Once);
    }

    [Fact]
    public async Task ReadInputsAsync_ActiveControllerReturnsNoInputs_ChecksInactiveController_ReceiversInput_DoesTriggerControllerChange_ReturnsSuccessfully()
    {
        // Arrange
        var activatedInput = new ActivatedInput(1, "test", new TestInputA(), "A", InputPhase.Start, new InputPower([]));

        _mockControllerA.Setup(m => m.ReadInputsAsync(It.IsAny<InputHandlerOptions>()))
            .ReturnsAsync([]);
        _mockControllerB.Setup(m => m.ReadInputsAsync(It.IsAny<InputHandlerOptions>()))
            .ReturnsAsync([ activatedInput ]);

        var changedController = false;
        _handler.OnControllerChanged += _ => changedController = true;

        // Assert Part 1: Validate backup controllers are check and new change event is triggered
        
        // Act
        var activatedInputs = await _handler.ReadInputsAsync();

        // Assert
        Assert.Single(activatedInputs);
        Assert.True(changedController);

        _mockControllerA.Verify(m => m.ReadInputsAsync(It.IsAny<InputHandlerOptions>()), Times.Once);
        _mockControllerB.Verify(m => m.ReadInputsAsync(It.IsAny<InputHandlerOptions>()), Times.Once);

        // Assert Part 2: Validate new active controller is called instead of original

        // Arrange
        changedController = false;
        _mockControllerA.Reset();
        _mockControllerB.Reset();

        _mockControllerA.Setup(m => m.ReadInputsAsync(It.IsAny<InputHandlerOptions>()))
            .ReturnsAsync([]);
        _mockControllerB.Setup(m => m.ReadInputsAsync(It.IsAny<InputHandlerOptions>()))
            .ReturnsAsync([activatedInput]);

        // Act
        activatedInputs = await _handler.ReadInputsAsync();

        // Assert
        Assert.Single(activatedInputs);
        Assert.False(changedController);

        _mockControllerA.Verify(m => m.ReadInputsAsync(It.IsAny<InputHandlerOptions>()), Times.Never);
        _mockControllerB.Verify(m => m.ReadInputsAsync(It.IsAny<InputHandlerOptions>()), Times.Once);
    }

    #endregion
}
