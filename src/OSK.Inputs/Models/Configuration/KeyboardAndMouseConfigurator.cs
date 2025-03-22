using System;
using System.Collections.Generic;
using System.Text;
using OSK.Inputs.Models.Inputs;
using OSK.Inputs.Options;

namespace OSK.Inputs.Models.Configuration;

public class KeyboardAndMouseConfigurator(Type inputReaderType) : InputControllerConfigurator(KeyboardAndMouseControllerName, inputReaderType)
{
    public readonly static InputControllerName KeyboardAndMouseControllerName = new InputControllerName("KeyBoardAndMouse");

    public KeyboardAndMouseConfigurator WithKeyboardKey(KeyBoardInput key, Action<DigitalInputOptions>? optionConfigurator = null)
    {
        AddInputConfiguration(key, optionConfigurator);
        return this;
    }

    public KeyboardAndMouseConfigurator WithKeyboardKey(KeyboardCombination key, Action<DigitalInputOptions>? optionConfigurator = null)
    {
        AddInputConfiguration(key, optionConfigurator);
        return this;
    }

    public KeyboardAndMouseConfigurator WithLeftClick(Action<DigitalInputOptions>? optionConfigurator = null)
    {
        AddInputConfiguration(MouseInputs.LeftClick, optionConfigurator);
        return this;
    }

    public KeyboardAndMouseConfigurator WithRightClick(Action<DigitalInputOptions>? optionConfigurator = null)
    {
        AddInputConfiguration(MouseInputs.RightClick, optionConfigurator);
        return this;
    }

    public KeyboardAndMouseConfigurator WithScrollWheelClick(Action<DigitalInputOptions>? optionConfigurator = null)
    {
        AddInputConfiguration(MouseInputs.ScrollWheelClick, optionConfigurator);
        return this;
    }

    public KeyboardAndMouseConfigurator WithScroll(Action<AnalogInputOptions>? optionConfigurator = null)
    {
        AddInputConfiguration(MouseInputs.ScrollWheel, optionConfigurator);
        return this;
    }

    public KeyboardAndMouseConfigurator WithMouseTracking(Action<AnalogInputOptions>? optionConfigurator = null)
    {
        AddInputConfiguration(MouseInputs.Movement, optionConfigurator);
        return this;
    }
}
