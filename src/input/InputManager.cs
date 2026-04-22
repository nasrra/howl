using Howl.Math;

namespace Howl.Input;

public static class InputManager
{
    /// <summary>
    ///     Checks if a mouse button is down.
    /// </summary>
    /// <param name="app">the howl app with the input state.</param>
    /// <param name="button">the mouse button to check.</param>
    /// <returns>true, if the mouse button is down; otherwise false.</returns>
    public static bool IsMouseButtonDown(HowlApp app, MouseButton button)
    {
        switch (button)
        {
            case MouseButton.Left:
                return Vendors.MonoGame.Input.Mouse.IsLeftButtonDown(app.MonoGameApp.Mouse);
            case MouseButton.Right:
                return Vendors.MonoGame.Input.Mouse.IsRightButtonDown(app.MonoGameApp.Mouse);
            case MouseButton.Middle:
                return Vendors.MonoGame.Input.Mouse.IsMiddleButtonDown(app.MonoGameApp.Mouse);
            case MouseButton.XButton1:
                return Vendors.MonoGame.Input.Mouse.IsXButton1Down(app.MonoGameApp.Mouse);
            case MouseButton.XButton2:
                return Vendors.MonoGame.Input.Mouse.IsXButton2Down(app.MonoGameApp.Mouse);
        }
        return false;
    }

    /// <summary>
    ///     Checks if a mouse button is up.
    /// </summary>
    /// <param name="app">the howl app with the input state.</param>
    /// <param name="button">the mouse button to check.</param>
    /// <returns>true, if the mouse button is up; otherwise false.</returns>
    public static bool IsMouseButtonUp(HowlApp app, MouseButton button)
    {
        switch (button)
        {
            case MouseButton.Left:
                return Vendors.MonoGame.Input.Mouse.IsLeftButtonUp(app.MonoGameApp.Mouse);
            case MouseButton.Right:
                return Vendors.MonoGame.Input.Mouse.IsRightButtonUp(app.MonoGameApp.Mouse);
            case MouseButton.Middle:
                return Vendors.MonoGame.Input.Mouse.IsMiddleButtonUp(app.MonoGameApp.Mouse);
            case MouseButton.XButton1:
                return Vendors.MonoGame.Input.Mouse.IsXButton1Up(app.MonoGameApp.Mouse);
            case MouseButton.XButton2:
                return Vendors.MonoGame.Input.Mouse.IsXButton2Up(app.MonoGameApp.Mouse);
        }
        return false;
    }

    /// <summary>
    ///     Checks if a mouse button has just been pressed.
    /// </summary>
    /// <param name="app">the howl app with the input state.</param>
    /// <param name="button">the mouse button to check.</param>
    /// <returns>true, if the mouse button has just been pressed; otherwise false.</returns>
    public static bool IsMouseButtonJustPressed(HowlApp app, MouseButton button)
    {
        switch (button)
        {
            case MouseButton.Left:
                return Vendors.MonoGame.Input.Mouse.IsLeftButtonJustPressed(app.MonoGameApp.Mouse);
            case MouseButton.Right:
                return Vendors.MonoGame.Input.Mouse.IsRightButtonJustPressed(app.MonoGameApp.Mouse);
            case MouseButton.Middle:
                return Vendors.MonoGame.Input.Mouse.IsMiddleButtonJustPressed(app.MonoGameApp.Mouse);
            case MouseButton.XButton1:
                return Vendors.MonoGame.Input.Mouse.IsXButton1JustPressed(app.MonoGameApp.Mouse);
            case MouseButton.XButton2:
                return Vendors.MonoGame.Input.Mouse.IsXButton2JustPressed(app.MonoGameApp.Mouse);
        }
        return false;        
    }

    /// <summary>
    ///     Checks if a mouse button has just been released.
    /// </summary>
    /// <param name="app">the howl app with the input state.</param>
    /// <param name="button">the mouse button to check.</param>
    /// <returns>true, if the mouse button has just been released; otherwise false.</returns>
    public static bool IsMouseButtonJustReleased(HowlApp app, MouseButton button)
    {
        switch (button)
        {
            case MouseButton.Left:
                return Vendors.MonoGame.Input.Mouse.IsLeftButtonJustReleased(app.MonoGameApp.Mouse);
            case MouseButton.Right:
                return Vendors.MonoGame.Input.Mouse.IsRightButtonJustReleased(app.MonoGameApp.Mouse);
            case MouseButton.Middle:
                return Vendors.MonoGame.Input.Mouse.IsMiddleButtonJustReleased(app.MonoGameApp.Mouse);
            case MouseButton.XButton1:
                return Vendors.MonoGame.Input.Mouse.IsXButton1JustReleased(app.MonoGameApp.Mouse);
            case MouseButton.XButton2:
                return Vendors.MonoGame.Input.Mouse.IsXButton2JustReleased(app.MonoGameApp.Mouse);
        }
        return false;                
    }

    /// <summary>
    ///     Gets the mouse position, relative to a destination render target.
    /// </summary>
    /// <param name="mouseBackBufferPos">the mouse position on the back buffer.</param>
    /// <param name="destResolution">the resolution of the destination render target.</param>
    /// <param name="destRectPos">the position of the destination rectangle on the back buffer.</param>
    /// <param name="destRectWidth">the width of the destination rectangle.</param>
    /// <param name="destRectHeight">the height of the destination rectangle.</param>
    /// <returns></returns>
    public static Vector2 GetPositionRelative(Vector2 mouseBackBufferPos, Vector2 destResolution, Vector2 destRectPos, int destRectWidth, int destRectHeight)
    {
        // get the distance from the destination rect to the mouse position. 
        float x = mouseBackBufferPos.X - destRectPos.X;
        float y = mouseBackBufferPos.Y - destRectPos.Y;

        // normalise the value between zero and one, to find
        // how far into the destination rect the mouse is. 
        // - x/y is less than zero, the mouse is outside the destinationRectangle to the left.
        // - x/y is greater than zero, the mouse is outside the destinationRectangle to the right.
        x /= destRectWidth;
        y /= destRectHeight;

        // you may need to clamp between 0 and 1 here...

        // bring into the destination resolution coordinate space.
        x *= destResolution.X;
        y *= destResolution.Y; // negative here as howl renders in y+ = up coordinate space; not y+ is down.

        return new Vector2((int)x, (int)y);
    }
}