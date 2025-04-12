using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OSK.Inputs.Models.Inputs;
using OSK.Inputs.Options;

namespace OSK.Inputs.Models.Configuration;

public class Keyboard(Type inputReaderType) : InputDevice(KeyboardName, inputReaderType)
{
    #region Static

    public readonly static InputDeviceName KeyboardName = new InputDeviceName("Keyboard");

    // Non Asci 2
    public static KeyBoardInput UpArrow = new KeyBoardInput(300, "Up", "˄");
    public static KeyBoardInput RightArrow = new KeyBoardInput(301, "Right", "˃");
    public static KeyBoardInput LeftArrow = new KeyBoardInput(302, "Left", "˂");
    public static KeyBoardInput DownArrow = new KeyBoardInput(303, "Down", "˅");

    public static KeyBoardInput Caps = new KeyBoardInput(304, "Caps Lock", "Caps");
    public static KeyBoardInput Ctrl = new KeyBoardInput(305, "Control", "Ctrl");
    public static KeyBoardInput Alt = new KeyBoardInput(306, "Alt", "Alt");

    // Ascii

    public static KeyBoardInput Zero = new KeyBoardInput(48, "Zero", "0");
    public static KeyBoardInput One = new KeyBoardInput(49, "One", "1");
    public static KeyBoardInput Two = new KeyBoardInput(50, "Two", "2");
    public static KeyBoardInput Three = new KeyBoardInput(51, "Three", "3");
    public static KeyBoardInput Four = new KeyBoardInput(52, "Four", "4");
    public static KeyBoardInput Five = new KeyBoardInput(53, "Five", "5");
    public static KeyBoardInput Six = new KeyBoardInput(54, "Six", "6");
    public static KeyBoardInput Seven = new KeyBoardInput(55, "Seven", "7");
    public static KeyBoardInput Eight = new KeyBoardInput(56, "Eight", "8");
    public static KeyBoardInput Nine = new KeyBoardInput(57, "Nine", "9");

    public static KeyBoardInput BackTick = new KeyBoardInput(96, "Back Tick", "`");
    public static KeyBoardInput Minus = new KeyBoardInput(45, "Minus", "-");
    public static KeyBoardInput Equal = new KeyBoardInput(61, "Equals", "=");
    public static KeyBoardInput BackSpace = new KeyBoardInput(8, "Backspace", "<-");
    public static KeyBoardInput Tab = new KeyBoardInput(9, "Tab", "Tab");
    public static KeyBoardInput Shift = new KeyBoardInput(15, "Shift", "Shift");
    public static KeyBoardInput Space = new KeyBoardInput(32, "Space", " ");
    public static KeyBoardInput Enter = new KeyBoardInput(13, "Enter", "Enter");
    public static KeyBoardInput BackSlash = new KeyBoardInput(92, "Back Slash", "\\");
    public static KeyBoardInput LeftBracket = new KeyBoardInput(91, "Left Bracket", "[");
    public static KeyBoardInput RightBracket = new KeyBoardInput(93, "Right Bracket", "]");
    public static KeyBoardInput SemiColon = new KeyBoardInput(59, "Semi colon", ";");
    public static KeyBoardInput SingleQuote = new KeyBoardInput(39, "Single Quote", "'");
    public static KeyBoardInput Comma = new KeyBoardInput(44, "Comma","");
    public static KeyBoardInput Period = new KeyBoardInput(46, "Period", ".");
    public static KeyBoardInput ForwardSlash = new KeyBoardInput(47, "Forward Slash", "/");

    public static KeyboardCombination Tilde = new(126, "Tilde", "~", Shift, BackTick);
    public static KeyboardCombination ExclamationPoint = new KeyboardCombination(33, "Exclamation Mark", "!", Shift, One);
    public static KeyboardCombination At = new KeyboardCombination(64, "At Sign", "@", Shift, Two);
    public static KeyboardCombination Pound = new KeyboardCombination(35, "Pound", "#", Shift, Three);
    public static KeyboardCombination Dollar = new KeyboardCombination(36, "Dollar", "$", Shift, Four);
    public static KeyboardCombination Percent = new KeyboardCombination(37, "Percent", "%", Shift, Five);
    public static KeyboardCombination Caret = new KeyboardCombination(94, "Caret", "^", Shift, Six);
    public static KeyboardCombination Ampersand = new KeyboardCombination(38, "Ampersand", " & ", Shift, Seven);
    public static KeyboardCombination Asterisk = new KeyboardCombination(42, "Asterisk", "*", Shift, Eight);
    public static KeyboardCombination LeftParanthesis = new KeyboardCombination(40, "Open Parenthesis", "(", Shift, Nine);
    public static KeyboardCombination RightParanthesis = new KeyboardCombination(41, "Close Parenthesis", ")", Shift, Zero);
    public static KeyboardCombination Underscore = new KeyboardCombination(95, "Underscore", "_", Shift, Minus);
    public static KeyboardCombination Plus = new KeyboardCombination(42, "Plus", "+", Shift, Equal);
    public static KeyboardCombination LeftCurlyBrace = new KeyboardCombination(123, "Left Curly Brace", "{", Shift, LeftBracket);
    public static KeyboardCombination RightCurlyBrace = new KeyboardCombination(125, "Right Curly Brace", "}", Shift, RightBracket);
    public static KeyboardCombination Colon = new KeyboardCombination(58, "Colon", ":", Shift, SemiColon);
    public static KeyboardCombination DoubleQuote = new KeyboardCombination(147, "Double Quote", "\"", Shift, SingleQuote);
    public static KeyboardCombination LessThan = new KeyboardCombination(60, "<", "Less Than", Shift, Comma);
    public static KeyboardCombination GreaterThan = new KeyboardCombination(62, "Greater Than", ">", Shift, Period);
    public static KeyboardCombination QuestionMark = new KeyboardCombination(63, "Question Mark", "?", Shift, ForwardSlash);
    public static KeyboardCombination Pipe = new KeyboardCombination(124, "Pipe", "|");

    public static KeyBoardInput A = new KeyBoardInput(65, "Uppercase A", "A");  
    public static KeyBoardInput B = new KeyBoardInput(66, "Uppercase B", "B");
    public static KeyBoardInput C = new KeyBoardInput(67, "Uppercase C", "C");
    public static KeyBoardInput D = new KeyBoardInput(68, "Uppercase D", "D");
    public static KeyBoardInput E = new KeyBoardInput(69, "Uppercase E", "E");
    public static KeyBoardInput F = new KeyBoardInput(70, "Uppercase F", "F");
    public static KeyBoardInput G = new KeyBoardInput(71, "Uppercase G", "G");
    public static KeyBoardInput H = new KeyBoardInput(72, "Uppercase H", "H");
    public static KeyBoardInput I = new KeyBoardInput(73, "Uppercase I", "I");
    public static KeyBoardInput J = new KeyBoardInput(74, "Uppercase J", "J");
    public static KeyBoardInput K = new KeyBoardInput(75, "Uppercase K", "K");
    public static KeyBoardInput L = new KeyBoardInput(76, "Uppercase L", "L");
    public static KeyBoardInput M = new KeyBoardInput(77, "Uppercase M", "M");
    public static KeyBoardInput N = new KeyBoardInput(78, "Uppercase N", "N");
    public static KeyBoardInput O = new KeyBoardInput(79, "Uppercase O", "O");
    public static KeyBoardInput P = new KeyBoardInput(80, "Uppercase P", "P");
    public static KeyBoardInput Q = new KeyBoardInput(81, "Uppercase Q", "Q");
    public static KeyBoardInput R = new KeyBoardInput(82, "Uppercase R", "R");
    public static KeyBoardInput S = new KeyBoardInput(83, "Uppercase S", "S");
    public static KeyBoardInput T = new KeyBoardInput(84, "Uppercase T", "T");
    public static KeyBoardInput U = new KeyBoardInput(85, "Uppercase U", "U");
    public static KeyBoardInput V = new KeyBoardInput(86, "Uppercase V", "V");
    public static KeyBoardInput W = new KeyBoardInput(87, "Uppercase W", "W");
    public static KeyBoardInput X = new KeyBoardInput(88, "Uppercase X", "X");
    public static KeyBoardInput Y = new KeyBoardInput(89, "Uppercase Y", "Y");
    public static KeyBoardInput Z = new KeyBoardInput(90, "Uppercase Z", "Z");

    #endregion

    #region InputDevice Overrides

    public override IEnumerable<IInput> AllInputs { get; } = [
        Zero, One, Two, Three, Four, Five, Six, Seven, Eight, Nine,
        BackTick, Minus, Equal, BackSpace, Tab, Caps, Shift, Ctrl,
        Alt, Space, Enter, BackSlash, LeftBracket, RightBracket,
        SemiColon, SingleQuote, Comma, Period, ForwardSlash,
        Tilde, ExclamationPoint, At, Pound, Dollar, Percent,
        Caret, Ampersand, Asterisk, LeftParanthesis, RightParanthesis,
        Underscore, Plus, LeftCurlyBrace, RightCurlyBrace, Colon, DoubleQuote,
        LessThan, GreaterThan, QuestionMark, Pipe,
        Q, W, E, R, T, Y, U, I, O, P, A, S, D, F, G, H, J, K, L, Z, X, C, V, B,
        B, N, M,
        UpArrow, LeftArrow, DownArrow, RightArrow
    ];

    #endregion
}
