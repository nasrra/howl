using System;
using Howl.Input;
using Microsoft.Xna.Framework.Input;

namespace Howl.Vendors.MonoGame.Input;

public static class ButtonsExtensions{
    public static Buttons ToMonoGame(this GamePadButton gamepadButton)
    {
        return gamepadButton switch
        {
            // Dpad buttons.
            GamePadButton.DpadNorth => Buttons.DPadUp,
            GamePadButton.DpadEast  => Buttons.DPadRight,
            GamePadButton.DpadSouth => Buttons.DPadDown,
            GamePadButton.DpadWest  => Buttons.DPadLeft,
            
            // Face buttons,
            // Note: Monogame/Xna maps to a xbox controller layout
            // (todo) test Switch Pro controller; may need manual adjusting.

            GamePadButton.FaceNorth => Buttons.Y,
            GamePadButton.FaceEast => Buttons.B,
            GamePadButton.FaceSouth => Buttons.A,
            GamePadButton.FaceWest => Buttons.X,

            // Shoulders.
            GamePadButton.ShoulderRight => Buttons.RightShoulder,
            GamePadButton.ShoulderLeft => Buttons.LeftShoulder,

            // Triggers.
            GamePadButton.TriggerRight => Buttons.RightTrigger,
            GamePadButton.TriggerLeft => Buttons.LeftTrigger,

            // Special.
            GamePadButton.Start => Buttons.Start,
            GamePadButton.Menu => Buttons.Back,

            // Thumbsticks.
            GamePadButton.LeftThumbstick => Buttons.LeftStick,
            GamePadButton.RightThumbstick => Buttons.RightStick,
            _ => throw new InvalidOperationException($"Monogame GamePad is not mapped to {gamepadButton}")
        };

    }
}