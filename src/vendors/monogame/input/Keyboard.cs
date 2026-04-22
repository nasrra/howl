using Howl.Input;

namespace Howl.Vendors.MonoGame.Input;

public class Keyboard
{

    /// <summary>
    ///     Updates the state information for a keyboard.
    /// </summary>
    /// <param name="state">the keyboard state to update.</param>
    public static void Update(KeyboardState state)
    {
        state.PreviousState = state.CurrentState;
        state.CurrentState = Microsoft.Xna.Framework.Input.Keyboard.GetState();
    }

    /// <summary>
    ///     Checks whether a specified key is down.
    /// </summary>
    /// <param name="state">the keyboard state to check.</param>
    /// <param name="key">the specified keycode.</param>
    /// <returns>true, if the key is down; otherwise false.</returns>
    public bool IsKeyDown(KeyboardState state, Microsoft.Xna.Framework.Input.Keys key)
    {
        return state.CurrentState.IsKeyDown(key);
    }

    /// <summary>
    ///     Checks whether the specified key is up.
    /// </summary>
    /// <param name="state">the keyboard state to check.</param>
    /// <param name="key">the specified keycode.</param>
    /// <returns>true, if the key is up; otherwise false. </returns>
    public bool IsKeyUp(KeyboardState state, Microsoft.Xna.Framework.Input.Keys key)
    {
        return state.CurrentState.IsKeyDown(key);
    }

    /// <summary>
    ///     Checks whether the specified key has just been pressed.
    /// </summary>
    /// <param name="state">the keyboard state to check.</param>
    /// <param name="key">the specified keycode.</param>
    /// <returns>true, if the key has just been pressed; otherwise false.</returns>
    public bool IsKeyJustPressed(KeyboardState state, Microsoft.Xna.Framework.Input.Keys key)
    {
        return state.PreviousState.IsKeyUp(key) && state.CurrentState.IsKeyDown(key);
    }

    /// <summary>
    ///     Checks whether the specified key has just been released.
    /// </summary>
    /// <param name="state">the keyboard state to check.</param>
    /// <param name="key">the specified keycode.</param>
    /// <returns>true, if the key has just been released; otherwise false.</returns>
    public bool IsKeyJustReleased(KeyboardState state, Microsoft.Xna.Framework.Input.Keys key)
    {
        return state.PreviousState.IsKeyDown(key) && state.CurrentState.IsKeyUp(key);
    }

}