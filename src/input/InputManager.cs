using Howl.Math;

namespace Howl.Input;

public static class InputManager
{
    /// <summary>
    ///     Updates the input state.
    /// </summary>
    /// <param name="app">the howl app instance to update.</param>
    public static void Update(HowlApp app)
    {
        Vendors.MonoGame.Input.InputManager.Update(app.MonoGameApp.InputManagerState);
    }
}