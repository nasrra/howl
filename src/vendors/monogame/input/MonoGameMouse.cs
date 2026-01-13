using Howl.Math;
using Howl.Input;
using Microsoft.Xna.Framework.Input;
using System;

namespace Howl.Vendors.MonoGame.Input;

public class MonoGameMouse : IMouse
{
    
    /// <summary>
    /// The state of the mouse input during the previous update cycle.
    /// </summary>
    private MouseState previousState;

    /// <summary>
    /// The state of the mouse input during the current update cycle.
    /// </summary>
    private MouseState currentState;

    /// <summary>
    /// Creates a new Monogame Mouse instance.
    /// </summary>
    public MonoGameMouse()
    {
        currentState = Mouse.GetState();
        previousState = new MouseState();
    }

    public Vector2Int Position
    {
        get => new (currentState.Position.X, currentState.Position.Y);
        set => SetPosition(value);
    } 

    public int X
    {
        get => currentState.X;
        set => SetPosition(new(value, currentState.Y));
    }

    public int Y
    {
        get => currentState.Y;
        set => SetPosition(new(currentState.Y, value));
    }

    public Vector2Int PositionDelta
    {
        get{
            Microsoft.Xna.Framework.Point point = currentState.Position - previousState.Position;
            return new(point.X, point.Y);       
        }
    }

    public int XDelta => currentState.X - previousState.X;

    public int YDelta => currentState.Y - previousState.Y;

    public bool WasMoved => PositionDelta != Vector2Int.Zero;

    public int ScrollWheel => currentState.ScrollWheelValue;

    public int ScrollWheelDelta => currentState.ScrollWheelValue - previousState.ScrollWheelValue;

    public void Update()
    {
        previousState = currentState;
        currentState = Mouse.GetState();
    }

    public void SetPosition(Vector2Int position)
    {
        Mouse.SetPosition(position.X,position.Y);
        currentState = new MouseState(
            position.X,
            position.Y,
            currentState.ScrollWheelValue,
            currentState.LeftButton,
            currentState.MiddleButton,
            currentState.RightButton,
            currentState.XButton1,
            currentState.XButton2
        );
    }

    public bool IsButtonDown(MouseButton mouseButton)
    {
        switch (mouseButton)
        {
            case MouseButton.Left:
                return currentState.LeftButton == ButtonState.Pressed;
            case MouseButton.Right:
                return currentState.RightButton == ButtonState.Pressed;
            case MouseButton.Middle:
                return currentState.MiddleButton == ButtonState.Pressed;
            case MouseButton.XButton1:
                return currentState.XButton1 == ButtonState.Pressed;
            case MouseButton.XButton2:
                return currentState.XButton2 == ButtonState.Pressed;
            default:
                throw new InvalidOperationException($"{mouseButton} is not a valid mouse input for Monogame Mouse.");
        }
    }

    public bool IsButtonUp(MouseButton mouseButton)
    {
        switch (mouseButton)
        {
            case MouseButton.Left:
                return currentState.LeftButton == ButtonState.Released;
            case MouseButton.Right:
                return currentState.RightButton == ButtonState.Released;
            case MouseButton.Middle:
                return currentState.MiddleButton == ButtonState.Released;
            case MouseButton.XButton1:
                return currentState.XButton1 == ButtonState.Released;
            case MouseButton.XButton2:
                return currentState.XButton2 == ButtonState.Released;
            default:
                throw new InvalidOperationException($"{mouseButton} is not a valid mouse input for Monogame Mouse.");
        }
    }

    public bool IsButtonJustPressed(MouseButton mouseButton)
    {
        switch (mouseButton)
        {
            case MouseButton.Left:
                return currentState.LeftButton == ButtonState.Pressed && previousState.LeftButton == ButtonState.Released;
            case MouseButton.Right:
                return currentState.RightButton == ButtonState.Pressed && previousState.RightButton == ButtonState.Released;
            case MouseButton.Middle:
                return currentState.MiddleButton == ButtonState.Pressed && previousState.MiddleButton == ButtonState.Released;
            case MouseButton.XButton1:
                return currentState.XButton1 == ButtonState.Pressed && previousState.XButton1 == ButtonState.Released;
            case MouseButton.XButton2:
                return currentState.XButton2 == ButtonState.Pressed && previousState.XButton2 == ButtonState.Released;
            default:
                throw new InvalidOperationException($"{mouseButton} is not a valid mouse input for Monogame Mouse.");
        }

    }

    public bool IsButtonJustReleased(MouseButton mouseButton)
    {
        switch (mouseButton)
        {
            case MouseButton.Left:
                return currentState.LeftButton == ButtonState.Released && previousState.LeftButton == ButtonState.Pressed;
            case MouseButton.Right:
                return currentState.RightButton == ButtonState.Released && previousState.RightButton == ButtonState.Pressed;
            case MouseButton.Middle:
                return currentState.MiddleButton == ButtonState.Released && previousState.MiddleButton == ButtonState.Pressed;
            case MouseButton.XButton1:
                return currentState.XButton1 == ButtonState.Released && previousState.XButton1 == ButtonState.Pressed;
            case MouseButton.XButton2:
                return currentState.XButton2 == ButtonState.Released && previousState.XButton2 == ButtonState.Pressed;
            default:
                throw new InvalidOperationException($"{mouseButton} is not a valid mouse input for Monogame Mouse.");
        }
    }
}