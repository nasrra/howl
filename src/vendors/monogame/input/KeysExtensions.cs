using System;
using System.Runtime.CompilerServices;

namespace Howl.Vendors.MonoGame.Input;

public static class KeyExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static Microsoft.Xna.Framework.Input.Keys ToMonoGame(this Howl.Input.Key key)
    {
        return key switch
        {
            // Basic keys
            Howl.Input.Key.None => Microsoft.Xna.Framework.Input.Keys.None,
            Howl.Input.Key.Back => Microsoft.Xna.Framework.Input.Keys.Back,
            Howl.Input.Key.Tab => Microsoft.Xna.Framework.Input.Keys.Tab,
            Howl.Input.Key.Enter => Microsoft.Xna.Framework.Input.Keys.Enter,
            Howl.Input.Key.CapsLock => Microsoft.Xna.Framework.Input.Keys.CapsLock,
            Howl.Input.Key.Escape => Microsoft.Xna.Framework.Input.Keys.Escape,
            Howl.Input.Key.Space => Microsoft.Xna.Framework.Input.Keys.Space,
            Howl.Input.Key.PageUp => Microsoft.Xna.Framework.Input.Keys.PageUp,
            Howl.Input.Key.PageDown => Microsoft.Xna.Framework.Input.Keys.PageDown,
            Howl.Input.Key.End => Microsoft.Xna.Framework.Input.Keys.End,
            Howl.Input.Key.Home => Microsoft.Xna.Framework.Input.Keys.Home,
            Howl.Input.Key.Left => Microsoft.Xna.Framework.Input.Keys.Left,
            Howl.Input.Key.Up => Microsoft.Xna.Framework.Input.Keys.Up,
            Howl.Input.Key.Right => Microsoft.Xna.Framework.Input.Keys.Right,
            Howl.Input.Key.Down => Microsoft.Xna.Framework.Input.Keys.Down,
            Howl.Input.Key.Select => Microsoft.Xna.Framework.Input.Keys.Select,
            Howl.Input.Key.Print => Microsoft.Xna.Framework.Input.Keys.Print,
            Howl.Input.Key.Execute => Microsoft.Xna.Framework.Input.Keys.Execute,
            Howl.Input.Key.PrintScreen => Microsoft.Xna.Framework.Input.Keys.PrintScreen,
            Howl.Input.Key.Insert => Microsoft.Xna.Framework.Input.Keys.Insert,
            Howl.Input.Key.Delete => Microsoft.Xna.Framework.Input.Keys.Delete,
            Howl.Input.Key.Help => Microsoft.Xna.Framework.Input.Keys.Help,
            Howl.Input.Key.Pause => Microsoft.Xna.Framework.Input.Keys.Pause,

            // Digit keys (0-9)
            Howl.Input.Key.D0 => Microsoft.Xna.Framework.Input.Keys.D0,
            Howl.Input.Key.D1 => Microsoft.Xna.Framework.Input.Keys.D1,
            Howl.Input.Key.D2 => Microsoft.Xna.Framework.Input.Keys.D2,
            Howl.Input.Key.D3 => Microsoft.Xna.Framework.Input.Keys.D3,
            Howl.Input.Key.D4 => Microsoft.Xna.Framework.Input.Keys.D4,
            Howl.Input.Key.D5 => Microsoft.Xna.Framework.Input.Keys.D5,
            Howl.Input.Key.D6 => Microsoft.Xna.Framework.Input.Keys.D6,
            Howl.Input.Key.D7 => Microsoft.Xna.Framework.Input.Keys.D7,
            Howl.Input.Key.D8 => Microsoft.Xna.Framework.Input.Keys.D8,
            Howl.Input.Key.D9 => Microsoft.Xna.Framework.Input.Keys.D9,

            // Letter keys (A-Z)
            Howl.Input.Key.A => Microsoft.Xna.Framework.Input.Keys.A,
            Howl.Input.Key.B => Microsoft.Xna.Framework.Input.Keys.B,
            Howl.Input.Key.C => Microsoft.Xna.Framework.Input.Keys.C,
            Howl.Input.Key.D => Microsoft.Xna.Framework.Input.Keys.D,
            Howl.Input.Key.E => Microsoft.Xna.Framework.Input.Keys.E,
            Howl.Input.Key.F => Microsoft.Xna.Framework.Input.Keys.F,
            Howl.Input.Key.G => Microsoft.Xna.Framework.Input.Keys.G,
            Howl.Input.Key.H => Microsoft.Xna.Framework.Input.Keys.H,
            Howl.Input.Key.I => Microsoft.Xna.Framework.Input.Keys.I,
            Howl.Input.Key.J => Microsoft.Xna.Framework.Input.Keys.J,
            Howl.Input.Key.K => Microsoft.Xna.Framework.Input.Keys.K,
            Howl.Input.Key.L => Microsoft.Xna.Framework.Input.Keys.L,
            Howl.Input.Key.M => Microsoft.Xna.Framework.Input.Keys.M,
            Howl.Input.Key.N => Microsoft.Xna.Framework.Input.Keys.N,
            Howl.Input.Key.O => Microsoft.Xna.Framework.Input.Keys.O,
            Howl.Input.Key.P => Microsoft.Xna.Framework.Input.Keys.P,
            Howl.Input.Key.Q => Microsoft.Xna.Framework.Input.Keys.Q,
            Howl.Input.Key.R => Microsoft.Xna.Framework.Input.Keys.R,
            Howl.Input.Key.S => Microsoft.Xna.Framework.Input.Keys.S,
            Howl.Input.Key.T => Microsoft.Xna.Framework.Input.Keys.T,
            Howl.Input.Key.U => Microsoft.Xna.Framework.Input.Keys.U,
            Howl.Input.Key.V => Microsoft.Xna.Framework.Input.Keys.V,
            Howl.Input.Key.W => Microsoft.Xna.Framework.Input.Keys.W,
            Howl.Input.Key.X => Microsoft.Xna.Framework.Input.Keys.X,
            Howl.Input.Key.Y => Microsoft.Xna.Framework.Input.Keys.Y,
            Howl.Input.Key.Z => Microsoft.Xna.Framework.Input.Keys.Z,

            // Windows keys
            Howl.Input.Key.LeftMeta => Microsoft.Xna.Framework.Input.Keys.LeftWindows,
            Howl.Input.Key.RightMeta => Microsoft.Xna.Framework.Input.Keys.RightWindows,
            Howl.Input.Key.Menu => Microsoft.Xna.Framework.Input.Keys.Apps,

            // NumPad keys
            Howl.Input.Key.NP0 => Microsoft.Xna.Framework.Input.Keys.NumPad0,
            Howl.Input.Key.NP1 => Microsoft.Xna.Framework.Input.Keys.NumPad1,
            Howl.Input.Key.NP2 => Microsoft.Xna.Framework.Input.Keys.NumPad2,
            Howl.Input.Key.NP3 => Microsoft.Xna.Framework.Input.Keys.NumPad3,
            Howl.Input.Key.NP4 => Microsoft.Xna.Framework.Input.Keys.NumPad4,
            Howl.Input.Key.NP5 => Microsoft.Xna.Framework.Input.Keys.NumPad5,
            Howl.Input.Key.NP6 => Microsoft.Xna.Framework.Input.Keys.NumPad6,
            Howl.Input.Key.NP7 => Microsoft.Xna.Framework.Input.Keys.NumPad7,
            Howl.Input.Key.NP8 => Microsoft.Xna.Framework.Input.Keys.NumPad8,
            Howl.Input.Key.NP9 => Microsoft.Xna.Framework.Input.Keys.NumPad9,

            // NumPad operators
            Howl.Input.Key.NPMultiply => Microsoft.Xna.Framework.Input.Keys.Multiply,
            Howl.Input.Key.NPAdd => Microsoft.Xna.Framework.Input.Keys.Add,
            Howl.Input.Key.NPSeparator => Microsoft.Xna.Framework.Input.Keys.Separator,
            Howl.Input.Key.NPSubtract => Microsoft.Xna.Framework.Input.Keys.Subtract,
            Howl.Input.Key.NPDecimal => Microsoft.Xna.Framework.Input.Keys.Decimal,
            Howl.Input.Key.NPDivide => Microsoft.Xna.Framework.Input.Keys.Divide,

            // Function keys (F1-F24)
            Howl.Input.Key.F1 => Microsoft.Xna.Framework.Input.Keys.F1,
            Howl.Input.Key.F2 => Microsoft.Xna.Framework.Input.Keys.F2,
            Howl.Input.Key.F3 => Microsoft.Xna.Framework.Input.Keys.F3,
            Howl.Input.Key.F4 => Microsoft.Xna.Framework.Input.Keys.F4,
            Howl.Input.Key.F5 => Microsoft.Xna.Framework.Input.Keys.F5,
            Howl.Input.Key.F6 => Microsoft.Xna.Framework.Input.Keys.F6,
            Howl.Input.Key.F7 => Microsoft.Xna.Framework.Input.Keys.F7,
            Howl.Input.Key.F8 => Microsoft.Xna.Framework.Input.Keys.F8,
            Howl.Input.Key.F9 => Microsoft.Xna.Framework.Input.Keys.F9,
            Howl.Input.Key.F10 => Microsoft.Xna.Framework.Input.Keys.F10,
            Howl.Input.Key.F11 => Microsoft.Xna.Framework.Input.Keys.F11,
            Howl.Input.Key.F12 => Microsoft.Xna.Framework.Input.Keys.F12,
            Howl.Input.Key.F13 => Microsoft.Xna.Framework.Input.Keys.F13,
            Howl.Input.Key.F14 => Microsoft.Xna.Framework.Input.Keys.F14,
            Howl.Input.Key.F15 => Microsoft.Xna.Framework.Input.Keys.F15,
            Howl.Input.Key.F16 => Microsoft.Xna.Framework.Input.Keys.F16,
            Howl.Input.Key.F17 => Microsoft.Xna.Framework.Input.Keys.F17,
            Howl.Input.Key.F18 => Microsoft.Xna.Framework.Input.Keys.F18,
            Howl.Input.Key.F19 => Microsoft.Xna.Framework.Input.Keys.F19,
            Howl.Input.Key.F20 => Microsoft.Xna.Framework.Input.Keys.F20,
            Howl.Input.Key.F21 => Microsoft.Xna.Framework.Input.Keys.F21,
            Howl.Input.Key.F22 => Microsoft.Xna.Framework.Input.Keys.F22,
            Howl.Input.Key.F23 => Microsoft.Xna.Framework.Input.Keys.F23,
            Howl.Input.Key.F24 => Microsoft.Xna.Framework.Input.Keys.F24,

            // Lock keys
            Howl.Input.Key.NumLock => Microsoft.Xna.Framework.Input.Keys.NumLock,
            Howl.Input.Key.Scroll => Microsoft.Xna.Framework.Input.Keys.Scroll,

            // Modifier keys
            Howl.Input.Key.LeftShift => Microsoft.Xna.Framework.Input.Keys.LeftShift,
            Howl.Input.Key.RightShift => Microsoft.Xna.Framework.Input.Keys.RightShift,
            Howl.Input.Key.LeftControl => Microsoft.Xna.Framework.Input.Keys.LeftControl,
            Howl.Input.Key.RightControl => Microsoft.Xna.Framework.Input.Keys.RightControl,
            Howl.Input.Key.LeftAlt => Microsoft.Xna.Framework.Input.Keys.LeftAlt,
            Howl.Input.Key.RightAlt => Microsoft.Xna.Framework.Input.Keys.RightAlt,

            // // Browser keys
            // Key.BrowserBack => Microsoft.Xna.Framework.Input.Keys.BrowserBack,
            // Key.BrowserForward => Microsoft.Xna.Framework.Input.Keys.BrowserForward,
            // Key.BrowserRefresh => Microsoft.Xna.Framework.Input.Keys.BrowserRefresh,
            // Key.BrowserStop => Microsoft.Xna.Framework.Input.Keys.BrowserStop,
            // Key.BrowserSearch => Microsoft.Xna.Framework.Input.Keys.BrowserSearch,
            // Key.BrowserFavorites => Microsoft.Xna.Framework.Input.Keys.BrowserFavorites,
            // Key.BrowserHome => Microsoft.Xna.Framework.Input.Keys.BrowserHome,

            // // Media keys
            // Key.VolumeMute => Microsoft.Xna.Framework.Input.Keys.VolumeMute,
            // Key.VolumeDown => Microsoft.Xna.Framework.Input.Keys.VolumeDown,
            // Key.VolumeUp => Microsoft.Xna.Framework.Input.Keys.VolumeUp,
            // Key.MediaNextTrack => Microsoft.Xna.Framework.Input.Keys.MediaNextTrack,
            // Key.MediaPreviousTrack => Microsoft.Xna.Framework.Input.Keys.MediaPreviousTrack,
            // Key.MediaStop => Microsoft.Xna.Framework.Input.Keys.MediaStop,
            // Key.MediaPlayPause => Microsoft.Xna.Framework.Input.Keys.MediaPlayPause,

            // // Application keys
            // Key.LaunchMail => Microsoft.Xna.Framework.Input.Keys.LaunchMail,
            // Key.SelectMedia => Microsoft.Xna.Framework.Input.Keys.SelectMedia,
            // Key.LaunchApplication1 => Microsoft.Xna.Framework.Input.Keys.LaunchApplication1,
            // Key.LaunchApplication2 => Microsoft.Xna.Framework.Input.Keys.LaunchApplication2,

            // // OEM keys
            // Key.OemSemicolon => Microsoft.Xna.Framework.Input.Keys.OemSemicolon,
            // Key.OemPlus => Microsoft.Xna.Framework.Input.Keys.OemPlus,
            // Key.OemComma => Microsoft.Xna.Framework.Input.Keys.OemComma,
            // Key.OemMinus => Microsoft.Xna.Framework.Input.Keys.OemMinus,
            // Key.OemPeriod => Microsoft.Xna.Framework.Input.Keys.OemPeriod,
            // Key.OemQuestion => Microsoft.Xna.Framework.Input.Keys.OemQuestion,
            // Key.OemTilde => Microsoft.Xna.Framework.Input.Keys.OemTilde,
            // Key.OemOpenBrackets => Microsoft.Xna.Framework.Input.Keys.OemOpenBrackets,
            // Key.OemPipe => Microsoft.Xna.Framework.Input.Keys.OemPipe,
            // Key.OemCloseBrackets => Microsoft.Xna.Framework.Input.Keys.OemCloseBrackets,
            // Key.OemQuotes => Microsoft.Xna.Framework.Input.Keys.OemQuotes,
            // Key.Oem8 => Microsoft.Xna.Framework.Input.Keys.Oem8,
            // Key.OemBackslash => Microsoft.Xna.Framework.Input.Keys.OemBackslash,

            // // IME/International keys
            // Key.ImeConvert => Microsoft.Xna.Framework.Input.Keys.ImeConvert,
            // Key.ImeNoConvert => Microsoft.Xna.Framework.Input.Keys.ImeNoConvert,
            // Key.Kana => Microsoft.Xna.Framework.Input.Keys.Kana,
            // Key.Kanji => Microsoft.Xna.Framework.Input.Keys.Kanji,

            // // Special keys
            // Key.Sleep => Microsoft.Xna.Framework.Input.Keys.Sleep,
            // Key.ProcessKey => Microsoft.Xna.Framework.Input.Keys.ProcessKey,
            // Key.Attn => Microsoft.Xna.Framework.Input.Keys.Attn,
            // Key.Crsel => Microsoft.Xna.Framework.Input.Keys.Crsel,
            // Key.Exsel => Microsoft.Xna.Framework.Input.Keys.Exsel,
            // Key.EraseEof => Microsoft.Xna.Framework.Input.Keys.EraseEof,
            // Key.Play => Microsoft.Xna.Framework.Input.Keys.Play,
            // Key.Zoom => Microsoft.Xna.Framework.Input.Keys.Zoom,
            // Key.Pa1 => Microsoft.Xna.Framework.Input.Keys.Pa1,
            // Key.OemClear => Microsoft.Xna.Framework.Input.Keys.OemClear,

            // // ChatPad keys
            // Key.ChatPadGreen => Microsoft.Xna.Framework.Input.Keys.ChatPadGreen,
            // Key.ChatPadOrange => Microsoft.Xna.Framework.Input.Keys.ChatPadOrange,

            // // Additional OEM keys
            // Key.OemAuto => Microsoft.Xna.Framework.Input.Keys.OemAuto,
            // Key.OemCopy => Microsoft.Xna.Framework.Input.Keys.OemCopy,
            // Key.OemEnlW => Microsoft.Xna.Framework.Input.Keys.OemEnlW,

            _ => throw new InvalidOperationException($"Monogame Keyboard is not mapped to key {key}")
        };
    }
}
