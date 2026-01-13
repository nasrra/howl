using System;
using Howl.Input;

namespace Howl.Vendors.MonoGame.Input;

public class MonoGameInputManager : IInputManager
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

    public MonoGameInputManager()
    {
        Keyboard = new MonoGameKeyboard();
        Mouse = new MonoGameMouse();
        GamePads = new IGamePad[(int)GamePadId.Count];
        Span<IGamePad> gamepads = GamePads.AsSpan();
        for(int i = 0; i < gamepads.Length; i++)
        {
            gamepads[i] = new MonoGameGamePad((GamePadId)i);
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