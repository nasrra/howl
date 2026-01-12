using Howl.Math;
using Microsoft.Xna.Framework.Input;

namespace Howl.Input;

public interface IGamePad
{
    public const int MaxGamePads = 4;

    /// <summary>
    /// Gets the id associated with this gamepad instance.
    /// </summary>
    public GamePadId GamepadId {get;}

    /// <summary>
    /// Gets whether the gamepad is currently connected.
    /// </summary>
    public bool IsConnected {get;}

    /// <summary>
    /// Gets the coordinate value of the left thumbstick.
    /// </summary>
    public Vector2 ThumbstickLeft {get;}

    /// <summary>
    /// Gets the coordinate value of the right thumbstick.
    /// </summary>
    public Vector2 ThumbstickRight {get;}

    /// <summary>
    /// Get the pressed value of the left trigger.
    /// </summary>
    public float TriggerLeft {get;}

    /// <summary>
    /// Get the pressed value of the right trigger.
    /// </summary>
    public float TriggerRight {get;}

    /// <summary>
    /// Gets whether a gamepad button is currently down.
    /// </summary>
    /// <param name="gamePadButton">The specified gamepad button to check.</param>
    /// <returns>true, if the button is currently down; otherwise false.</returns>
    public bool IsButtonDown(GamePadButton gamePadButton);

    /// <summary>
    /// Gets whether a gamepad button is currently up.
    /// </summary>
    /// <param name="gamePadButton">The specified gamepad button to check.</param>
    /// <returns>true, if the button is currently up; otherwise false.</returns>
    public bool IsButtonUp(GamePadButton gamePadButton);

    /// <summary>
    /// Gets whether a gamepad button has just been pressed.
    /// </summary>
    /// <param name="gamePadButton">The specified gamepad button to check.</param>
    /// <returns>true, if the button has just been pressed; otherwise false.</returns>
    public bool IsButtonJustPressed(GamePadButton gamePadButton);

    /// <summary>
    /// Gets whether a gamepad button has just been released.
    /// </summary>
    /// <param name="gamePadButton">The specified gamepad button to check.</param>
    /// <returns>true, ifthe button has just been released; otherwise false.</returns>
    public bool IsButtonJustReleased(GamePadButton gamePadButton);

    /// <summary>
    /// Sets the vibration for all motors of this gamepad.
    /// </summary>
    /// <param name="strength">The strength of the vibration from 0.0f (none) to 1.0f (full).</param>
    public void SetVibration(float strength);

    /// <summary>
    /// Stops the vibration od all motors for this gamepad.
    /// </summary>
    public void StopVibration();

    /// <summary>
    /// Updates the state information for this gamepad instance.
    /// </summary>
    /// <param name="deltaTime"></param>
    public void Update(float deltaTime);
}