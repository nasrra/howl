using System;
using Howl.Input;
using Microsoft.Xna.Framework;

namespace Howl.Vendors.MonoGame.Input;

public class PlayerIndexExtensions
{
    public static PlayerIndex ToMonoGame(GamePadId gamepadId)
    {
        return gamepadId switch
        {
            GamePadId.One => PlayerIndex.One,
            GamePadId.Two => PlayerIndex.Two,
            GamePadId.Three => PlayerIndex.Three,
            GamePadId.Four => PlayerIndex.Four,
            _ => throw new InvalidOperationException($"Monogame does not support player index for {gamepadId}"),
        };
    }
}