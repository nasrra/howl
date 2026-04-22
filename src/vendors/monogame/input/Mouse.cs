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




    public static bool IsLeftButtonDown(MouseState state)
    {
        return state.CurrentState.LeftButton == ButtonState.Pressed;
    }

    public static bool IsRightButtonDown(MouseState state)
    {
        return state.CurrentState.RightButton == ButtonState.Pressed;
    }

    public static bool IsMiddleButtonDown(MouseState state)
    {
        return state.CurrentState.MiddleButton == ButtonState.Pressed;
    }

    public static bool IsXButton1Down(MouseState state)
    {
        return state.CurrentState.XButton1 == ButtonState.Pressed;
    }

    public static bool IsXButton2Down(MouseState state)
    {
        return state.CurrentState.XButton2 == ButtonState.Pressed;
    }




    /******************
    
        Is Button Up Checks.
    
    *******************/




    public static bool IsLeftButtonUp(MouseState state)
    {
        return state.CurrentState.LeftButton == ButtonState.Released;
    }

    public static bool IsRightButtonUp(MouseState state)
    {
        return state.CurrentState.RightButton == ButtonState.Released;
    }

    public static bool IsMiddleButtonUp(MouseState state)
    {
        return state.CurrentState.MiddleButton == ButtonState.Released;
    }

    public static bool IsXButton1Up(MouseState state)
    {
        return state.CurrentState.XButton1 == ButtonState.Released;
    }

    public static bool IsXButton2Up(MouseState state)
    {
        return state.CurrentState.XButton2 == ButtonState.Released;
    }




    /******************
    
        Is Button Just Pressed Checks.
    
    *******************/




    public static bool IsLeftButtonJustPressed(MouseState state)
    {
        return state.CurrentState.LeftButton == ButtonState.Pressed && state.PreviousState.LeftButton == ButtonState.Released;
    }

    public static bool IsRightButtonJustPressed(MouseState state)
    {
        return state.CurrentState.RightButton == ButtonState.Pressed && state.PreviousState.RightButton == ButtonState.Released;
    }

    public static bool IsMiddleButtonJustPressed(MouseState state)
    {
        return state.CurrentState.MiddleButton == ButtonState.Pressed && state.PreviousState.MiddleButton == ButtonState.Released;
    }

    public static bool IsXButton1JustPressed(MouseState state)
    {
        return state.CurrentState.XButton1 == ButtonState.Pressed && state.PreviousState.XButton1 == ButtonState.Released;
    }

    public static bool IsXButton2JustPressed(MouseState state)
    {
        return state.CurrentState.XButton2 == ButtonState.Pressed && state.PreviousState.XButton2 == ButtonState.Released;
    }




    /******************
    
        Is Button Just Released Checks.
    
    *******************/




    public static bool IsLeftButtonJustReleased(MouseState state)
    {
        return state.CurrentState.LeftButton == ButtonState.Released && state.PreviousState.LeftButton == ButtonState.Pressed;
    }

    public static bool IsRightButtonJustReleased(MouseState state)
    {
        return state.CurrentState.RightButton == ButtonState.Released && state.PreviousState.RightButton == ButtonState.Pressed;
    }

    public static bool IsMiddleButtonJustReleased(MouseState state)
    {
        return state.CurrentState.MiddleButton == ButtonState.Released && state.PreviousState.MiddleButton == ButtonState.Pressed;
    }

    public static bool IsXButton1JustReleased(MouseState state)
    {
        return state.CurrentState.XButton1 == ButtonState.Released && state.PreviousState.XButton1 == ButtonState.Pressed;
    }

    public static bool IsXButton2JustReleased(MouseState state)
    {
        return state.CurrentState.XButton2 == ButtonState.Released && state.PreviousState.XButton2 == ButtonState.Pressed;
    }
}