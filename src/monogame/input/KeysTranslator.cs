using System;
using Howl.Input;

namespace Howl.Monogame.Input;

public class KeysTranslator
{
    public static Microsoft.Xna.Framework.Input.Keys ToMonogameKeys(Key key)
    {
        return key switch
        {
            // Basic keys
            Key.None => Microsoft.Xna.Framework.Input.Keys.None,
            Key.Back => Microsoft.Xna.Framework.Input.Keys.Back,
            Key.Tab => Microsoft.Xna.Framework.Input.Keys.Tab,
            Key.Enter => Microsoft.Xna.Framework.Input.Keys.Enter,
            Key.CapsLock => Microsoft.Xna.Framework.Input.Keys.CapsLock,
            Key.Escape => Microsoft.Xna.Framework.Input.Keys.Escape,
            Key.Space => Microsoft.Xna.Framework.Input.Keys.Space,
            Key.PageUp => Microsoft.Xna.Framework.Input.Keys.PageUp,
            Key.PageDown => Microsoft.Xna.Framework.Input.Keys.PageDown,
            Key.End => Microsoft.Xna.Framework.Input.Keys.End,
            Key.Home => Microsoft.Xna.Framework.Input.Keys.Home,
            Key.Left => Microsoft.Xna.Framework.Input.Keys.Left,
            Key.Up => Microsoft.Xna.Framework.Input.Keys.Up,
            Key.Right => Microsoft.Xna.Framework.Input.Keys.Right,
            Key.Down => Microsoft.Xna.Framework.Input.Keys.Down,
            Key.Select => Microsoft.Xna.Framework.Input.Keys.Select,
            Key.Print => Microsoft.Xna.Framework.Input.Keys.Print,
            Key.Execute => Microsoft.Xna.Framework.Input.Keys.Execute,
            Key.PrintScreen => Microsoft.Xna.Framework.Input.Keys.PrintScreen,
            Key.Insert => Microsoft.Xna.Framework.Input.Keys.Insert,
            Key.Delete => Microsoft.Xna.Framework.Input.Keys.Delete,
            Key.Help => Microsoft.Xna.Framework.Input.Keys.Help,
            Key.Pause => Microsoft.Xna.Framework.Input.Keys.Pause,
            
            // Digit keys (0-9)
            Key.D0 => Microsoft.Xna.Framework.Input.Keys.D0,
            Key.D1 => Microsoft.Xna.Framework.Input.Keys.D1,
            Key.D2 => Microsoft.Xna.Framework.Input.Keys.D2,
            Key.D3 => Microsoft.Xna.Framework.Input.Keys.D3,
            Key.D4 => Microsoft.Xna.Framework.Input.Keys.D4,
            Key.D5 => Microsoft.Xna.Framework.Input.Keys.D5,
            Key.D6 => Microsoft.Xna.Framework.Input.Keys.D6,
            Key.D7 => Microsoft.Xna.Framework.Input.Keys.D7,
            Key.D8 => Microsoft.Xna.Framework.Input.Keys.D8,
            Key.D9 => Microsoft.Xna.Framework.Input.Keys.D9,
            
            // Letter keys (A-Z)
            Key.A => Microsoft.Xna.Framework.Input.Keys.A,
            Key.B => Microsoft.Xna.Framework.Input.Keys.B,
            Key.C => Microsoft.Xna.Framework.Input.Keys.C,
            Key.D => Microsoft.Xna.Framework.Input.Keys.D,
            Key.E => Microsoft.Xna.Framework.Input.Keys.E,
            Key.F => Microsoft.Xna.Framework.Input.Keys.F,
            Key.G => Microsoft.Xna.Framework.Input.Keys.G,
            Key.H => Microsoft.Xna.Framework.Input.Keys.H,
            Key.I => Microsoft.Xna.Framework.Input.Keys.I,
            Key.J => Microsoft.Xna.Framework.Input.Keys.J,
            Key.K => Microsoft.Xna.Framework.Input.Keys.K,
            Key.L => Microsoft.Xna.Framework.Input.Keys.L,
            Key.M => Microsoft.Xna.Framework.Input.Keys.M,
            Key.N => Microsoft.Xna.Framework.Input.Keys.N,
            Key.O => Microsoft.Xna.Framework.Input.Keys.O,
            Key.P => Microsoft.Xna.Framework.Input.Keys.P,
            Key.Q => Microsoft.Xna.Framework.Input.Keys.Q,
            Key.R => Microsoft.Xna.Framework.Input.Keys.R,
            Key.S => Microsoft.Xna.Framework.Input.Keys.S,
            Key.T => Microsoft.Xna.Framework.Input.Keys.T,
            Key.U => Microsoft.Xna.Framework.Input.Keys.U,
            Key.V => Microsoft.Xna.Framework.Input.Keys.V,
            Key.W => Microsoft.Xna.Framework.Input.Keys.W,
            Key.X => Microsoft.Xna.Framework.Input.Keys.X,
            Key.Y => Microsoft.Xna.Framework.Input.Keys.Y,
            Key.Z => Microsoft.Xna.Framework.Input.Keys.Z,
            
            // Windows keys
            Key.LeftMeta => Microsoft.Xna.Framework.Input.Keys.LeftWindows,
            Key.RightMeta => Microsoft.Xna.Framework.Input.Keys.RightWindows,
            Key.Menu => Microsoft.Xna.Framework.Input.Keys.Apps,
            
            // NumPad keys
            Key.NP0 => Microsoft.Xna.Framework.Input.Keys.NumPad0,
            Key.NP1 => Microsoft.Xna.Framework.Input.Keys.NumPad1,
            Key.NP2 => Microsoft.Xna.Framework.Input.Keys.NumPad2,
            Key.NP3 => Microsoft.Xna.Framework.Input.Keys.NumPad3,
            Key.NP4 => Microsoft.Xna.Framework.Input.Keys.NumPad4,
            Key.NP5 => Microsoft.Xna.Framework.Input.Keys.NumPad5,
            Key.NP6 => Microsoft.Xna.Framework.Input.Keys.NumPad6,
            Key.NP7 => Microsoft.Xna.Framework.Input.Keys.NumPad7,
            Key.NP8 => Microsoft.Xna.Framework.Input.Keys.NumPad8,
            Key.NP9 => Microsoft.Xna.Framework.Input.Keys.NumPad9,
            
            // NumPad operators
            Key.NPMultiply => Microsoft.Xna.Framework.Input.Keys.Multiply,
            Key.NPAdd => Microsoft.Xna.Framework.Input.Keys.Add,
            Key.NPSeparator => Microsoft.Xna.Framework.Input.Keys.Separator,
            Key.NPSubtract => Microsoft.Xna.Framework.Input.Keys.Subtract,
            Key.NPDecimal => Microsoft.Xna.Framework.Input.Keys.Decimal,
            Key.NPDivide => Microsoft.Xna.Framework.Input.Keys.Divide,
            
            // Function keys (F1-F24)
            Key.F1 => Microsoft.Xna.Framework.Input.Keys.F1,
            Key.F2 => Microsoft.Xna.Framework.Input.Keys.F2,
            Key.F3 => Microsoft.Xna.Framework.Input.Keys.F3,
            Key.F4 => Microsoft.Xna.Framework.Input.Keys.F4,
            Key.F5 => Microsoft.Xna.Framework.Input.Keys.F5,
            Key.F6 => Microsoft.Xna.Framework.Input.Keys.F6,
            Key.F7 => Microsoft.Xna.Framework.Input.Keys.F7,
            Key.F8 => Microsoft.Xna.Framework.Input.Keys.F8,
            Key.F9 => Microsoft.Xna.Framework.Input.Keys.F9,
            Key.F10 => Microsoft.Xna.Framework.Input.Keys.F10,
            Key.F11 => Microsoft.Xna.Framework.Input.Keys.F11,
            Key.F12 => Microsoft.Xna.Framework.Input.Keys.F12,
            Key.F13 => Microsoft.Xna.Framework.Input.Keys.F13,
            Key.F14 => Microsoft.Xna.Framework.Input.Keys.F14,
            Key.F15 => Microsoft.Xna.Framework.Input.Keys.F15,
            Key.F16 => Microsoft.Xna.Framework.Input.Keys.F16,
            Key.F17 => Microsoft.Xna.Framework.Input.Keys.F17,
            Key.F18 => Microsoft.Xna.Framework.Input.Keys.F18,
            Key.F19 => Microsoft.Xna.Framework.Input.Keys.F19,
            Key.F20 => Microsoft.Xna.Framework.Input.Keys.F20,
            Key.F21 => Microsoft.Xna.Framework.Input.Keys.F21,
            Key.F22 => Microsoft.Xna.Framework.Input.Keys.F22,
            Key.F23 => Microsoft.Xna.Framework.Input.Keys.F23,
            Key.F24 => Microsoft.Xna.Framework.Input.Keys.F24,
            
            // Lock keys
            Key.NumLock => Microsoft.Xna.Framework.Input.Keys.NumLock,
            Key.Scroll => Microsoft.Xna.Framework.Input.Keys.Scroll,
            
            // Modifier keys
            Key.LeftShift => Microsoft.Xna.Framework.Input.Keys.LeftShift,
            Key.RightShift => Microsoft.Xna.Framework.Input.Keys.RightShift,
            Key.LeftControl => Microsoft.Xna.Framework.Input.Keys.LeftControl,
            Key.RightControl => Microsoft.Xna.Framework.Input.Keys.RightControl,
            Key.LeftAlt => Microsoft.Xna.Framework.Input.Keys.LeftAlt,
            Key.RightAlt => Microsoft.Xna.Framework.Input.Keys.RightAlt,
            
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