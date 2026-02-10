using System;
using System.Windows.Forms;

namespace RogueCore
{
    /// <summary>
    /// Enum representing different keyboard keys
    /// </summary>
    public enum Key
    {
        None = 0,
        Esc, F1, F2, F3, F4, F5, F6, F7, F8, F9, F10, F11, F12,
        Tilda, Key1, Key2, Key3, Key4, Key5, Key6, Key7, Key8, Key9, Key0, KeyMinus, KeyPlus,
        Backspace, Enter, Space,
        Up, Down, Left, Right,
        NumSlash, NumAsterisk, NumMinus, NumPlus,
        Num1, Num2, Num3, Num4, Num5, Num6, Num7, Num8, Num9, Num0,
        Q, W, E, R, T, Y, U, I, O, P, 
        A, S, D, F, G, H, J, K, L, 
        Z, X, C, V, B, N, M, 
        LSquare, RSquare, Colon, Quotes, BackSlash, LAngle, RAngle, Slash,
    }

    /// <summary>
    /// Represents information about a keyboard key press including modifiers
    /// </summary>
    public class KeyInfo
    {
        public Key KeyCode { get; set; } = Key.None;
        public bool Shift { get; set; } = false;
        public bool Ctrl { get; set; } = false;
        public bool Alt { get; set; } = false;

        public override string ToString()
        {
            return KeyCode switch
            {
                Key.Tilda => Shift ? "~" : "`",
                Key.Key1 => Shift ? "!" : "1",
                Key.Key2 => Shift ? "@" : "2",
                Key.Key3 => Shift ? "#" : "3",
                Key.Key4 => Shift ? "$" : "4",
                Key.Key5 => Shift ? "%" : "5",
                Key.Key6 => Shift ? "^" : "6",
                Key.Key7 => Shift ? "&" : "7",
                Key.Key8 => Shift ? "*" : "8",
                Key.Key9 => Shift ? "(" : "9",
                Key.Key0 => Shift ? ")" : "0",
                Key.KeyMinus => Shift ? "_" : "-",
                Key.KeyPlus => Shift ? "+" : "=",
                Key.Q => Shift ? "Q" : "q",
                Key.W => Shift ? "W" : "w",
                Key.E => Shift ? "E" : "e",
                Key.R => Shift ? "R" : "r",
                Key.T => Shift ? "T" : "t",
                Key.Y => Shift ? "Y" : "y",
                Key.U => Shift ? "U" : "u",
                Key.I => Shift ? "I" : "i",
                Key.O => Shift ? "O" : "o",
                Key.P => Shift ? "P" : "p",
                Key.A => Shift ? "A" : "a",
                Key.S => Shift ? "S" : "s",
                Key.D => Shift ? "D" : "d",
                Key.F => Shift ? "F" : "f",
                Key.G => Shift ? "G" : "g",
                Key.H => Shift ? "H" : "h",
                Key.J => Shift ? "J" : "j",
                Key.K => Shift ? "K" : "k",
                Key.L => Shift ? "L" : "l",
                Key.Z => Shift ? "Z" : "z",
                Key.X => Shift ? "X" : "x",
                Key.C => Shift ? "C" : "c",
                Key.V => Shift ? "V" : "v",
                Key.B => Shift ? "B" : "b",
                Key.N => Shift ? "N" : "n",
                Key.M => Shift ? "M" : "m",
                Key.Space => " ",
                Key.NumSlash => "/",
                Key.NumAsterisk => "*",
                Key.NumMinus => "-",
                Key.NumPlus => "+",
                Key.Num0 => "0",
                Key.Num1 => "1",
                Key.Num2 => "2",
                Key.Num3 => "3",
                Key.Num4 => "4",
                Key.Num5 => "5",
                Key.Num6 => "6",
                Key.Num7 => "7",
                Key.Num8 => "8",
                Key.Num9 => "9",
                Key.LSquare => Shift ? "{" : "[",
                Key.RSquare => Shift ? "}" : "]",
                Key.Colon => Shift ? ":" : ";",
                Key.Quotes => Shift ? "\"" : "'",
                Key.BackSlash => Shift ? "|" : "\\",
                Key.LAngle => Shift ? "<" : ",",
                Key.RAngle => Shift ? ">" : ".",
                Key.Slash => Shift ? "?" : "/",
                _ => ""
            };
        }
    }

    /// <summary>
    /// Delegate for handling input key events
    /// </summary>
    /// <param name="key">The key information</param>
    public delegate void InputHandler(KeyInfo key);

    /// <summary>
    /// Handles keyboard input translation and dispatching
    /// </summary>
    public class Input
    {
        private InputHandler _inputHandler;

        /// <summary>
        /// Sends a key event to the registered handler
        /// </summary>
        /// <param name="key">The key information to send</param>
        public void SendKey(KeyInfo key)
        {
            _inputHandler?.Invoke(key);
        }

        /// <summary>
        /// Registers an input handler to receive key events
        /// </summary>
        /// <param name="handler">The handler to register</param>
        public void RegisterHandler(InputHandler handler)
        {
            _inputHandler = handler;
        }

        /// <summary>
        /// Translates Windows Forms KeyEventArgs to our internal KeyInfo representation
        /// </summary>
        /// <param name="e">The Windows Forms key event args</param>
        /// <returns>The translated key information</returns>
        public KeyInfo Translate(KeyEventArgs e)
        {
            var key = new KeyInfo
            {
                Shift = e.Shift,
                Ctrl = e.Control,
                Alt = e.Alt
            };

            // Handle special cases first
            switch (e.KeyCode)
            {
                case Keys.Enter:
                    key.KeyCode = Key.Enter;
                    return key;
                case Keys.Escape:
                    key.KeyCode = Key.Esc;
                    return key;
                case Keys.Back:
                    key.KeyCode = Key.Backspace;
                    return key;
                case Keys.Space:
                    key.KeyCode = Key.Space;
                    return key;
            }

            // Use pattern matching for regular keys
            key.KeyCode = e.KeyCode switch
            {
                Keys.F1 => Key.F1,
                Keys.F2 => Key.F2,
                Keys.F3 => Key.F3,
                Keys.F4 => Key.F4,
                Keys.F5 => Key.F5,
                Keys.F6 => Key.F6,
                Keys.F7 => Key.F7,
                Keys.F8 => Key.F8,
                Keys.F9 => Key.F9,
                Keys.F10 => Key.F10,
                Keys.F11 => Key.F11,
                Keys.F12 => Key.F12,
                Keys.D1 => Key.Key1,
                Keys.D2 => Key.Key2,
                Keys.D3 => Key.Key3,
                Keys.D4 => Key.Key4,
                Keys.D5 => Key.Key5,
                Keys.D6 => Key.Key6,
                Keys.D7 => Key.Key7,
                Keys.D8 => Key.Key8,
                Keys.D9 => Key.Key9,
                Keys.D0 => Key.Key0,
                Keys.Oem3 => Key.Tilda,
                Keys.OemMinus => Key.KeyMinus,
                Keys.Oemplus => Key.KeyPlus,
                Keys.Q => Key.Q,
                Keys.W => Key.W,
                Keys.E => Key.E,
                Keys.R => Key.R,
                Keys.T => Key.T,
                Keys.Y => Key.Y,
                Keys.U => Key.U,
                Keys.I => Key.I,
                Keys.O => Key.O,
                Keys.P => Key.P,
                Keys.A => Key.A,
                Keys.S => Key.S,
                Keys.D => Key.D,
                Keys.F => Key.F,
                Keys.G => Key.G,
                Keys.H => Key.H,
                Keys.J => Key.J,
                Keys.K => Key.K,
                Keys.L => Key.L,
                Keys.Z => Key.Z,
                Keys.X => Key.X,
                Keys.C => Key.C,
                Keys.V => Key.V,
                Keys.B => Key.B,
                Keys.N => Key.N,
                Keys.M => Key.M,
                Keys.NumPad0 => Key.Num0,
                Keys.NumPad1 => Key.Num1,
                Keys.NumPad2 => Key.Num2,
                Keys.NumPad3 => Key.Num3,
                Keys.NumPad4 => Key.Num4,
                Keys.NumPad5 => Key.Num5,
                Keys.NumPad6 => Key.Num6,
                Keys.NumPad7 => Key.Num7,
                Keys.NumPad8 => Key.Num8,
                Keys.NumPad9 => Key.Num9,
                Keys.Divide => Key.NumSlash,
                Keys.Multiply => Key.NumAsterisk,
                Keys.Subtract => Key.NumMinus,
                Keys.Add => Key.NumPlus,
                Keys.Up => Key.Up,
                Keys.Down => Key.Down,
                Keys.Left => Key.Left,
                Keys.Right => Key.Right,
                Keys.Oem4 => Key.LSquare,
                Keys.Oem6 => Key.RSquare,
                Keys.Oem1 => Key.Colon,
                Keys.Oem7 => Key.Quotes,
                Keys.Oem5 => Key.BackSlash,
                Keys.Oemcomma => Key.LAngle,
                Keys.OemPeriod => Key.RAngle,
                Keys.Oem2 => Key.Slash,
                _ => Key.None
            };

            return key;
        }
    }
}
