using System;

namespace Howl.Vendors.MonoGame.Input;

public static class InputManager
{
    /// <summary>
    ///     Updates a state instance.
    /// </summary>
    /// <param name="state">the state instance to update.</param>
    public static void Update(InputManagerState state)
    {
        Keyboard.Update(state.KeyboardState);
        Mouse.Update(state.MouseState);
        for(int i = 0; i < state.GamePadStates.Length; i++)
        {
            GamePad.Update(state.GamePadStates[i]);
        }
    }

    /// <summary>
    ///     Disposes of a state instance.
    /// </summary>
    /// <param name="state">the state instance to dispose of.</param>
    public static void Dispose(InputManagerState state)
    {
        if (state.Disposed)
        {
            return;
        }
        
        state.Disposed = true;
        
        Keyboard.Dispose(state.KeyboardState);
        for(int i = 0; i < state.GamePadStates.Length; i++)
        {
            GamePad.Dispose(state.GamePadStates[i]);
        }

        GC.SuppressFinalize(state);
    }
}