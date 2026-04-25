namespace Howl.Input;

public static class Keyboard
{
    /// <summary>
    ///     Checks if a keyboard key is down.
    /// </summary>
    /// <param name="app">the howl app instance with the input state.</param>
    /// <param name="key">the key to check.</param>
    /// <returns>true, if the key is pressed down; otherwise false.</returns>
    public static bool IsKeyDown(HowlApp app, Key key)
    {
        Microsoft.Xna.Framework.Input.Keys keys = Vendors.MonoGame.Input.KeyExtensions.ToMonoGame(key);
        return Vendors.MonoGame.Input.Keyboard.IsKeyDown(app.MonoGameAppState.InputManagerState.KeyboardState, keys);
    }

    /// <summary>
    ///     Checks if a keyboard key is up.
    /// </summary>
    /// <param name="app">the howl app instance with the input state.</param>
    /// <param name="key">the key to check.</param>
    /// <returns>true, if the key is not pressed down; otherwise false.</returns>
    public static bool IsKeyUp(HowlApp app, Key key)
    {
        Microsoft.Xna.Framework.Input.Keys keys = Vendors.MonoGame.Input.KeyExtensions.ToMonoGame(key);
        return Vendors.MonoGame.Input.Keyboard.IsKeyUp(app.MonoGameAppState.InputManagerState.KeyboardState, keys);
    }

    /// <summary>
    ///     Checks if a key has just been pressed this update cycle.
    /// </summary>
    /// <param name="app">the howl app instance with the input state.</param>
    /// <param name="key">the key to check.</param>
    /// <returns>true, if the key has just been pressed; otherwise false.</returns>
    public static bool IsKeyJustPressed(HowlApp app, Key key)
    {
        Microsoft.Xna.Framework.Input.Keys keys = Vendors.MonoGame.Input.KeyExtensions.ToMonoGame(key);
        return Vendors.MonoGame.Input.Keyboard.IsKeyJustPressed(app.MonoGameAppState.InputManagerState.KeyboardState, keys);
    }

    /// <summary>
    ///     Checks if a key has just been released this update cycle.
    /// </summary>
    /// <param name="app">the howl app instance with the input state.</param>
    /// <param name="key">the key to check.</param>
    /// <returns>true if the key has just been pressed; otheriwse false.</returns>
    public static bool IsKeyJustReleased(HowlApp app, Key key)
    {
        Microsoft.Xna.Framework.Input.Keys keys = Vendors.MonoGame.Input.KeyExtensions.ToMonoGame(key);
        return Vendors.MonoGame.Input.Keyboard.IsKeyJustReleased(app.MonoGameAppState.InputManagerState.KeyboardState, keys);
    }
}