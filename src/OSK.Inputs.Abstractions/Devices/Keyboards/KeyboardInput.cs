namespace OSK.Inputs.Abstractions.Devices.Keyboards;

/// <summary>
/// Defines all of the keyboard inputs available for the input system
/// </summary>
public enum KeyboardInput
{
    // Special / Non-ASCII (300 range)
    UpArrow = 300,
    RightArrow = 301,
    LeftArrow = 302,
    DownArrow = 303,
    CapsLock = 304,
    Control = 305,
    Alt = 306,
    F10 = 307,
    F11 = 308,
    F12 = 309,
    End = 310,
    Home = 311,

    // Standard ASCII Control & Symbols
    BackSpace = 8,
    Tab = 9,
    Enter = 13,
    Shift = 15,
    Escape = 27,
    Space = 32,
    Delete = 127,

    // Numeric Row
    Zero = 48,
    One = 49,
    Two = 50,
    Three = 51,
    Four = 52,
    Five = 53,
    Six = 54,
    Seven = 55,
    Eight = 56,
    Nine = 57,

    // Alphabet (ASCII Uppercase)
    A = 65, B = 66, C = 67, D = 68, E = 69, F = 70, G = 71, H = 72, I = 73,
    J = 74, K = 75, L = 76, M = 77, N = 78, O = 79, P = 80, Q = 81, R = 82,
    S = 83, T = 84, U = 85, V = 86, W = 87, X = 88, Y = 89, Z = 90,

    // Punctuation & Brackets
    Minus = 45,
    Equal = 61,
    LeftBracket = 91,
    RightBracket = 93,
    BackSlash = 92,
    SemiColon = 59,
    SingleQuote = 39,
    Comma = 44,
    Period = 46,
    ForwardSlash = 47,
    BackTick = 96,

    // Function Keys (240 range)
    F1 = 241,
    F2 = 242,
    F3 = 243,
    F4 = 244,
    F5 = 245,
    F6 = 246,
    F7 = 247,
    F8 = 248,
    F9 = 249,

    // Shift-Modified Symbols (Combinations)
    ExclamationPoint = 33,
    Pound = 35,
    Dollar = 36,
    Percent = 37,
    Ampersand = 38,
    LeftParanthesis = 40,
    RightParanthesis = 41,
    Asterisk = 42,
    Plus = 43,
    Colon = 58,
    LessThan = 60,
    GreaterThan = 62,
    QuestionMark = 63,
    At = 64,
    Caret = 94,
    Underscore = 95,
    LeftCurlyBrace = 123,
    Pipe = 124,
    RightCurlyBrace = 125,
    Tilde = 126,
    DoubleQuote = 147,
}
