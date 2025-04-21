using Microsoft.Extensions.DependencyInjection;
using OSK.Inputs.Models.Configuration;
using OSK.Inputs.UnitTests._Helpers;
using Xunit;

namespace OSK.Inputs.UnitTests.Internal;
public class InputSystemConfigurationTests
{
    [Fact]
    public void InputSystemConfigurationTests_AddInputSystemAndInputDevices_NoErrors()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddInputs(builder =>
        {
            builder.AddKeyboard<TestInputReader>(_ => { });
            builder.AddMouse<TestInputReader>(_ => { });
            builder.AddPlayStationController<TestInputReader>(_ => { });
            builder.AddXboxController<TestInputReader>(_ => { });
            builder.AddSensorController<TestInputReader>(_ => { });

            builder.AddInputDefinition("Test", definition =>
            {
                definition.AddAction("Trigger", _ => ValueTask.CompletedTask);
                definition.AddInputScheme("abc", scheme =>
                {
                    scheme.AddKeyboardMaps(keyboard =>
                    {
                        keyboard.AssignStartAction(Keyboard.W, "Trigger");
                    });
                });
            });
        });

        // Act/Assert
        serviceCollection.BuildServiceProvider();
    }
}
