using System;

namespace Howl.Input;

public interface IInputManager
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

    /// <summary>
    /// Updates this InputManagers state information.
    /// </summary>
    /// <param name="deltaTime"></param>
    public void Update(float deltaTime);
}