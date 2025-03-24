using System;
using System.Collections.Generic;
using System.Text;
using OSK.Inputs.Models.Inputs;
using OSK.Inputs.Options;

namespace OSK.Inputs.Models.Configuration;
public class XboxControllerConfigurator(Type inputReaderType) : InputControllerConfigurator(XboxControllerName, inputReaderType)
{
    public static readonly InputControllerName XboxControllerName = new InputControllerName("XboxController");

    public XboxControllerConfigurator WithX(Action<DigitalInputOptions>? optionConfigurator = null)
    {
        AddInputConfiguration(GamePadInputs.X, optionConfigurator);
        return this;
    }

    public XboxControllerConfigurator WithY(Action<DigitalInputOptions>? optionConfigurator = null)
    {
        AddInputConfiguration(GamePadInputs.Y, optionConfigurator);
        return this;
    }

    public XboxControllerConfigurator WithB(Action<DigitalInputOptions>? optionConfigurator = null)
    {
        AddInputConfiguration(GamePadInputs.B, optionConfigurator);
        return this;
    }

    public XboxControllerConfigurator WithA(Action<DigitalInputOptions>? optionConfigurator = null)
    {
        AddInputConfiguration(GamePadInputs.A, optionConfigurator);
        return this;
    }

    public XboxControllerConfigurator WithRightBumper(Action<DigitalInputOptions>? optionConfigurator = null)
    {
        AddInputConfiguration(GamePadInputs.RightBumper, optionConfigurator);
        return this;
    }

    public XboxControllerConfigurator WithRightTrigger(Action<DigitalInputOptions>? optionConfigurator = null)
    {
        AddInputConfiguration(GamePadInputs.RightTrigger, optionConfigurator);
        return this;
    }

    public XboxControllerConfigurator WithLeftBumper(Action<DigitalInputOptions>? optionConfigurator = null)
    {
        AddInputConfiguration(GamePadInputs.LeftBumper, optionConfigurator);
        return this;
    }

    public XboxControllerConfigurator WithLeftTrigger(Action<DigitalInputOptions>? optionConfigurator = null)
    {
        AddInputConfiguration(GamePadInputs.LeftTrigger, optionConfigurator);
        return this;
    }

    public XboxControllerConfigurator WithDpadLeft(Action<DigitalInputOptions>? optionConfigurator = null)
    {
        AddInputConfiguration(GamePadInputs.DpadLeft, optionConfigurator);
        return this;
    }

    public XboxControllerConfigurator WithDpadRight(Action<DigitalInputOptions>? optionConfigurator = null)
    {
        AddInputConfiguration(GamePadInputs.DpadRight, optionConfigurator);
        return this;
    }

    public XboxControllerConfigurator WithDpadDown(Action<DigitalInputOptions>? optionConfigurator = null)
    {
        AddInputConfiguration(GamePadInputs.DpadDown, optionConfigurator);
        return this;
    }


    public XboxControllerConfigurator WithDpadUp(Action<DigitalInputOptions>? optionConfigurator = null)
    {
        AddInputConfiguration(GamePadInputs.DpadUp, optionConfigurator);
        return this;
    }

    public XboxControllerConfigurator WithMenu(Action<DigitalInputOptions>? optionConfigurator = null)
    {
        AddInputConfiguration(GamePadInputs.Menu, optionConfigurator);
        return this;
    }

    public XboxControllerConfigurator WithLeftJoyStickClick(Action<DigitalInputOptions>? optionConfigurator = null)
    {
        AddInputConfiguration(GamePadInputs.LeftJoyStickClick, optionConfigurator);
        return this;
    }

    public XboxControllerConfigurator WithLeftJoyStick(Action<AnalogInputOptions>? optionConfigurator = null)
    {
        AddInputConfiguration(GamePadInputs.LeftJoyStick, optionConfigurator);
        return this;
    }

    public XboxControllerConfigurator WithRightJoyStick(Action<AnalogInputOptions>? optionConfigurator = null)
    {
        AddInputConfiguration(GamePadInputs.RightJoyStick, optionConfigurator);
        return this;
    }
}
