using OSK.Inputs.Abstractions.Devices.Keyboards;

namespace OSK.Inputs.Abstractions.Devices.Keyboards;

/// <summary>
/// Defines all of the keyboard inputs available for the input system
/// </summary>
public static class KeyboardInputs
{
    #region Statics

    public const string KeyboardDeviceType = "Keyboard";

    // Non Ascii
    public static KeyboardKey UpArrow = new(300);
    public static KeyboardKey RightArrow = new(301);
    public static KeyboardKey LeftArrow = new(302);
    public static KeyboardKey DownArrow = new(303);

    public static KeyboardKey Caps = new(304);
    public static KeyboardKey Ctrl = new(305);
    public static KeyboardKey Alt = new(306);

    public static KeyboardKey F10 = new(307);
    public static KeyboardKey F11 = new(308);
    public static KeyboardKey F12 = new(309);

    public static KeyboardKey End = new(310);
    public static KeyboardKey Home = new(311);

    // Ascii
    public static KeyboardKey Zero = new(48);
    public static KeyboardKey One = new(49);
    public static KeyboardKey Two = new(50);
    public static KeyboardKey Three = new(51);
    public static KeyboardKey Four = new(52);
    public static KeyboardKey Five = new(53);
    public static KeyboardKey Six = new(54);
    public static KeyboardKey Seven = new(55);
    public static KeyboardKey Eight = new(56);
    public static KeyboardKey Nine = new(57);

    public static KeyboardKey BackTick = new(96);
    public static KeyboardKey Minus = new(45);
    public static KeyboardKey Equal = new(61);
    public static KeyboardKey BackSpace = new(8);
    public static KeyboardKey Tab = new(9);
    public static KeyboardKey Shift = new(15);
    public static KeyboardKey Space = new(32);
    public static KeyboardKey Enter = new(13);
    public static KeyboardKey BackSlash = new(92);
    public static KeyboardKey LeftBracket = new(91);
    public static KeyboardKey RightBracket = new(93);
    public static KeyboardKey SemiColon = new(59);
    public static KeyboardKey SingleQuote = new(39);
    public static KeyboardKey Comma = new(44);
    public static KeyboardKey Period = new(46);
    public static KeyboardKey ForwardSlash = new(47);

    public static KeyboardKey F1 = new(241);
    public static KeyboardKey F2 = new(242);
    public static KeyboardKey F3 = new(243);
    public static KeyboardKey F4 = new(244);
    public static KeyboardKey F5 = new(245);
    public static KeyboardKey F6 = new(246);
    public static KeyboardKey F7 = new(247);
    public static KeyboardKey F8 = new(248);
    public static KeyboardKey F9 = new(249);

    public static KeyboardKey Escape = new(27);
    public static KeyboardKey Delete = new(127);

    public static KeyboardCombination Tilde = new(126, Shift, BackTick);
    public static KeyboardCombination ExclamationPoint = new(33, Shift, One);
    public static KeyboardCombination At = new(64, Shift, Two);
    public static KeyboardCombination Pound = new(35, Shift, Three);
    public static KeyboardCombination Dollar = new(36, Shift, Four);
    public static KeyboardCombination Percent = new(37, Shift, Five);
    public static KeyboardCombination Caret = new(94, Shift, Six);
    public static KeyboardCombination Ampersand = new(38, Shift, Seven);
    public static KeyboardCombination Asterisk = new(42, Shift, Eight);
    public static KeyboardCombination LeftParanthesis = new(40, Shift, Nine);
    public static KeyboardCombination RightParanthesis = new(41,Shift, Zero);
    public static KeyboardCombination Underscore = new(95, Shift, Minus);
    public static KeyboardCombination Plus = new(43, Shift, Equal);
    public static KeyboardCombination LeftCurlyBrace = new(123, Shift, LeftBracket);
    public static KeyboardCombination RightCurlyBrace = new(125, Shift, RightBracket);
    public static KeyboardCombination Colon = new(58, Shift, SemiColon);
    public static KeyboardCombination DoubleQuote = new(147, Shift, SingleQuote);
    public static KeyboardCombination LessThan = new(60, Shift, Comma);
    public static KeyboardCombination GreaterThan = new(62, Shift, Period);
    public static KeyboardCombination QuestionMark = new(63, Shift, ForwardSlash);
    public static KeyboardCombination Pipe = new(124);

    public static KeyboardKey A = new(65);
    public static KeyboardKey B = new(66);
    public static KeyboardKey C = new(67);
    public static KeyboardKey D = new(68);
    public static KeyboardKey E = new(69);
    public static KeyboardKey F = new(70);
    public static KeyboardKey G = new(71);
    public static KeyboardKey H = new(72);
    public static KeyboardKey I = new(73);
    public static KeyboardKey J = new(74);
    public static KeyboardKey K = new(75);
    public static KeyboardKey L = new(76);
    public static KeyboardKey M = new(77);
    public static KeyboardKey N = new(78);
    public static KeyboardKey O = new(79);
    public static KeyboardKey P = new(80);
    public static KeyboardKey Q = new(81);
    public static KeyboardKey R = new(82);
    public static KeyboardKey S = new(83);
    public static KeyboardKey T = new(84);
    public static KeyboardKey U = new(85);
    public static KeyboardKey V = new(86);
    public static KeyboardKey W = new(87);
    public static KeyboardKey X = new(88);
    public static KeyboardKey Y = new(89);
    public static KeyboardKey Z = new(90);

    #endregion
}
