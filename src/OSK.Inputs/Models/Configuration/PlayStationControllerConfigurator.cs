using System;
using System.Collections.Generic;
using System.Text;
using OSK.Inputs.Models.Inputs;
using OSK.Inputs.Options;

namespace OSK.Inputs.Models.Configuration;
public class PlayStationControllerConfigurator(Type inputReaderType) : InputControllerConfigurator(PlayStationControllerName, inputReaderType)
{
    public static readonly InputControllerName PlayStationControllerName = new InputControllerName("PlayStationController");

    public PlayStationControllerConfigurator WithSquareInput(Action<DigitalInputOptions>? optionConfigurator = null)
    {
        AddInputConfiguration(GamePadInputs.Square, optionConfigurator);
        return this;
    }

    public PlayStationControllerConfigurator WithCircleInput(Action<DigitalInputOptions>? optionConfigurator = null)
    {
        AddInputConfiguration(GamePadInputs.Circle, optionConfigurator);
        return this;
    }

    public PlayStationControllerConfigurator WithTriangleInput(Action<DigitalInputOptions>? optionConfigurator = null)
    {
        AddInputConfiguration(GamePadInputs.Triangle, optionConfigurator);
        return this;
    }

    public PlayStationControllerConfigurator WithXInput(Action<DigitalInputOptions>? optionConfigurator = null)
    {
        AddInputConfiguration(GamePadInputs.X, optionConfigurator);
        return this;
    }

    public PlayStationControllerConfigurator WithRightBumper(Action<DigitalInputOptions>? optionConfigurator = null)
    {
        AddInputConfiguration(GamePadInputs.RightBumper, optionConfigurator);
        return this;
    }

    public PlayStationControllerConfigurator WithRightTrigger(Action<DigitalInputOptions>? optionConfigurator = null)
    {
        AddInputConfiguration(GamePadInputs.RightTrigger, optionConfigurator);
        return this;
    }

    public PlayStationControllerConfigurator WithLeftBumper(Action<DigitalInputOptions>? optionConfigurator = null)
    {
        AddInputConfiguration(GamePadInputs.LeftBumper, optionConfigurator);
        return this;
    }

    public PlayStationControllerConfigurator WithLeftTrigger(Action<DigitalInputOptions>? optionConfigurator = null)
    {
        AddInputConfiguration(GamePadInputs.LeftTrigger, optionConfigurator);
        return this;
    }

    public PlayStationControllerConfigurator WithDpadLeft(Action<DigitalInputOptions>? optionConfigurator = null)
    {
        AddInputConfiguration(GamePadInputs.DpadLeft, optionConfigurator);
        return this;
    }

    public PlayStationControllerConfigurator WithDpadRight(Action<DigitalInputOptions>? optionConfigurator = null)
    {
        AddInputConfiguration(GamePadInputs.DpadRight, optionConfigurator);
        return this;
    }


    public PlayStationControllerConfigurator WithDpadDown(Action<DigitalInputOptions>? optionConfigurator = null)
    {
        AddInputConfiguration(GamePadInputs.DpadDown, optionConfigurator);
        return this;
    }


    public PlayStationControllerConfigurator WithDpadUp(Action<DigitalInputOptions>? optionConfigurator = null)
    {
        AddInputConfiguration(GamePadInputs.DpadUp, optionConfigurator);
        return this;
    }

    public PlayStationControllerConfigurator WithMenu(Action<DigitalInputOptions>? optionConfigurator = null)
    {
        AddInputConfiguration(GamePadInputs.Menu, optionConfigurator);
        return this;
    }

    public PlayStationControllerConfigurator WithLeftJoyStickClick(Action<DigitalInputOptions>? optionConfigurator = null)
    {
        AddInputConfiguration(GamePadInputs.LeftJoyStickClick, optionConfigurator);
        return this;
    }

    public PlayStationControllerConfigurator WithLeftJoyStick(Action<AnalogInputOptions>? optionConfigurator = null)
    {
        AddInputConfiguration(GamePadInputs.LeftJoyStick, optionConfigurator);
        return this;
    }

    public PlayStationControllerConfigurator WithRightJoyStick(Action<AnalogInputOptions>? optionConfigurator = null)
    {
        AddInputConfiguration(GamePadInputs.RightJoyStick, optionConfigurator);
        return this;
    }
}
