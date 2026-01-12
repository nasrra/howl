using System;
using Howl.Monogame.Input;

namespace Howl.Input;

public class InputManager
{
    /// <summary>
    /// Gets the keyboard state information.
    /// </summary>
    public IKeyboard Keyboard {get;}

    /// <summary>
    /// Gets the mouse state information.
    /// </summary>
    public IMouse Mouse {get;}

    /// <summary>
    /// Gets the gamepad state information.
    /// </summary>
    public IGamePad[] GamePads {get;}

    public InputManager()
    {
        Keyboard = new MonogameKeyboard();
        Mouse = new MonogameMouse();
        GamePads = new IGamePad[(int)GamePadId.Count];
        Span<IGamePad> gamepads = GamePads.AsSpan();
        for(int i = 0; i < gamepads.Length; i++)
        {
            gamepads[i] = new MonogameGamePad((GamePadId)i);
        }
    }

    public void Update(float deltaTime)
    {
        Keyboard.Update();
        Mouse.Update();
        Span<IGamePad> gamepads = GamePads.AsSpan();
        for(int i = 0; i < gamepads.Length; i++)
        {
            gamepads[i].Update(deltaTime);
        }
    }
}