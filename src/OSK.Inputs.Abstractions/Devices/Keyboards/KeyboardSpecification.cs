using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using OSK.Inputs.Abstractions.Configuration;
using OSK.Inputs.Abstractions.Inputs;
using OSK.Inputs.Abstractions.Devices.Keyboards;

namespace OSK.Inputs.Abstractions.Devices.Keyboards;

public class KeyboardSpecification : InputDeviceSpecification
{
    #region Variables

    public static InputDeviceIdentity Keyboard = new("Keyboard", KeyboardInputs.KeyboardDeviceType);

    #endregion

    #region InputDeviceSpecification Overrides

    public override InputDeviceIdentity DeviceIdentity => Keyboard;

    public override IReadOnlyCollection<Input> Inputs { get; } = [
        KeyboardInputs.Zero, KeyboardInputs.One, KeyboardInputs.Two, KeyboardInputs.Three, KeyboardInputs.Four, KeyboardInputs.Five, KeyboardInputs.Six,
        KeyboardInputs.Seven, KeyboardInputs.Eight, KeyboardInputs.Nine,
        KeyboardInputs.BackTick, KeyboardInputs.Minus, KeyboardInputs.Equal, KeyboardInputs.BackSpace, KeyboardInputs.Tab, KeyboardInputs.Caps,
        KeyboardInputs.Shift, KeyboardInputs.Ctrl, KeyboardInputs.Alt, KeyboardInputs.Space, KeyboardInputs.Enter, KeyboardInputs.BackSlash,
        KeyboardInputs.LeftBracket, KeyboardInputs.RightBracket, KeyboardInputs.SemiColon, KeyboardInputs.SingleQuote, KeyboardInputs.Comma,
        KeyboardInputs.Period, KeyboardInputs.ForwardSlash, KeyboardInputs.Tilde, KeyboardInputs.ExclamationPoint, KeyboardInputs.At,
        KeyboardInputs.Pound, KeyboardInputs.Dollar, KeyboardInputs.Percent, KeyboardInputs.Caret, KeyboardInputs.Ampersand, KeyboardInputs.Asterisk,
        KeyboardInputs.LeftParanthesis, KeyboardInputs.RightParanthesis, KeyboardInputs.Underscore, KeyboardInputs.Plus, KeyboardInputs.LeftCurlyBrace,
        KeyboardInputs.RightCurlyBrace, KeyboardInputs.Colon, KeyboardInputs.DoubleQuote, KeyboardInputs.LessThan, KeyboardInputs.GreaterThan,
        KeyboardInputs.QuestionMark, KeyboardInputs.Pipe,
        KeyboardInputs.Q, KeyboardInputs.W, KeyboardInputs.E, KeyboardInputs.R, KeyboardInputs.T, KeyboardInputs.Y, KeyboardInputs.U, KeyboardInputs.I,
        KeyboardInputs.O, KeyboardInputs.P, KeyboardInputs.A, KeyboardInputs.S, KeyboardInputs.D, KeyboardInputs.F, KeyboardInputs.G, KeyboardInputs.H,
        KeyboardInputs.J, KeyboardInputs.K, KeyboardInputs.L, KeyboardInputs.Z, KeyboardInputs.X, KeyboardInputs.C, KeyboardInputs.V, KeyboardInputs.B,
        KeyboardInputs.N, KeyboardInputs.M,
        KeyboardInputs.F1, KeyboardInputs.F2, KeyboardInputs.F3, KeyboardInputs.F4, KeyboardInputs.F5, KeyboardInputs.F6, KeyboardInputs.F7,
        KeyboardInputs.F8, KeyboardInputs.F9, KeyboardInputs.F10, KeyboardInputs.F11, KeyboardInputs.F12,
        KeyboardInputs.UpArrow, KeyboardInputs.LeftArrow, KeyboardInputs.DownArrow, KeyboardInputs.RightArrow,
        KeyboardInputs.Home, KeyboardInputs.Delete, KeyboardInputs.Escape
    ];

    #endregion
}
