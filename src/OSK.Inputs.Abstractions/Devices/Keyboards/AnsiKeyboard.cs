namespace OSK.Inputs.Abstractions.Devices.Keyboards;

public class AnsiKeyboard : KeyboardDeviceSpecification
{
    #region Variables

    public static InputDeviceFamily Ansi = new("AnsiKeyboard", InputDeviceType.Keyboard);

    #endregion

    #region KeyboardDeviceSpecification Overrides

    public override InputDeviceFamily DeviceFamily => Ansi;

    protected override KeyboardInput[] StandardKeys { get; } = [
        // Special / Non-ASCII
        KeyboardInput.UpArrow,
        KeyboardInput.RightArrow,
        KeyboardInput.LeftArrow,
        KeyboardInput.DownArrow,
        KeyboardInput.CapsLock,
        KeyboardInput.Control,
        KeyboardInput.Alt,
        KeyboardInput.F10,
        KeyboardInput.F11,
        KeyboardInput.F12,
        KeyboardInput.End,
        KeyboardInput.Home,

        // Standard ASCII Control & Symbols
        KeyboardInput.BackSpace,
        KeyboardInput.Tab,
        KeyboardInput.Enter,
        KeyboardInput.Shift,
        KeyboardInput.Escape,
        KeyboardInput.Space,
        KeyboardInput.Delete,

        // Numeric Row
        KeyboardInput.Zero,
        KeyboardInput.One,
        KeyboardInput.Two,
        KeyboardInput.Three,
        KeyboardInput.Four,
        KeyboardInput.Five,
        KeyboardInput.Six,
        KeyboardInput.Seven,
        KeyboardInput.Eight,
        KeyboardInput.Nine,

        // Alphabet
        KeyboardInput.A, KeyboardInput.B, KeyboardInput.C,
        KeyboardInput.D, KeyboardInput.E, KeyboardInput.F,
        KeyboardInput.G, KeyboardInput.H, KeyboardInput.I,
        KeyboardInput.J, KeyboardInput.K, KeyboardInput.L,
        KeyboardInput.M, KeyboardInput.N, KeyboardInput.O,
        KeyboardInput.P, KeyboardInput.Q, KeyboardInput.R,
        KeyboardInput.S, KeyboardInput.T, KeyboardInput.U,
        KeyboardInput.V, KeyboardInput.W, KeyboardInput.X,
        KeyboardInput.Y, KeyboardInput.Z,

        // Punctuation & Brackets
        KeyboardInput.Minus,
        KeyboardInput.Equal,
        KeyboardInput.LeftBracket,
        KeyboardInput.RightBracket,
        KeyboardInput.BackSlash,
        KeyboardInput.SemiColon,
        KeyboardInput.SingleQuote,
        KeyboardInput.Comma,
        KeyboardInput.Period,
        KeyboardInput.ForwardSlash,
        KeyboardInput.BackTick,

        // Function Keys
        KeyboardInput.F1,
        KeyboardInput.F2,
        KeyboardInput.F3,
        KeyboardInput.F4,
        KeyboardInput.F5,
        KeyboardInput.F6,
        KeyboardInput.F7,
        KeyboardInput.F8,
        KeyboardInput.F9
    ];

    protected override KeyboardCombination[] Combinations { get; } = [
        new(KeyboardInput.Tilde, KeyboardInput.Shift, KeyboardInput.BackTick),
        new(KeyboardInput.ExclamationPoint, KeyboardInput.Shift, KeyboardInput.One),
        new(KeyboardInput.At, KeyboardInput.Shift, KeyboardInput.Two),
        new(KeyboardInput.Pound, KeyboardInput.Shift, KeyboardInput.Three),
        new(KeyboardInput.Dollar, KeyboardInput.Shift, KeyboardInput.Four),
        new(KeyboardInput.Percent, KeyboardInput.Shift, KeyboardInput.Five),
        new(KeyboardInput.Caret, KeyboardInput.Shift, KeyboardInput.Six),
        new(KeyboardInput.Ampersand, KeyboardInput.Shift, KeyboardInput.Seven),
        new(KeyboardInput.Asterisk, KeyboardInput.Shift, KeyboardInput.Eight),
        new(KeyboardInput.LeftParanthesis, KeyboardInput.Shift, KeyboardInput.Nine),
        new(KeyboardInput.RightParanthesis, KeyboardInput.Shift, KeyboardInput.Zero),
        new(KeyboardInput.Underscore, KeyboardInput.Shift, KeyboardInput.Minus),
        new(KeyboardInput.Plus, KeyboardInput.Shift, KeyboardInput.Equal),
        new(KeyboardInput.LeftCurlyBrace, KeyboardInput.Shift, KeyboardInput.LeftBracket),
        new(KeyboardInput.RightCurlyBrace, KeyboardInput.Shift, KeyboardInput.RightBracket),
        new(KeyboardInput.Colon, KeyboardInput.Shift, KeyboardInput.SemiColon),
        new(KeyboardInput.DoubleQuote, KeyboardInput.Shift, KeyboardInput.SingleQuote),
        new(KeyboardInput.LessThan, KeyboardInput.Shift, KeyboardInput.Comma),
        new(KeyboardInput.GreaterThan, KeyboardInput.Shift, KeyboardInput.Period),
        new(KeyboardInput.QuestionMark, KeyboardInput.Shift, KeyboardInput.ForwardSlash),
        new(KeyboardInput.Pipe, KeyboardInput.Shift, KeyboardInput.BackSlash)
    ];

    #endregion
}
