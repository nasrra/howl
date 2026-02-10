using Howl.Math;

namespace Howl.Input;

public interface IMouse
{
    /// <summary>
    /// Gets and sets the current position of the mouse cursor; relative the window back buffer.
    /// </summary>
    public Vector2Int BackBufferPosition {get; set;}

    /// <summary>
    /// Gets or Sets the current x-coordinate position of the mouse cursor in screen-space.
    /// </summary>
    public int X{get; set;}

    /// <summary>
    /// Gets or Sets the current y-coordinate position of the mouse cursor in screen-space.
    /// </summary>
    public int Y{get; set;}

    /// <summary>
    /// Gets the difference in the mouse cursor position between the previous and current frame; relative to the window back buffer.
    /// </summary>
    public Vector2Int BackBufferPositionDelta {get;}

    /// <summary>
    /// Gets the difference in the mouse cursor x-position between the previous and current frame.
    /// </summary>
    public int XDelta {get;}

    /// <summary>
    /// Gets the difference in the mouse cursor y-position between the previous and current frame.
    /// </summary>
    public int YDelta {get;}

    /// <summary>
    /// Sets the current position - relative to the back buffer - of the mouse cursor in screen-space and updates the CurrentState with the new position.
    /// </summary>
    /// <param name="position">The position to set the mouse; in screen-space.</param>
    public void SetBackBufferPosition(Vector2Int position);

    /// <summary>
    /// Gets whether the mouse has been moved between the previous and current frame.
    /// </summary>
    public bool WasMoved {get;}

    /// <summary>
    /// Gets the cumulative value of the mouse scroll wheel since the start of the application.
    /// </summary>
    public int ScrollWheel {get;}

    /// <summary>
    /// Gets the difference in the scroll wheel value between the previous and current frame.
    /// </summary>
    public int ScrollWheelDelta {get;}

    /// <summary>
    /// Updates the state information of the mouse input;
    /// </summary>
    public void Update();

    /// <summary>
    /// Returns whether the specified mouse button is currently down.
    /// </summary>
    /// <param name="mouseButton">The specified mouse button to check.</param>
    /// <returns>true, if the mouse button is down; otherwise false.</returns>
    public bool IsButtonDown(MouseButton mouseButton);

    /// <summary>
    /// Returns whether the specified mouse button is currently up.
    /// </summary>
    /// <param name="mouseButton">The specified mouse button to check.</param>
    /// <returns>true, if the mouse button is up; otherwise false.</returns>
    public bool IsButtonUp(MouseButton mouseButton);

    /// <summary>
    /// Returns whether the specified mouse button has just been pressed.
    /// </summary>
    /// <param name="mouseButton">The specified mouse button to check.</param>
    /// <returns>true, if the mouse button has just been pressed; otherwise false.</returns>
    public bool IsButtonJustPressed(MouseButton mouseButton);

    /// <summary>
    /// Returns whether the specified mouse button has just been released.
    /// </summary>
    /// <param name="mouseButton">The specified mouse button to check.</param>
    /// <returns>true, if the mouse button has just been released; otherwise false.</returns>
    public bool IsButtonJustReleased(MouseButton mouseButton);
}