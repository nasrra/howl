
using System;
using System.IO;
using Howl.Debug;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Howl.Vendors.MonoGame.FontStashSharp;

public static class FontManager
{
    /// <summary>
    ///     Registers a sprite front into a state instance.
    /// </summary>
    /// <param name="state">the state instance to registers into.</param>
    /// <param name="filePath">the file path of the font - relative to the working directory.</param>
    /// <param name="spriteFontIndex">output for the index in the fonts array assigned to the font.</param>
    /// <returns>true, if the texture was successfully registered; otherwise false.</returns>
    public static bool RegisterFont(FontManagerState state, string filePath, ref int spriteFontIndex)
    {
        int nextIndex = state.RegisteredCount+1;

        // dont register at all if the font has already 
        if(nextIndex >= state.MaxRegisteredCount)
        {
            System.Diagnostics.Debug.Assert(false, $"SpriteFont '{filePath}' cannot be registered as max register count '{state.MaxRegisteredCount}' was exceeded.");
            return false;
        }

        if (state.FilePathToIndex.ContainsKey(filePath))
        {
            System.Diagnostics.Debug.Assert(false, $"SpriteFont '{filePath}' was registered twice.");
            return false;
        }

        // register an index for the sprite front.
        spriteFontIndex = nextIndex;
        state.FilePathToIndex.Add(filePath, nextIndex);
        state.RegisteredCount++;
        return true;
    }

    /// <summary>
    ///     Loads a font from disc into video memory.
    /// </summary>
    /// <param name="state">the state instance that contains the registered font.</param>
    /// <param name="filePath">the file path of the registered font to load - relative to the working directory.</param>
    /// <param name="lineHeight">the desired text line height in pixels.</param>
    /// <returns>true, if the font was successfully loaded; otherwise false.</returns>
    public static bool LoadFont(FontManagerState state, string filePath, float lineHeight)
    {
        if(state.FilePathToIndex.ContainsKey(filePath) == false)
        {
            Log.WriteLine(LogType.Error, $"SpriteFont '{filePath}' cannot be loaded as it hasn't been registered.");
            return false;
        }

        int index = state.FilePathToIndex[filePath];

        if(state.Fonts[index] != null)
        {
            Log.WriteLine(LogType.Error, $"SpriteFont '{filePath}' has already been loaded.");
            return false;
        }
 
        // load the font.
        try
        {
            Font font = new();
            font.FontSystem.AddFont(File.ReadAllBytes(filePath));
            font.SpriteFontBase = font.FontSystem.GetFont(lineHeight);
            state.Fonts[index] = font;
        }
        catch(Exception e)
        {
            Log.WriteLine(LogType.Error, e.Message);
            return false;
        }
        
        return true;
    }

    /// <summary>
    ///     Unloads a loaded font from video memory.
    /// </summary>
    /// <param name="state">the state instance that contains the loaded font.</param>
    /// <param name="filePath">the file path of the registered font to unload.</param>
    /// <returns>true, if the font was successfully unloaded; otherwise false.</returns>
    public static bool UnloadFont(FontManagerState state, string filePath)
    {
        if(state.FilePathToIndex.ContainsKey(filePath) == false)
        {
            Log.WriteLine(LogType.Error, $"SpriteFont '{filePath}' cannot be unloaded as it hasn't been registered");
            return false;
        }

        int textureIndex = state.FilePathToIndex[filePath];

        if(state.Fonts[textureIndex] == null)
        {
            Log.WriteLine(LogType.Error, $"SpriteFont '{filePath}' has already been unloaded.");
            return false;
        }

        // dispose of the only unmanaged resource.
        Font.Dispose(state.Fonts[textureIndex]);
        
        // set to null so the system knows this font has been unloaded.
        state.Fonts[textureIndex] = null;
        return true;
    }

    /// <summary>
    ///     Loads and sets the Nil (fallback) font to use when failing to retrieve a valid font.
    /// </summary>
    /// <param name="state">the state instance to set the Nil font in.</param>
    /// <param name="filePath">the file path of the registered font to load - relative to the working directory.</param>
    /// <param name="lineHeight">the desired text line height in pixels.</param>
    /// <returns>true, if the font was successfully loaded; otherwise false.</returns>
    public static bool LoadNilFont(FontManagerState state, string filePath, int lineHeight)
    {
        // dispose the previous Nil texture if there was any.
        if(state.Fonts[0] != null)
        {
            Font.Dispose(state.Fonts[0]);
            state.Fonts[0] = null;
        }

        try
        {
            Font font = new();
            font.FontSystem.AddFont(File.ReadAllBytes(filePath));
            font.SpriteFontBase = font.FontSystem.GetFont(lineHeight);
            state.Fonts[0] = font;
        }
        catch(Exception e)
        {
            Log.WriteLine(LogType.Error, e.Message);
            return false;
        }

        return true;
    }

    /// <summary>
    ///     Disposes of a state instance.
    /// </summary>
    /// <param name="state">the state instance to dispose of.</param>
    /// <param name="content">the content manager where the fonts have been loaded into.</param>
    public static void Dispose(FontManagerState state)
    {
        if (state.Disposed)
        {
            return;
        }

        state.Disposed = true;
        
        // free all fonts from video memory.
        for(int i = 0; i < state.Fonts.Length; i++)
        {
            Font font = state.Fonts[i];
            if(font != null)
            {
                Font.Dispose(font);
            } 
        }
        state.Fonts = null;
        state.FilePathToIndex = null;

        state.RegisteredCount = 0;
        state.MaxRegisteredCount = 0;

        GC.SuppressFinalize(state);
    }
}