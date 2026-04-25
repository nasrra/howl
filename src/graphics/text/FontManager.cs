namespace Howl.Graphics.Text;

public static class FontManager
{
    /// <summary>
    ///     Registers a front into a font manager state instance.
    /// </summary>
    /// <param name="app">the howl app instance to register a font into.</param>
    /// <param name="filePath">the file path of the font - relative to the working directory.</param>
    /// <param name="fontId">output for the id of the newly registered font.</param>
    /// <returns>true, if the texture was successfully registered; otherwise false.</returns>
    public static bool RegisterFont(HowlApp app, string filePath, ref int fontId)
    {
        return Vendors.MonoGame.FontStashSharp.FontManager.RegisterFont(app.MonoGameAppState.FontManagerState, filePath, ref fontId);
    }

    /// <summary>
    ///     Loads a registered font from disc into video memory.
    /// </summary>
    /// <param name="app">the howl app instance to load a font into.</param>
    /// <param name="filePath">the file path of the registered font to load - relative to the working directory.</param>
    /// <param name="lineHeight">the desired text line height in pixels.</param>
    /// <returns>true, if the font was successfully loaded; otherwise false.</returns>
    public static bool LoadFont(HowlApp app, string filePath, int lineHeight)
    {
        return Vendors.MonoGame.FontStashSharp.FontManager.LoadFont(app.MonoGameAppState.FontManagerState, filePath, lineHeight);
    }

    /// <summary>
    ///     Unloads a loaded font from video memory.
    /// </summary>
    /// <param name="app">the howl app instance containing the font to unload.</param>
    /// <param name="filePath">the file path of the registered font to unload - relative to the working directory.</param>
    /// <returns>true, if the font was successfully unloaded; otherwise false.</returns>
    public static bool UnloadFont(HowlApp app, string filePath)
    {
        return Vendors.MonoGame.FontStashSharp.FontManager.UnloadFont(app.MonoGameAppState.FontManagerState, filePath);
    }

    /// <summary>
    ///     Loads and sets the Nil (fallback) font to use when failing to retrieve a valid font.
    /// </summary>
    /// <param name="app">the howl app instance to load the fallback font into.</param>
    /// <param name="filePath">the file path of the fallback font to load - relative to the working directory.</param>
    /// <param name="lineHeight">the desired text line height in pixels.</param>
    /// <returns></returns>
    public static bool LoadNilFont(HowlApp app, string filePath, int lineHeight)
    {
        return Vendors.MonoGame.FontStashSharp.FontManager.LoadNilFont(app.MonoGameAppState.FontManagerState, filePath, lineHeight);
    }

    /// <summary>
    ///     Gets whether or not a font has been loaded.
    /// </summary>
    /// <param name="app">the howl app with the loaded font.</param>
    /// <param name="fontId">the id of the font.</param>
    /// <returns>true, if the font is loaded; otherwise false.</returns>
    public static bool IsFontLoaded(HowlApp app, int fontId)
    {
        return app.MonoGameAppState.FontManagerState.Fonts[fontId] != null;
    }
}