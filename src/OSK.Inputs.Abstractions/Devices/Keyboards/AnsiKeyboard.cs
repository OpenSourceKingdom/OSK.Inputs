using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using OSK.Inputs.Abstractions.Configuration;
using OSK.Inputs.Abstractions.Inputs;
using OSK.Inputs.Abstractions.Devices.Keyboards;

namespace OSK.Inputs.Abstractions.Devices.Keyboards;

public class AnsiKeyboard : InputDeviceSpecification<IKeyboardInput>
{
    #region Variables

    public static InputDeviceIdentity Ansi = new("AnsiKeyboard", KeyboardInputs.KeyboardDeviceType);

    #endregion

    #region InputDeviceSpecification Overrides

    public override InputDeviceIdentity DeviceIdentity => Ansi;

    public override IKeyboardInput[] Inputs { get; } = [
        // Standard Row 1
        KeyboardInputs.BackTick, KeyboardInputs.Tilde,
        KeyboardInputs.Zero, KeyboardInputs.One, KeyboardInputs.Two, KeyboardInputs.Three,
        KeyboardInputs.Four, KeyboardInputs.Five, KeyboardInputs.Six, KeyboardInputs.Seven,
        KeyboardInputs.Eight, KeyboardInputs.Nine,
        KeyboardInputs.ExclamationPoint, KeyboardInputs.At, KeyboardInputs.Pound,
        KeyboardInputs.Dollar, KeyboardInputs.Percent, KeyboardInputs.Caret,
        KeyboardInputs.Ampersand, KeyboardInputs.Asterisk, KeyboardInputs.LeftParanthesis,
        KeyboardInputs.RightParanthesis,

        // Punctuation & Brackets
        KeyboardInputs.Minus, KeyboardInputs.Underscore, KeyboardInputs.Equal, KeyboardInputs.Plus,
        KeyboardInputs.LeftBracket, KeyboardInputs.LeftCurlyBrace, KeyboardInputs.RightBracket, KeyboardInputs.RightCurlyBrace,
        KeyboardInputs.BackSlash, KeyboardInputs.Pipe,
        KeyboardInputs.SemiColon, KeyboardInputs.Colon, KeyboardInputs.SingleQuote, KeyboardInputs.DoubleQuote,
        KeyboardInputs.Comma, KeyboardInputs.LessThan, KeyboardInputs.Period, KeyboardInputs.GreaterThan,
        KeyboardInputs.ForwardSlash, KeyboardInputs.QuestionMark,

        // Core Keys
        KeyboardInputs.BackSpace, KeyboardInputs.Tab, KeyboardInputs.Caps,
        KeyboardInputs.Enter, KeyboardInputs.Space, KeyboardInputs.Escape,

        // Modifiers
        KeyboardInputs.Shift, KeyboardInputs.Ctrl, KeyboardInputs.Alt,

        // Alphabet
        KeyboardInputs.A, KeyboardInputs.B, KeyboardInputs.C, KeyboardInputs.D, KeyboardInputs.E,
        KeyboardInputs.F, KeyboardInputs.G, KeyboardInputs.H, KeyboardInputs.I, KeyboardInputs.J,
        KeyboardInputs.K, KeyboardInputs.L, KeyboardInputs.M, KeyboardInputs.N, KeyboardInputs.O,
        KeyboardInputs.P, KeyboardInputs.Q, KeyboardInputs.R, KeyboardInputs.S, KeyboardInputs.T,
        KeyboardInputs.U, KeyboardInputs.V, KeyboardInputs.W, KeyboardInputs.X, KeyboardInputs.Y,
        KeyboardInputs.Z,

        // Function & Navigation
        KeyboardInputs.F1, KeyboardInputs.F2, KeyboardInputs.F3, KeyboardInputs.F4, KeyboardInputs.F5,
        KeyboardInputs.F6, KeyboardInputs.F7, KeyboardInputs.F8, KeyboardInputs.F9, KeyboardInputs.F10,
        KeyboardInputs.F11, KeyboardInputs.F12,
        KeyboardInputs.UpArrow, KeyboardInputs.LeftArrow, KeyboardInputs.DownArrow, KeyboardInputs.RightArrow,
        KeyboardInputs.Home, KeyboardInputs.Delete, KeyboardInputs.End
    ];

    #endregion
}
