using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OSK.Inputs.Models.Inputs;
using OSK.Inputs.Options;

namespace OSK.Inputs.Models.Configuration;

public class KeyboardAndMouseDevice(Type inputReaderType) : InputDevice(KeyboardAndMouseControllerName, inputReaderType)
{
    #region Static

    public readonly static InputControllerName KeyboardAndMouseControllerName = new InputControllerName("KeyBoardAndMouse");

    public static KeyBoardInput Zero = new KeyBoardInput("0");
    public static KeyBoardInput One = new KeyBoardInput("1");
    public static KeyBoardInput Two = new KeyBoardInput("2");
    public static KeyBoardInput Three = new KeyBoardInput("3");
    public static KeyBoardInput Four = new KeyBoardInput("4");
    public static KeyBoardInput Five = new KeyBoardInput("5");
    public static KeyBoardInput Six = new KeyBoardInput("6");
    public static KeyBoardInput Seven = new KeyBoardInput("7");
    public static KeyBoardInput Eight = new KeyBoardInput("8");
    public static KeyBoardInput Nine = new KeyBoardInput("9");

    public static KeyBoardInput BackTick = new KeyBoardInput("`");
    public static KeyBoardInput Minus = new KeyBoardInput("-");
    public static KeyBoardInput Equal = new KeyBoardInput("=");
    public static KeyBoardInput BackSpace = new KeyBoardInput("Back");
    public static KeyBoardInput Tab = new KeyBoardInput("Tab");
    public static KeyBoardInput Caps = new KeyBoardInput("Caps");
    public static KeyBoardInput Shift = new KeyBoardInput("Shift");
    public static KeyBoardInput Ctrl = new KeyBoardInput("Ctrl");
    public static KeyBoardInput Alt = new KeyBoardInput("Alt");
    public static KeyBoardInput Space = new KeyBoardInput("Space");
    public static KeyBoardInput Enter = new KeyBoardInput("Enter");
    public static KeyBoardInput BackSlash = new KeyBoardInput("BackSlash");
    public static KeyBoardInput LeftBracket = new KeyBoardInput("[");
    public static KeyBoardInput RightBracket = new KeyBoardInput("]");
    public static KeyBoardInput SemiColon = new KeyBoardInput(";");
    public static KeyBoardInput SingleQuote = new KeyBoardInput("'");
    public static KeyBoardInput Comma = new KeyBoardInput(",");
    public static KeyBoardInput Period = new KeyBoardInput(".");
    public static KeyBoardInput ForwardSlash = new KeyBoardInput("/");

    public static KeyboardCombination Tilde = new("~", Shift, BackTick);
    public static KeyboardCombination ExclamationPoint = new KeyboardCombination("!", Shift, One);
    public static KeyboardCombination At = new KeyboardCombination("@", Shift, Two);
    public static KeyboardCombination Pound = new KeyboardCombination("#", Shift, Three);
    public static KeyboardCombination Dollar = new KeyboardCombination("$", Shift, Four);
    public static KeyboardCombination Percent = new KeyboardCombination("%", Shift, Five);
    public static KeyboardCombination Exponent = new KeyboardCombination("^", Shift, Six);
    public static KeyboardCombination AmpersAnd = new KeyboardCombination("&", Shift, Seven);
    public static KeyboardCombination Asterisk = new KeyboardCombination("*", Shift, Eight);
    public static KeyboardCombination LeftParanthesis = new KeyboardCombination("(", Shift, Nine);
    public static KeyboardCombination RightParanthesis = new KeyboardCombination(")", Shift, Zero);
    public static KeyboardCombination Underscore = new KeyboardCombination("_", Shift, Minus);
    public static KeyboardCombination Plus = new KeyboardCombination("+", Shift, Equal);
    public static KeyboardCombination LeftCurlyBrace = new KeyboardCombination("{", Shift, LeftBracket);
    public static KeyboardCombination RightCurlyBrace = new KeyboardCombination("}", Shift, RightBracket);
    public static KeyboardCombination Colon = new KeyboardCombination(":", Shift, SemiColon);
    public static KeyboardCombination DoubleQuote = new KeyboardCombination("DoubleQuote", Shift, SingleQuote);
    public static KeyboardCombination LessThan = new KeyboardCombination("<", Shift, Comma);
    public static KeyboardCombination GreaterThan = new KeyboardCombination("+", Shift, Period);
    public static KeyboardCombination QuestionMark = new KeyboardCombination("+", Shift, ForwardSlash);
    public static KeyboardCombination Pipe = new KeyboardCombination("+", Shift, BackSlash);

    public static KeyBoardInput Q = new KeyBoardInput("Q");
    public static KeyBoardInput W = new KeyBoardInput("W");
    public static KeyBoardInput E = new KeyBoardInput("E");
    public static KeyBoardInput R = new KeyBoardInput("R");
    public static KeyBoardInput T = new KeyBoardInput("T");
    public static KeyBoardInput Y = new KeyBoardInput("Y");
    public static KeyBoardInput U = new KeyBoardInput("U");
    public static KeyBoardInput I = new KeyBoardInput("I");
    public static KeyBoardInput O = new KeyBoardInput("O");
    public static KeyBoardInput P = new KeyBoardInput("P");
    public static KeyBoardInput A = new KeyBoardInput("A");
    public static KeyBoardInput S = new KeyBoardInput("S");
    public static KeyBoardInput D = new KeyBoardInput("D");
    public static KeyBoardInput F = new KeyBoardInput("F");
    public static KeyBoardInput G = new KeyBoardInput("G");
    public static KeyBoardInput H = new KeyBoardInput("H");
    public static KeyBoardInput J = new KeyBoardInput("J");
    public static KeyBoardInput K = new KeyBoardInput("K");
    public static KeyBoardInput L = new KeyBoardInput("L");
    public static KeyBoardInput Z = new KeyBoardInput("Z");
    public static KeyBoardInput X = new KeyBoardInput("X");
    public static KeyBoardInput C = new KeyBoardInput("C");
    public static KeyBoardInput V = new KeyBoardInput("V");
    public static KeyBoardInput B = new KeyBoardInput("B");
    public static KeyBoardInput N = new KeyBoardInput("N");
    public static KeyBoardInput M = new KeyBoardInput("M");

    public static KeyBoardInput UpArrow = new KeyBoardInput("Up");
    public static KeyBoardInput RightArrow = new KeyBoardInput("Right");
    public static KeyBoardInput LeftArrow = new KeyBoardInput("Left");
    public static KeyBoardInput DownArrow = new KeyBoardInput("Down");

    public static KeyBoardInput NumPad_Zero = new KeyBoardInput("NumPad_0");
    public static KeyBoardInput NumPad_One = new KeyBoardInput("NumPad_1");
    public static KeyBoardInput NumPad_Two = new KeyBoardInput("NumPad_2");
    public static KeyBoardInput NumPad_Three = new KeyBoardInput("NumPad_3");
    public static KeyBoardInput NumPad_Four = new KeyBoardInput("NumPad_4");
    public static KeyBoardInput NumPad_Five = new KeyBoardInput("NumPad_5");
    public static KeyBoardInput NumPad_Six = new KeyBoardInput("NumPad_6");
    public static KeyBoardInput NumPad_Seven = new KeyBoardInput("NumPad_7");
    public static KeyBoardInput NumPad_Eight = new KeyBoardInput("NumPad_8");
    public static KeyBoardInput NumPad_Nine = new KeyBoardInput("NumPad_9");
    public static KeyBoardInput NumPad_Enter = new KeyBoardInput("NumPad_Enter");
    public static KeyBoardInput NumPad_Plus = new KeyBoardInput("NumPad_Plus");
    public static KeyBoardInput NumPad_ForwardSlash = new KeyBoardInput("/");
    public static KeyBoardInput NumPad_Asterisk = new KeyBoardInput("NumPad_*");
    public static KeyBoardInput NumPad_Minus = new KeyBoardInput("NumPad_-");

    public static readonly DigitalInput LeftClick = new DigitalInput("Left_Click");
    public static readonly DigitalInput RightClick = new DigitalInput("Right_Click");

    public static readonly DigitalInput ScrollWheelClick = new DigitalInput("Middle_Click");
    public static readonly AnalogInput ScrollWheel = new AnalogInput("Scroll_Wheel");

    public static readonly AnalogInput Movement = new AnalogInput("Mouse_Move");

    public static IEnumerable<IInput> KeyboardInputs = [
        Zero, One, Two, Three, Four, Five, Six, Seven, Eight, Nine,
        BackTick, Minus, Equal, BackSpace, Tab, Caps, Shift, Ctrl,
        Alt, Space, Enter, BackSlash, LeftBracket, RightBracket,
        SemiColon, SingleQuote, Comma, Period, ForwardSlash,
        Tilde, ExclamationPoint, At, Pound, Dollar, Percent,
        Exponent, AmpersAnd, Asterisk, LeftParanthesis, RightParanthesis,
        Underscore, Plus, LeftCurlyBrace, RightCurlyBrace, Colon, DoubleQuote,
        LessThan, GreaterThan, QuestionMark, Pipe,
        Q, W, E, R, T, Y, U, I, O, P, A, S, D, F, G, H, J, K, L, Z, X, C, V, B,
        B, N, M,
        UpArrow, LeftArrow, DownArrow, RightArrow,
        NumPad_Zero, NumPad_One, NumPad_Two, NumPad_Three, NumPad_Four, NumPad_Five,
        NumPad_Six, NumPad_Seven, NumPad_Eight, NumPad_Nine,
        NumPad_Enter, NumPad_Plus, NumPad_ForwardSlash, NumPad_Asterisk, NumPad_Minus,
    ];

    public static IEnumerable<IInput> MouseInputs = [
        LeftClick, RightClick, ScrollWheelClick, ScrollWheel, Movement
    ];

    #endregion

    #region InputDevice Overrides

    public override IEnumerable<IInput> AllInputs { get; } = KeyboardInputs.Concat(MouseInputs);

    #endregion
}
