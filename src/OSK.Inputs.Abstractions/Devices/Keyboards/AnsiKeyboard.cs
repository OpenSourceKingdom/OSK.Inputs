namespace OSK.Inputs.Abstractions.Devices.Keyboards;

public class AnsiKeyboard : KeyboardDeviceSpecification
{
    #region KeyboardDeviceSpecification Overrides

    public override InputDeviceFamily DeviceFamily => InputDeviceFamily.Keyboards;

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

    #endregion
}
