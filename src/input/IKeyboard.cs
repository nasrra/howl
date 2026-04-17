namespace Howl.Input;

public interface IKeyboard
{
    /// <summary>
    /// Updates the state information about keyboard input.
    /// </summary>
    public void Update();

    /// <summary>
    /// Returns whether the specified key is down.
    /// </summary>
    /// <param name="key">The specified keycode.</param>
    /// <returns>true, if the key is currently down; otherwise false.</returns>
    public bool IsKeyDown(Key key);

    /// <summary>
    /// Returns whether the specified key is up.
    /// </summary>
    /// <param name="key">The specified keycode.</param>
    /// <returns>true, if the key is currently up, otherwise false.</returns>
    public bool IsKeyUp(Key key);

    /// <summary>
    /// Returns whether the specified key has just been pressed.
    /// </summary>
    /// <param name="key">The specified keycode.</param>
    /// <returns>true, if the key has just been pressed; otherwise false.</returns>
    public bool IsKeyJustPressed(Key key);

    /// <summary>
    /// Returns whether the specified key has just been released.
    /// </summary>
    /// <param name="key">The specified keycode.</param>
    /// <returns>true, if the key has just been release; otherwise false.</returns>
    public bool IsKeyJustReleased(Key key);
}