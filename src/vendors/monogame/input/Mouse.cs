using Howl.Graphics;
using Howl.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;

namespace Howl.Vendors.MonoGame.Input;

public static class Mouse
{
    /// <summary>
    ///     The difference in the mouse cursor position between the previous and current frame; relative to the window back buffer.
    /// </summary>
    public static Vector2 GetBackBufferPosition(MouseState state)
    {
        return new (state.CurrentState.Position.X, state.CurrentState.Position.Y);
    }

    /// <summary>
    ///     Gets the difference in the mouse position between the previous and current frame.
    /// </summary>
    /// <param name="state">the mouse instance get the difference from.</param>
    /// <returns>the delta position of the mouse.</returns>
    public static Vector2 GetBackBufferPositionDelta(MouseState state)
    {
        Point point = state.CurrentState.Position - state.PreviousState.Position;
        return new(point.X, point.Y);
    }

    /// <summary>
    ///     Gets whether or not the mouse has been moved.
    /// </summary>
    /// <param name="state">the mouse instance to check.</param>
    /// <returns>true, if the mouse has moved; otherwise false.</returns>
    public static bool WasMoved(MouseState state)
    {
        return GetBackBufferPositionDelta(state) != Vector2.Zero;
    }

    /// <summary>
    ///     Gets te cumulative value of the mouse scroll wheel since the application started.
    /// </summary>
    /// <param name="state">the mouse instance to get the scroll wheel value from.</param>
    /// <returns>the scroll wheel value.</returns>
    public static int GetScrollWheelValue(MouseState state)
    {
        return state.CurrentState.ScrollWheelValue;
    }

    /// <summary>
    ///     Gets the difference in the scroll wheel value between the previous and current frame.
    /// </summary>
    /// <param name="state">the mouse instance to ge the delta scroll wheel value from.</param>
    /// <returns>the delta scroll wheel value.</returns>
    public static int GetScrollWheelDelta(MouseState state)
    {
        return state.CurrentState.ScrollWheelValue - state.PreviousState.ScrollWheelValue;
    }

    /// <summary>
    ///     Updates the state information for the mouse input.
    /// </summary>
    /// <param name="state">the mouse input to update.</param>
    public static void Update(MouseState state)
    {
        state.PreviousState = state.CurrentState;
        state.CurrentState = Microsoft.Xna.Framework.Input.Mouse.GetState();
    }

    /// <summary>
    ///     Sets the mouse position within the back buffer.
    /// </summary>
    /// <param name="state">the mouse instance to set.</param>
    /// <param name="position">the position on the back buffer to set the mouse to.</param>
    public static void SetBackBufferPosition(MouseState state, Vector2 position)
    {
        int x = (int)position.X;
        int y = (int)position.Y;
        Microsoft.Xna.Framework.Input.Mouse.SetPosition(x,y);
        state.CurrentState = new Microsoft.Xna.Framework.Input.MouseState(
            x,
            y,
            state.CurrentState.ScrollWheelValue,
            state.CurrentState.LeftButton,
            state.CurrentState.MiddleButton,
            state.CurrentState.RightButton,
            state.CurrentState.XButton1,
            state.CurrentState.XButton2
        );
    }




    /******************
    
        Is Button Down Checks.
    
    *******************/




    /// <summary>
    ///     Gets whether a the left mouse button is down.
    /// </summary>
    /// <param name="state">the state instance.</param>
    /// <returns>true, if the button is down; otherwise false.</returns>
    public static bool IsLeftButtonDown(MouseState state)
    {
        return state.CurrentState.LeftButton == ButtonState.Pressed;
    }

    /// <summary>
    ///     Gets whether the right mouse button is down.
    /// </summary>
    /// <param name="state">the state instance.</param>
    /// <returns>true, if the button is down; otherwise false.</returns>
    public static bool IsRightButtonDown(MouseState state)
    {
        return state.CurrentState.RightButton == ButtonState.Pressed;
    }

    /// <summary>
    ///     Gets whether the middle mouse button is down.
    /// </summary>
    /// <param name="state">the state instance.</param>
    /// <returns>true, if the button is down otherwise false.</returns>
    public static bool IsMiddleButtonDown(MouseState state)
    {
        return state.CurrentState.MiddleButton == ButtonState.Pressed;
    }

    /// <summary>
    ///     Gets whether the X button 1 is down.
    /// </summary>
    /// <param name="state">the state instance.</param>
    /// <returns>true, if the button is down otherwise false.</returns>
    public static bool IsXButton1Down(MouseState state)
    {
        return state.CurrentState.XButton1 == ButtonState.Pressed;
    }

    /// <summary>
    ///     Gets whether the X button 2 is down.
    /// </summary>
    /// <param name="state">the state instance.</param>
    /// <returns>true, if the button is down otherwise false.</returns>
    public static bool IsXButton2Down(MouseState state)
    {
        return state.CurrentState.XButton2 == ButtonState.Pressed;
    }




    /******************
    
        Is Button Up Checks.
    
    *******************/




    /// <summary>
    ///     Gets whether the left mouse button is up.
    /// </summary>
    /// <param name="state">the state instance.</param>
    /// <returns>true, if the button is up; otherwise false.</returns>
    public static bool IsLeftButtonUp(MouseState state)
    {
        return state.CurrentState.LeftButton == ButtonState.Released;
    }

    /// <summary>
    ///     Gets whether the right mouse button is up.
    /// </summary>
    /// <param name="state">the state instance.</param>
    /// <returns>true, if the button is up; otherwise false.</returns>
    public static bool IsRightButtonUp(MouseState state)
    {
        return state.CurrentState.RightButton == ButtonState.Released;
    }

    /// <summary>
    ///     Gets whether the middle mouse button is up.
    /// </summary>
    /// <param name="state">the state instance.</param>
    /// <returns>true, if the button is up; otherwise false.</returns>
    public static bool IsMiddleButtonUp(MouseState state)
    {
        return state.CurrentState.MiddleButton == ButtonState.Released;
    }

    /// <summary>
    ///     Gets whether the X button 1 is up.
    /// </summary>
    /// <param name="state">the state instance.</param>
    /// <returns>true, if the button is up; otherwise false.</returns>
    public static bool IsXButton1Up(MouseState state)
    {
        return state.CurrentState.XButton1 == ButtonState.Released;
    }

    /// <summary>
    ///     Gets whether the X button 2 is up.
    /// </summary>
    /// <param name="state">the state instance.</param>
    /// <returns>true, if the button is up; otherwise false.</returns>
    public static bool IsXButton2Up(MouseState state)
    {
        return state.CurrentState.XButton2 == ButtonState.Released;
    }




    /******************
    
        Is Button Just Pressed Checks.
    
    *******************/




    /// <summary>
    ///     Gets whether the left mouse button has just been pressed this update cycle.
    /// </summary>
    /// <param name="state">the state instance.</param>
    /// <returns>true, if the mouse button has just been pressed; otherwise false.</returns>
    public static bool IsLeftButtonJustPressed(MouseState state)
    {
        return state.CurrentState.LeftButton == ButtonState.Pressed && state.PreviousState.LeftButton == ButtonState.Released;
    }

    /// <summary>
    ///     Gets whether the right mouse button has just been pressed this update cycle.
    /// </summary>
    /// <param name="state">the state instance. </param>
    /// <returns>true, if the mouse button has just been pressed; otherwise false.</returns>
    public static bool IsRightButtonJustPressed(MouseState state)
    {
        return state.CurrentState.RightButton == ButtonState.Pressed && state.PreviousState.RightButton == ButtonState.Released;
    }

    /// <summary>
    ///     Gets whether the middle mouse button has just been pressed this update cycle.
    /// </summary>
    /// <param name="state">the state instance. </param>
    /// <returns>true, if the mouse button has just been pressed; otherwise false.</returns>
    public static bool IsMiddleButtonJustPressed(MouseState state)
    {
        return state.CurrentState.MiddleButton == ButtonState.Pressed && state.PreviousState.MiddleButton == ButtonState.Released;
    }

    /// <summary>
    ///     Gets whether the X mouse button 1 has just been pressed this update cycle.
    /// </summary>
    /// <param name="state">the state instance. </param>
    /// <returns>true, if the mouse button has just been pressed; otherwise false.</returns>
    public static bool IsXButton1JustPressed(MouseState state)
    {
        return state.CurrentState.XButton1 == ButtonState.Pressed && state.PreviousState.XButton1 == ButtonState.Released;
    }

    /// <summary>
    ///     Gets whether the X mouse button 2 has just been pressed this update cycle.
    /// </summary>
    /// <param name="state">the state instance. </param>
    /// <returns>true, if the mouse button has just been pressed; otherwise false.</returns>
    public static bool IsXButton2JustPressed(MouseState state)
    {
        return state.CurrentState.XButton2 == ButtonState.Pressed && state.PreviousState.XButton2 == ButtonState.Released;
    }




    /******************
    
        Is Button Just Released Checks.
    
    *******************/




    /// <summary>
    ///     Gets whether the left mouse button has just been released this update cycle.
    /// </summary>
    /// <param name="state">the state instance. </param>
    /// <returns>true, if the mouse button has just been pressed; otherwise false.</returns>
    public static bool IsLeftButtonJustReleased(MouseState state)
    {
        return state.CurrentState.LeftButton == ButtonState.Released && state.PreviousState.LeftButton == ButtonState.Pressed;
    }

    /// <summary>
    ///     Gets whether the right mouse button has just been released this update cycle.
    /// </summary>
    /// <param name="state">the state instance. </param>
    /// <returns>true, if the mouse button has just been pressed; otherwise false.</returns>
    public static bool IsRightButtonJustReleased(MouseState state)
    {
        return state.CurrentState.RightButton == ButtonState.Released && state.PreviousState.RightButton == ButtonState.Pressed;
    }

    /// <summary>
    ///     Gets whether the middle mouse button has just been released this update cycle.
    /// </summary>
    /// <param name="state">the state instance. </param>
    /// <returns>true, if the mouse button has just been pressed; otherwise false.</returns>
    public static bool IsMiddleButtonJustReleased(MouseState state)
    {
        return state.CurrentState.MiddleButton == ButtonState.Released && state.PreviousState.MiddleButton == ButtonState.Pressed;
    }

    /// <summary>
    ///     Gets whether the X mouse button 1 has just been released this update cycle.
    /// </summary>
    /// <param name="state">the state instance. </param>
    /// <returns>true, if the mouse button has just been pressed; otherwise false.</returns>
    public static bool IsXButton1JustReleased(MouseState state)
    {
        return state.CurrentState.XButton1 == ButtonState.Released && state.PreviousState.XButton1 == ButtonState.Pressed;
    }

    /// <summary>
    ///     Gets whether the X mouse button 2 has just been released this update cycle.
    /// </summary>
    /// <param name="state">the state instance. </param>
    /// <returns>true, if the mouse button has just been pressed; otherwise false.</returns>
    public static bool IsXButton2JustReleased(MouseState state)
    {
        return state.CurrentState.XButton2 == ButtonState.Released && state.PreviousState.XButton2 == ButtonState.Pressed;
    }




    /******************
    
        Mouse-Space Conversions
    
    *******************/



    /// <summary>
    ///     Gets the mouse position in world-space.
    /// </summary>
    /// <param name="app">the monogame app instance.</param>
    /// <param name="cameraPosition">the world-space camera position.</param>
    /// <param name="cameraZoom">the world-space camera zoom factor.</param>
    /// <param name="cameraVerticalFov">the world-space camera vertical fov.</param>
    /// <returns>the position of the mouse in world-space.</returns>
    public static Vector2 GetWorldPosition(MonoGameApp app, Vector2 cameraPosition, float cameraZoom, float cameraVerticalFov)
    {
        MouseState mouse = app.InputManagerState.MouseState;
        Vector2 renderTargetPosition = GetPositionRelative(
            GetBackBufferPosition(mouse), 
            app.OutputResolution, 
            new Vector2(app.DestinationRectangle.X, app.DestinationRectangle.Y), 
            (int)app.DestinationRectangle.Width,
            (int)app.DestinationRectangle.Height
        );
        // offset by half the output resolution as the world camera (0,0) is at the center of the screen.
        Howl.Math.Vector2 offset = new Howl.Math.Vector2(app.OutputResolution.X*0.5f, app.OutputResolution.Y*0.5f);
        
        Vector2 v = new Vector2( 
            ((renderTargetPosition.X - offset.X)/cameraZoom) + cameraPosition.X,
            ((renderTargetPosition.Y - offset.Y)/cameraZoom) - cameraPosition.Y
        );

        // invert y as world space in monogame is Y+ is down; where as howl engine is y+ is up.
        return new Vector2(v.X, -v.Y);
    }

    /// <summary>
    ///     Gets the mouse postion in screen-space.
    /// </summary>
    /// <param name="app">the monogame app instance.</param>
    /// <param name="cameraPosition">the screen-space camera position.</param>
    /// <param name="cameraZoom">the screen-space camera zoom factor.</param>
    /// <param name="cameraVerticalFov">the screen-space camera vertical fov.</param>
    /// <returns>the position of the mouse in screen-space.</returns>
    public static Vector2 GetScreenPosition(MonoGameApp app, Vector2 cameraPosition, float cameraZoom, float cameraVerticalFov)
    {
        MouseState mouse = app.InputManagerState.MouseState;
        Vector2 renderTargetPosition = GetPositionRelative(
            GetBackBufferPosition(mouse), 
            app.OutputResolution, 
            new Vector2(app.DestinationRectangle.X, app.DestinationRectangle.Y), 
            (int)app.DestinationRectangle.Width,
            (int)app.DestinationRectangle.Height
        );

        return new Vector2( 
            (renderTargetPosition.X + cameraPosition.X)/cameraZoom,
            (renderTargetPosition.Y + cameraPosition.Y)/cameraZoom
        );
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