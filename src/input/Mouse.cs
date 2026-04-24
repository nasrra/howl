using Howl.Graphics;
using Howl.Math;

namespace Howl.Input;

public static class Mouse
{




    /******************
    
        Input.
    
    *******************/




    /// <summary>
    ///     Checks if a mouse button is down.
    /// </summary>
    /// <param name="app">the howl app with the input state.</param>
    /// <param name="button">the mouse button to check.</param>
    /// <returns>true, if the mouse button is down; otherwise false.</returns>
    public static bool IsButtonDown(HowlApp app, MouseButton button)
    {
        Vendors.MonoGame.Input.InputManagerState input = app.MonoGameApp.InputManagerState;

        switch (button)
        {
            case MouseButton.Left:
                return Vendors.MonoGame.Input.Mouse.IsLeftButtonDown(input.MouseState);
            case MouseButton.Right:
                return Vendors.MonoGame.Input.Mouse.IsRightButtonDown(input.MouseState);
            case MouseButton.Middle:
                return Vendors.MonoGame.Input.Mouse.IsMiddleButtonDown(input.MouseState);
            case MouseButton.XButton1:
                return Vendors.MonoGame.Input.Mouse.IsXButton1Down(input.MouseState);
            case MouseButton.XButton2:
                return Vendors.MonoGame.Input.Mouse.IsXButton2Down(input.MouseState);
        }
        return false;
    }

    /// <summary>
    ///     Checks if a mouse button is up.
    /// </summary>
    /// <param name="app">the howl app with the input state.</param>
    /// <param name="button">the mouse button to check.</param>
    /// <returns>true, if the mouse button is up; otherwise false.</returns>
    public static bool IsButtonUp(HowlApp app, MouseButton button)
    {
        Vendors.MonoGame.Input.InputManagerState input = app.MonoGameApp.InputManagerState;

        switch (button)
        {
            case MouseButton.Left:
                return Vendors.MonoGame.Input.Mouse.IsLeftButtonUp(input.MouseState);
            case MouseButton.Right:
                return Vendors.MonoGame.Input.Mouse.IsRightButtonUp(input.MouseState);
            case MouseButton.Middle:
                return Vendors.MonoGame.Input.Mouse.IsMiddleButtonUp(input.MouseState);
            case MouseButton.XButton1:
                return Vendors.MonoGame.Input.Mouse.IsXButton1Up(input.MouseState);
            case MouseButton.XButton2:
                return Vendors.MonoGame.Input.Mouse.IsXButton2Up(input.MouseState);
        }
        return false;
    }

    /// <summary>
    ///     Checks if a mouse button has just been pressed.
    /// </summary>
    /// <param name="app">the howl app with the input state.</param>
    /// <param name="button">the mouse button to check.</param>
    /// <returns>true, if the mouse button has just been pressed; otherwise false.</returns>
    public static bool IsButtonJustPressed(HowlApp app, MouseButton button)
    {
        Vendors.MonoGame.Input.InputManagerState input = app.MonoGameApp.InputManagerState;

        switch (button)
        {
            case MouseButton.Left:
                return Vendors.MonoGame.Input.Mouse.IsLeftButtonJustPressed(input.MouseState);
            case MouseButton.Right:
                return Vendors.MonoGame.Input.Mouse.IsRightButtonJustPressed(input.MouseState);
            case MouseButton.Middle:
                return Vendors.MonoGame.Input.Mouse.IsMiddleButtonJustPressed(input.MouseState);
            case MouseButton.XButton1:
                return Vendors.MonoGame.Input.Mouse.IsXButton1JustPressed(input.MouseState);
            case MouseButton.XButton2:
                return Vendors.MonoGame.Input.Mouse.IsXButton2JustPressed(input.MouseState);
        }
        return false;
    }

    /// <summary>
    ///     Checks if a mouse button has just been released.
    /// </summary>
    /// <param name="app">the howl app with the input state.</param>
    /// <param name="button">the mouse button to check.</param>
    /// <returns>true, if the mouse button has just been released; otherwise false.</returns>
    public static bool IsButtonJustReleased(HowlApp app, MouseButton button)
    {
        Vendors.MonoGame.Input.InputManagerState input = app.MonoGameApp.InputManagerState;

        switch (button)
        {
            case MouseButton.Left:
                return Vendors.MonoGame.Input.Mouse.IsLeftButtonJustReleased(input.MouseState);
            case MouseButton.Right:
                return Vendors.MonoGame.Input.Mouse.IsRightButtonJustReleased(input.MouseState);
            case MouseButton.Middle:
                return Vendors.MonoGame.Input.Mouse.IsMiddleButtonJustReleased(input.MouseState);
            case MouseButton.XButton1:
                return Vendors.MonoGame.Input.Mouse.IsXButton1JustReleased(input.MouseState);
            case MouseButton.XButton2:
                return Vendors.MonoGame.Input.Mouse.IsXButton2JustReleased(input.MouseState);
        }
        return false;
           
    }




    /******************
    
        Mouse-Space Conversions.
    
    *******************/



    /// <summary>
    ///     Gets the position of the mouse in world space.
    /// </summary>
    /// <param name="app">the howl app instance containg the mouse and camera states.</param>
    /// <returns>the mouse world-space position.</returns>
    public static Vector2 GetWorldPosition(HowlApp app)
    {
        Camera camera = default;
        CameraSystem.GetDrawSpaceCamera(app.EcsState, DrawSpace.World, ref camera);

        Microsoft.Xna.Framework.Vector2 mCameraPosition = Vendors.MonoGame.Math.Vector2Extensions.ToMonoGame(camera.Position);
        Microsoft.Xna.Framework.Vector2 mPosition =  Vendors.MonoGame.Input.Mouse.GetWorldPosition(app.MonoGameApp, mCameraPosition, camera.Zoom, camera.VerticalFov);

        return Vendors.MonoGame.Math.Vector2Extensions.ToHowl(mPosition);
    }

    /// <summary>
    ///     Gets the position of the mouse in screen-space.
    /// </summary>
    /// <param name="app">the howl app instance containing the mouse and camera states.</param>
    /// <returns>the mouse screen-space position.</returns>
    public static Vector2 GetScreenPosition(HowlApp app)
    {
        Camera camera = default;
        CameraSystem.GetDrawSpaceCamera(app.EcsState, DrawSpace.Gui, ref camera);

        Microsoft.Xna.Framework.Vector2 mCameraPosition = Vendors.MonoGame.Math.Vector2Extensions.ToMonoGame(camera.Position);
        Microsoft.Xna.Framework.Vector2 mPosition =  Vendors.MonoGame.Input.Mouse.GetScreenPosition(app.MonoGameApp, mCameraPosition, camera.Zoom, camera.VerticalFov);

        return Vendors.MonoGame.Math.Vector2Extensions.ToHowl(mPosition);
    }
}