using Howl.Math;

namespace Howl.Input;

public static class GamePad
{
    /// <summary>
    ///     Gets whether a gamepad is connected.
    /// </summary>
    /// <param name="app">the howl app instance with the input state.</param>
    /// <param name="gamePadId">the id of the gamepad.</param>
    /// <returns>true, if the gamepad is connected; otherwise false.</returns>
    public static bool IsConnected(HowlApp app, int gamePadId)
    {
        Vendors.MonoGame.Input.GamePadState state = app.MonoGameApp.InputManagerState.GamePadStates[gamePadId];
        return Vendors.MonoGame.Input.GamePad.IsConnected(state);
    }

    /// <summary>
    ///     Gets the user input of a gamepad's left thumbstick.
    /// </summary>
    /// <param name="app">the howl app instance with the input state.</param>
    /// <param name="gamePadId">the id of the gamepad.</param>
    /// <returns>the directional vector of the thumbstick input.</returns>
    public static Vector2 GetLeftThumbstickInput(HowlApp app, int gamePadId)
    {
        Vendors.MonoGame.Input.GamePadState state = app.MonoGameApp.InputManagerState.GamePadStates[gamePadId];
        Microsoft.Xna.Framework.Vector2 v = Vendors.MonoGame.Input.GamePad.GetLeftThumbstickInput(state);
        return Vendors.MonoGame.Math.Vector2Extensions.ToHowl(v);
    }
    
    /// <summary>
    ///     Gets the user input of a gamepad's right thumbstick.
    /// </summary>
    /// <param name="app">the howl app instance with the input state.</param>
    /// <param name="gamePadId">the id of the gamepad.</param>
    /// <returns>the directional vector of the thumbstick input.</returns>
    public static Vector2 GetRightThumbstickInput(HowlApp app, int gamePadId)
    {
        Vendors.MonoGame.Input.GamePadState state = app.MonoGameApp.InputManagerState.GamePadStates[gamePadId];
        Microsoft.Xna.Framework.Vector2 v = Vendors.MonoGame.Input.GamePad.GetRightThumbstickInput(state);
        return Vendors.MonoGame.Math.Vector2Extensions.ToHowl(v);
    }

    /// <summary>
    ///     Gets the user input of a gamepad's left trigger.
    /// </summary>
    /// <param name="app">the howl app instance with the input state.</param>
    /// <param name="gamePadId">the id of the gamepad.</param>
    /// <returns>a normalised value (0-1) indicating how far down the trigger is pressed.</returns>
    public static float GetLeftTriggerInput(HowlApp app, int gamePadId)
    {
        Vendors.MonoGame.Input.GamePadState state = app.MonoGameApp.InputManagerState.GamePadStates[gamePadId];
        return Vendors.MonoGame.Input.GamePad.GetLeftTriggerInput(state);        
    }

    /// <summary>
    ///     Gets the user input of a gamepad's right trigger.
    /// </summary>
    /// <param name="app">the howl app instance with the input state.</param>
    /// <param name="gamePadId">the id of the gamepad.</param>
    /// <returns>a normalised value (0-1) indicating how far down the trigger is pressed.</returns>
    public static float GetRightTriggerInput(HowlApp app, int gamePadId)
    {
        Vendors.MonoGame.Input.GamePadState state = app.MonoGameApp.InputManagerState.GamePadStates[gamePadId];
        return Vendors.MonoGame.Input.GamePad.GetRightTriggerInput(state);        
    }

    /// <summary>
    ///     Checks if a button is currently pressed down.
    /// </summary>
    /// <param name="app">the howl app instance with the input state.</param>
    /// <param name="gamePadId">the id of the gamepad.</param>
    /// <param name="button">the button to check.</param>
    /// <returns>true, if the button is down; otherwise false.</returns>
    public static bool IsButtonDown(HowlApp app, int gamePadId, GamePadButton button)
    {
        Vendors.MonoGame.Input.GamePadState state = app.MonoGameApp.InputManagerState.GamePadStates[gamePadId];
        Microsoft.Xna.Framework.Input.Buttons mButton = Vendors.MonoGame.Input.ButtonsExtensions.ToMonoGame(button);
        return Vendors.MonoGame.Input.GamePad.IsButtonDown(state, mButton);
    }

    /// <summary>
    ///     Checks if a button is currently not pressed down.
    /// </summary>
    /// <param name="app">the howl app instance with the input state.</param>
    /// <param name="gamePadId">the id of the gamepad.</param>
    /// <param name="button">the button to check.</param>
    /// <returns>true, if the button is down; otherwise false.</returns>
    public static bool IsButtonUp(HowlApp app, int gamePadId, GamePadButton button)
    {
        Vendors.MonoGame.Input.GamePadState state = app.MonoGameApp.InputManagerState.GamePadStates[gamePadId];
        Microsoft.Xna.Framework.Input.Buttons mButton = Vendors.MonoGame.Input.ButtonsExtensions.ToMonoGame(button);
        return Vendors.MonoGame.Input.GamePad.IsButtonUp(state, mButton);
    }

    /// <summary>
    ///     Checks if a button has just been pressed this update cycle.
    /// </summary>
    /// <param name="app">the howl app instance with the input state.</param>
    /// <param name="gamePadId">the id of the gamepad.</param>
    /// <param name="button">the button to check.</param>
    /// <returns>true, if the button has just been pressed; otherwise false.</returns>
    public static bool IsButtonJustPressed(HowlApp app, int gamePadId, GamePadButton button)
    {
        Vendors.MonoGame.Input.GamePadState state = app.MonoGameApp.InputManagerState.GamePadStates[gamePadId];
        Microsoft.Xna.Framework.Input.Buttons mButton = Vendors.MonoGame.Input.ButtonsExtensions.ToMonoGame(button);
        return Vendors.MonoGame.Input.GamePad.IsButtonDown(state, mButton);
    }

    /// <summary>
    ///     Checks if a button has just been released this update cycle.
    /// </summary>
    /// <param name="app">the howl app instance with the input state.</param>
    /// <param name="gamePadId">the id of the gamepad.</param>
    /// <param name="button">the button to check.</param>
    /// <returns>true, if the button hs just been released; otherwise false.</returns>
    public static bool IsButtonJustReleased(HowlApp app, int gamePadId, GamePadButton button)
    {
        Vendors.MonoGame.Input.GamePadState state = app.MonoGameApp.InputManagerState.GamePadStates[gamePadId];
        Microsoft.Xna.Framework.Input.Buttons mButton = Vendors.MonoGame.Input.ButtonsExtensions.ToMonoGame(button);
        return Vendors.MonoGame.Input.GamePad.IsButtonUp(state, mButton);
    }

    /// <summary>
    ///     Sets the vibration amount for a gamepad.
    /// </summary>
    /// <remarks>
    ///     <paramref name="strength"/> will be clamped between 0 and 1.
    /// </remarks>
    /// <param name="app">the howl app instance with the input state.</param>
    /// <param name="gamePadId">the id of the gamepad.</param>
    /// <param name="strength">the amount of vibration.</param>
    public static void SetVibration(HowlApp app, int gamePadId, float strength)
    {
        Vendors.MonoGame.Input.GamePadState state = app.MonoGameApp.InputManagerState.GamePadStates[gamePadId];
        Vendors.MonoGame.Input.GamePad.SetVibration(state, strength);
    }

    /// <summary>
    ///     Sets the vibration amount to zero for a gamepad.
    /// </summary>
    /// <param name="app">the howl app instance with the input state.</param>
    /// <param name="gamePadId">the id of the gamepad.</param>
    public static void StopVibration(HowlApp app, int gamePadId)
    {
        Vendors.MonoGame.Input.GamePadState state = app.MonoGameApp.InputManagerState.GamePadStates[gamePadId];
        Vendors.MonoGame.Input.GamePad.StopVibration(state);
    }
}