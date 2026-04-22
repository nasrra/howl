namespace Howl.Vendors.MonoGame.Input;

public class MouseState
{
    /// <summary>
    ///     The state of the mouse input during the previous update cycle.
    /// </summary>
    public Microsoft.Xna.Framework.Input.MouseState PreviousState;

    /// <summary>
    ///     The state of the mouse input during the current update cycle.
    /// </summary>
    public Microsoft.Xna.Framework.Input.MouseState CurrentState;
    
    /// <summary>
    ///     Creates a new Monogame Mouse instance.
    /// </summary>
    public MouseState()
    {
        CurrentState = Microsoft.Xna.Framework.Input.Mouse.GetState();
        PreviousState = new Microsoft.Xna.Framework.Input.MouseState();
    }
}