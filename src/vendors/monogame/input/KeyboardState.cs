namespace Howl.Vendors.MonoGame.Input;

public class KeyboardState
{
    /// <summary>
    ///     The state of the keyboard input dureing the previous update cycle.
    /// </summary>
    public Microsoft.Xna.Framework.Input.KeyboardState PreviousState;

    /// <summary>
    ///     The state of the keyboard input durring the current update cycle.
    /// </summary>
    public Microsoft.Xna.Framework.Input.KeyboardState CurrentState;

    /// <summary>
    ///     Whether this instance has been diposed of.
    /// </summary>
    public bool Disposed;

    /// <summary>
    ///     Creates a new KeyboardState instance.
    /// </summary>
    public KeyboardState()
    {
        PreviousState = new();
        CurrentState = new();
    }
}