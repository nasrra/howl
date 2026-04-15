using Howl.Ecs;
using Howl.Graphics;
using Howl.Input;
using Howl.Math;
using Howl.Vendors.MonoGame;

namespace Howl;

public static class Renderer
{
    /// <summary>
    ///     Loads a font file.
    /// </summary>
    /// <param name="app">the howl app instance to load a font into.</param>
    /// <param name="fontName"></param>
    /// <param name="genIndex"></param>
    public static void LoadFont(HowlApp app, string fontName, out GenIndex genIndex)
    {
        MonoGameApp.LoadFont(app.MonoGameApp, fontName, out genIndex);
    }

    /// <summary>
    ///     Gets whether or not a font has been loaded.
    /// </summary>
    /// <param name="app"></param>
    /// <param name="genIndex"></param>
    /// <returns>true, if the font is loaded; otherwise false.</returns>
    public static GenIndexResult IsFontLoaded(HowlApp app, GenIndex genIndex)
    {
        return MonoGameApp.IsFontLoaded(app.MonoGameApp, genIndex);
    }

    /// <summary>
    ///     Sets the application to windowed mode.
    /// </summary>
    public static void SetWindowed(HowlApp app)
    {
        MonoGameApp.SetWindowed(app.MonoGameApp);
    }

    /// <summary>
    ///     Sets the application to fullscreen mode.
    /// </summary>
    /// <param name="app"></param>
    public static void SetFullscreen(HowlApp app)
    {
        MonoGameApp.SetFullscreen(app.MonoGameApp);
    }

    /// <summary>
    ///     Sets the application to borderless fullscreen mode.
    /// </summary>
    /// <param name="app"></param>
    public static void SetBorderlessFullscreen(HowlApp app)
    {
        MonoGameApp.SetBorderlessFullscreen(app.MonoGameApp);
    }

    /// <summary>
    ///     Sets the target frame rate of the application.
    /// </summary>
    /// <param name="app"></param>
    /// <param name="targetFrameRate"></param>
    public static void SetTargetFrameRate(HowlApp app, TargetFrameRate targetFrameRate)
    {
        MonoGameApp.SetTargetFrameRate(app.MonoGameApp, targetFrameRate);
    }

    /// <summary>
    ///     Gets the mouse position in world-space.
    /// </summary>
    /// <param name="app">the howl app instance.</param>
    /// <param name="mouse">the mouse data.</param>
    /// <returns>the position of the mouse in world-space.</returns>
    public static Vector2 GetMouseWorldPosition(HowlApp app, IMouse mouse)
    {
        return MonoGameApp.GetMouseWorldPosition(app.MonoGameApp, mouse);
    }

    /// <summary>
    ///     Gets the mouse position in screen-space.
    /// </summary>
    /// <param name="app">the howl app instance.</param>
    /// <param name="mouse">the mouse data.</param>
    /// <returns>the position of the mouse in world-space.</returns>
    public static Vector2 GetScreenSpacePosition(HowlApp app, IMouse mouse)
    {
        return MonoGameApp.GetMouseGuiPosition(app.MonoGameApp, mouse);
    }

    /// <summary>
    ///     Sets the resolution of the final render target.
    /// </summary>
    /// <param name="app">the howl app instance.</param>
    /// <param name="resolution">the resolution to set.</param>
    public static void SetFinalRenderTargetResolution(HowlApp app, Vector2Int resolution)
    {
        MonoGameApp.SetFinalRenderTargetResolution(app.MonoGameApp, resolution.X, resolution.Y);
    }

    /// <summary>
    ///     Sets the resolution of the final render target.
    /// </summary>
    /// <param name="app">the howl app instance.</param>
    /// <param name="width">the new width of the final render target.</param>
    /// <param name="height">the new height of the final render target.</param>
    /// <exception cref="ArgumentException"></exception>
    public static void SetFinalRenderTargetResolution(HowlApp app, int width, int height)
    {
        MonoGameApp.SetFinalRenderTargetResolution(app.MonoGameApp, width, height);
    }
    
    /// <summary>
    ///     Loads a texture from disc.
    /// </summary>
    /// <param name="app"></param>
    /// <param name="texturePath"></param>
    /// <param name="genIndex"></param>
    /// <returns></returns>
    /// <exception cref="FileNotFoundException"></exception>
    /// <exception cref="DirectoryNotFoundException"></exception>
    /// <exception cref="IOException"></exception>
    public static GenIndexResult LoadTexture(HowlApp app, string texturePath, out GenIndex genIndex)
    {
        return MonoGameApp.LoadTexture(app.MonoGameApp, texturePath, out genIndex);
    }

    /// <summary>
    ///     Gets the dimensions of a loaded texture.
    /// </summary>
    /// <param name="app"></param>
    /// <param name="genIndex"></param>
    /// <param name="dimensions"></param>
    /// <returns></returns>
    public static GenIndexResult GetTextureDimensions(HowlApp app, in GenIndex genIndex, out Vector2 dimensions)
    {
        return MonoGameApp.GetTextureDimensions(app.MonoGameApp, genIndex, out dimensions);
    }

    /// <summary>
    ///     Unloads a loaded texture from memory.
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public static GenIndexResult UnloadTexture(HowlApp app, in GenIndex index)
    {
        return MonoGameApp.UnloadTexture(app.MonoGameApp, index);
    }
}