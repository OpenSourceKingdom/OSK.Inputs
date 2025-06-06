﻿using Microsoft.Extensions.DependencyInjection;
using OSK.Functions.Outputs.Logging;
using OSK.Inputs.Models.Configuration;
using OSK.Inputs.Models.Runtime;
using OSK.Inputs.Options;
using OSK.Inputs.Ports;
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
            builder.AddKeyboard<TestInputReader>();
            builder.AddMouse<TestInputReader>();
            builder.AddPlayStationController<TestInputReader>();
            builder.AddXboxController<TestInputReader>();
            builder.AddSensorController<TestInputReader>();

            builder.AddInputDefinition("Test", definition =>
            {
                definition.AddAction("Trigger", _ => ValueTask.CompletedTask);
                definition.AddInputScheme("abc", scheme =>
                {
                    scheme.UseKeyboard(keyboard =>
                    {
                        keyboard.AssignStartAction(Keyboard.W, "Trigger");
                    });
                });
            });
        });

        // Act/Assert
        serviceCollection.BuildServiceProvider();
    }

    [Fact]
    public async Task InputSystemConfigurationTests_EndToEnd()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddLoggingFunctionOutputs();
        serviceCollection.AddLogging();
        serviceCollection.AddInputs(builder =>
        {
            builder.AddKeyboard<TestInputReader>();
            builder.AddMouse<TestInputReader>();

            builder.AddInputDefinition("Test", definition =>
            {
                definition.AddAction("Trigger", _ => ValueTask.CompletedTask);
                definition.AddInputScheme("abc", scheme =>
                {
                    scheme.UseKeyboard(keyboard =>
                    {
                        keyboard.AssignActiveAction(Keyboard.W, "Trigger");
                    });
                });
            });
        });

        var provider = serviceCollection.BuildServiceProvider();

        var manager = provider.GetRequiredService<IInputManager>();

        await manager.JoinUserAsync(1, new JoinUserOptions() { DeviceIdentifiers =
            [
                new InputDeviceIdentifier(1, Keyboard.KeyboardName),
                new InputDeviceIdentifier(2, Keyboard.KeyboardName)
            ] 
        });

        // Act
        var inputs = await manager.ReadInputsAsync(InputReadOptions.DefaultTwoUsers);

        // Assert
    }
}
