using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Howl.Vendors.MonoGame.Input;

public static class GamePad
{

    /// <summary>
    ///     Gets whether or not a gamepad is connected.
    /// </summary>
    /// <param name="state">the state instance of the gamepad.</param>
    /// <returns>true, if the gamepad is connected; otherwise false.</returns>
    public static bool IsConnected(GamePadState state)
    {
        return state.CurrentState.IsConnected;
    }

    /// <summary>
    ///     Gets the user input of a gamepad's left thumbstick.
    /// </summary>
    /// <param name="state">the state instance of the gamepad.</param>
    /// <returns>the directional vector of the thumbstick input.</returns>
    public static Vector2 GetLeftThumbstickInput(GamePadState state)
    {
        return state.CurrentState.ThumbSticks.Left;
    }

    /// <summary>
    ///     Gets the user input of a gamepad's right thumbstick.
    /// </summary>
    /// <param name="state">the state instance of the gamepad.</param>
    /// <returns>the directional vector of the thumbstick input.</returns>
    public static Vector2 GetRightThumbstickInput(GamePadState state)
    {
        return state.CurrentState.ThumbSticks.Right;
    }

    /// <summary>
    ///     Gets the user input of a gamepad's left trigger.
    /// </summary>
    /// <param name="state">the state instance of the gamepad.</param>
    /// <returns>a normalised value (0-1) indicating how far down the trigger is pressed.</returns>
    public static float GetLeftTriggerInput(GamePadState state)
    {
        return state.CurrentState.Triggers.Left;
    }

    /// <summary>
    ///     Gets the user input of a gamepad's right trigger.
    /// </summary>
    /// <param name="state">the state instance of the gamepad.</param>
    /// <returns>a normalised value (0-1) indicating how far down the trigger is pressed.</returns>
    public static float GetRightTriggerInput(GamePadState state)
    {
        return state.CurrentState.Triggers.Right;
    }

    /// <summary>
    ///     Checks if a button is currently pressed down.
    /// </summary>
    /// <param name="state">the state instance of the gamepad.</param>
    /// <param name="button">the button to check.</param>
    /// <returns>true, if the button is down; otherwise false.</returns>
    public static bool IsButtonDown(GamePadState state, Buttons button)
    {
        return state.CurrentState.IsButtonDown(button);
    }

    /// <summary>
    ///     Checks if a button is currently not pressed down.
    /// </summary>
    /// <param name="state">the state instance of the gamepad.</param>
    /// <param name="button">the button to check.</param>
    /// <returns>true, if the button is down; otherwise false.</returns>
    public static bool IsButtonUp(GamePadState state, Buttons button)
    {
        return state.CurrentState.IsButtonDown(button);
    }

    /// <summary>
    ///     Checks if a button has just been pressed this update cycle.
    /// </summary>
    /// <param name="state">the state instance of the gamepad.</param>
    /// <param name="button">the button to check.</param>
    /// <returns>true, if the button has just been pressed; otherwise false.</returns>
    public static bool IsButtonJustPressed(GamePadState state, Buttons button)
    {
        return state.CurrentState.IsButtonDown(button) && state.PreviousState.IsButtonUp(button);
    }

    /// <summary>
    ///     Checks if a button has just been released this update cycle.
    /// </summary>
    /// <param name="state">the state instance of the gamepad.</param>
    /// <param name="button">the button to check.</param>
    /// <returns>true, if the button has just been released; otherwise false.</returns>
    public static bool IsButtonJustReleased(GamePadState state, Buttons button)
    {
        return state.CurrentState.IsButtonUp(button) && state.PreviousState.IsButtonDown(button);        
    }

    /// <summary>
    ///     Sets the vibration amount for a gamepad.
    /// </summary>
    /// <remarks>
    ///     <paramref name="strength"/> will be clamped between 0 and 1.
    /// </remarks>
    /// <param name="state">the state instance of the gamepad.</param>
    /// <param name="strength">the amount of vibration.</param>
    public static void SetVibration(GamePadState state, float strength)
    {        
        float value = float.Clamp(strength, 0, 1); 
        Microsoft.Xna.Framework.Input.GamePad.SetVibration(state.PlayerIndex, value, value);
    }

    /// <summary>
    ///     Sets the vibration amount to zero for a gamepad.
    /// </summary>
    /// <param name="state">the state instance of the gamepad.</param>
    public static void StopVibration(GamePadState state)
    {
        Microsoft.Xna.Framework.Input.GamePad.SetVibration(state.PlayerIndex, 0, 0);
    }

    /// <summary>
    ///     Updates a the state information of gamepad input.
    /// </summary>
    /// <param name="state">the state instance of the gamepad to update.</param>
    public static void Update(GamePadState state)
    {
        if(IsConnected(state) == false)
        {
            return;
        }

        state.PreviousState = state.CurrentState;
        state.CurrentState = Microsoft.Xna.Framework.Input.GamePad.GetState(state.PlayerIndex);        
    }

    /// <summary>
    ///     Disposes of a state instance.
    /// </summary>
    /// <param name="state">the state instance to dipose of</param>
    public static void Dispose(GamePadState state)
    {
        if (state.Disposed)
        {
            return;
        }
        state.Disposed = true;
        GC.SuppressFinalize(state);
    }
}