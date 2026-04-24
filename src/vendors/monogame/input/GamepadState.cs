using System;
using Howl.Input;

namespace Howl.Vendors.MonoGame.Input;

public class GamePadState
{
    /// <summary>
    ///     The gamepad index.
    /// </summary>
    public Microsoft.Xna.Framework.PlayerIndex PlayerIndex;
    
    public Microsoft.Xna.Framework.Input.GamePadState CurrentState;
    
    public Microsoft.Xna.Framework.Input.GamePadState PreviousState;

    /// <summary>
    ///     Whether this instance has been diposed of.
    /// </summary>
    public bool Disposed;

    /// <summary>
    ///     Creates a new game pad state instance.
    /// </summary>
    /// <param name="playerIndex">the gamepad/player index.</param>
    public GamePadState(Microsoft.Xna.Framework.PlayerIndex playerIndex)
    {
        PlayerIndex = playerIndex;
        CurrentState = Microsoft.Xna.Framework.Input.GamePad.GetState(playerIndex);
        PreviousState = new Microsoft.Xna.Framework.Input.GamePadState();
    } 
}