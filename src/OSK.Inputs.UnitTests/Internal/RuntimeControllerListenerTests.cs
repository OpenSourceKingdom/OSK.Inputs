using Moq;
using OSK.Inputs.Internal;
using OSK.Inputs.Models.Configuration;
using OSK.Inputs.Models.Inputs;
using OSK.Inputs.Models.Runtime;
using OSK.Inputs.Options;
using OSK.Inputs.Ports;
using OSK.Inputs.UnitTests._Helpers;
using Xunit;

namespace OSK.Inputs.UnitTests.Internal;

public class RuntimeControllerListenerTests
{
    #region Variables

    private readonly Mock<IInputReceiver> _mockInputReceiver;

    private readonly RuntimeControllerListener _controller;

    #endregion

    #region Constructors

    public RuntimeControllerListenerTests()
    {
        _mockInputReceiver = new Mock<IInputReceiver>();
        
        _controller = new RuntimeControllerListener(new InputControllerConfiguration("abc", [], []), [_mockInputReceiver.Object]);
    }

    #endregion

    #region ReadInputsAsync

    [Fact]
    public async Task ReadInputsAsync_OperationTakesLongerThanCancellationToken_OperationIsCancelledEarly_ReturnsEmpty()
    {
        // Arrange
        _mockInputReceiver.Setup(m => m.ReadInputsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((CancellationToken token) =>
            {
                Task.Delay(100).Wait();
                if (token.IsCancellationRequested)
                {
                    return [];
                }

                return [ new ActivatedInput("abc", new TestInputA(), "abc", InputPhase.Start, new InputPower([])) ];
            });

        // Act
        var inputs = await _controller.ReadInputsAsync(new InputHandlerOptions()
        {
            ControllerReadTime = TimeSpan.FromMilliseconds(20)
        });

        // Assert
        Assert.Empty(inputs);

        _mockInputReceiver.Verify(m => m.ReadInputsAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ReadInputsAsync_OperationIsShorterThanCancellationToken_ReturnsSuccessfully()
    {
        // Arrange
        var activatedInput = new ActivatedInput("abc", new TestInputA(), "abc", InputPhase.Start, new InputPower([]));

        _mockInputReceiver.Setup(m => m.ReadInputsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((CancellationToken token) =>
            {
                Task.Delay(100).Wait();
                if (token.IsCancellationRequested)
                {
                    return [];
                }

                return [activatedInput];
            });

        // Act
        var inputs = await _controller.ReadInputsAsync(new InputHandlerOptions()
        {
            ControllerReadTime = TimeSpan.FromMilliseconds(500)
        });

        // Assert
        Assert.Single(inputs);
        Assert.Equal(activatedInput, inputs.First());

        _mockInputReceiver.Verify(m => m.ReadInputsAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion
}
