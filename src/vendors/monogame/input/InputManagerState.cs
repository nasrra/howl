using System;
using Howl.Input;

namespace Howl.Vendors.MonoGame.Input;

public class InputManagerState
{
    /// <summary>
    ///     The keyboard state information.
    /// </summary>
    public KeyboardState KeyboardState;

    /// <summary>
    ///     The mouse state information.
    /// </summary>
    public MouseState MouseState;

    /// <summary>
    ///     The gamepad state information.
    /// </summary>
    public GamePadState[] GamePadStates;

    /// <summary>
    ///     Whether this instance has been disposed.
    /// </summary>
    public bool Disposed;

    /// <summary>
    ///     Creates a new input manager instance.
    /// </summary>
    public InputManagerState()
    {
        KeyboardState = new KeyboardState();
        MouseState = new MouseState();
        GamePadStates = new GamePadState[4];
        GamePadStates[0] = new GamePadState(Microsoft.Xna.Framework.PlayerIndex.One);
        GamePadStates[1] = new GamePadState(Microsoft.Xna.Framework.PlayerIndex.Two);
        GamePadStates[2] = new GamePadState(Microsoft.Xna.Framework.PlayerIndex.Three);
        GamePadStates[3] = new GamePadState(Microsoft.Xna.Framework.PlayerIndex.Four);
    }
}