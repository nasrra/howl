using System.IO;
using System.Runtime.CompilerServices;
using Howl.Collections;
using Howl.Debug;
using Microsoft.Xna.Framework.Graphics;

namespace Howl.Vendors.MonoGame;

public static class TextureManager
{

    /// <summary>
    ///     Registers a texture into a texture manager state instance.
    /// </summary>
    /// <param name="state">the state instance.</param>
    /// <param name="filePath">the file path of the texture.</param>
    /// <param name="textureIndex">output for the index in the textures array assigned to the texture.</param>
    /// <returns>true, if the texture was successfully registered; otherwise false.</returns>
    public static bool RegisterTexture(TextureManagerState state, string filePath, ref int textureIndex)
    {        
        int nextTextureIndex = state.RegisteredTexturesCount+1;

        // dont register at all if the texture has already been registered.
        if (nextTextureIndex >= state.MaxTextureCount)
        {
            System.Diagnostics.Debug.Assert(false, $"Texture '{filePath}' cannot be registered as max texture count '{state.MaxTextureCount}' was exceeded.");
            return false;
        }

        if (state.FilePathToIndex.ContainsKey(filePath))
        {
            System.Diagnostics.Debug.Assert(false, $"Texture '{filePath}' was registered twice.");
            return false;
        }

        textureIndex = nextTextureIndex;
        // register an index for the texture.
        state.FilePathToIndex.Add(filePath, nextTextureIndex);
        state.RegisteredTexturesCount++;
        return true;            
    }

    /// <summary>
    ///     Loads a texture from disc into video memory.
    /// </summary>
    /// <param name="state">the texture manager state instance that contains the registered texture.</param>
    /// <param name="graphicsDevice">the graphics device used to create the texture instance.</param>
    /// <param name="filePath">the file path of the registered texture to load.</param>
    /// <returns>true; if the texture was successfully loaded; otherwise false.</returns>
    public static bool LoadTexture(TextureManagerState state, GraphicsDevice graphicsDevice, string filePath)
    {        
        if(state.FilePathToIndex.ContainsKey(filePath) == false)
        {
            Log.WriteLine(LogType.Error, $"Texture '{filePath}' cannot be loaded as it hasn't been registered");
            return false;
        }

        int textureIndex = state.FilePathToIndex[filePath];

        if (state.Textures[textureIndex] != null)
        {
            Log.WriteLine(LogType.Error, $"Texture '{filePath}' has already been loaded.");
            return false;
        }

        try
        {
            using(FileStream stream = new FileStream(filePath, FileMode.Open))
            {
                state.Textures[textureIndex] = Texture2D.FromStream(graphicsDevice, stream);                 
            }
        }
        catch(IOException e)
        {
            Log.WriteLine(LogType.Error, e.Message);
            return false;
        }

        return true;
    }

    /// <summary>
    ///     Unloads a loaded texture from video memory.
    /// </summary>
    /// <param name="state">the texture manager state that has loaded the texture.</param>
    /// <param name="filePath">the file path of the registered texture to unload.</param>
    /// <returns>true, if the texture was successfully unloaded; otherwise false.</returns>
    public static bool UnloadTexture(TextureManagerState state, string filePath)
    {
        if(state.FilePathToIndex.ContainsKey(filePath) == false)
        {
            Log.WriteLine(LogType.Error, $"Texture '{filePath}' cannot be loaded as it hasn't been registered");
            return false;
        }

        int textureIndex = state.FilePathToIndex[filePath];

        if(state.Textures[textureIndex] == null)
        {
            Log.WriteLine(LogType.Error, $"Texture '{filePath}' has already been unloaded.");
            return false;
        }

        state.Textures[textureIndex].Dispose();
        state.Textures[textureIndex] = null;

        return true;
    }

    /// <summary>
    ///     Sets the Nil texture value in a texture manager state instance.
    /// </summary>
    /// <param name="state">the state instance to set the Nil texture in.</param>
    /// <param name="graphicsDevice">the graphics device used to create the texture instance.</param>
    /// <param name="filePath">the file path of the registered texture to load.</param>
    /// <returns>true; if the texture was successfully loaded; otherwise false.</returns>
    public static bool LoadNilTexture(TextureManagerState state, MonoGameApp monoGame, string filePath)
    {
        // dispose the previous Nil texture if there was any.
        state.Textures[0]?.Dispose();

        try
        {
            using(FileStream stream = new FileStream(filePath, FileMode.Open))
            {
                state.Textures[0] = Texture2D.FromStream(monoGame.GraphicsDevice, stream);                 
            }
        }
        catch(IOException e)
        {
            Log.WriteLine(LogType.Error, e.Message);
            return false;
        }

        return true;
    }

    /// <summary>
    ///     Gets the dimensions of a loaded texture in pixels.
    /// </summary>
    /// <param name="state">the state instance that contains the loaded texture.</param>
    /// <param name="textureIndex">the index of the texture.</param>
    /// <param name="width">output for the texture width.</param>
    /// <param name="height">output for the texture height.</param>
    /// <returns>true, if the texture's dimensions were successfully retrieved otherwise false.</returns>
    public static bool GetTextureDimensions(TextureManagerState state, int textureIndex, ref int width, ref int height)
    {
        // texture isnt loaded.
        if(state.Textures[textureIndex] == null)
        {
            Log.WriteLine(LogType.Error, $"Texture '{textureIndex}' dimensions cannot be retrieved.");
            return false;
        }
        
        GetTextureDimensionsUnsafe(state, textureIndex, ref width, ref height);
        return true;
    }

    /// <summary>
    ///     Gets the dimensions of a loaded texture in pixels.
    /// </summary>
    /// <remarks>
    ///     Note: this function bypasses any null checks for the texture; meaning this may result in a crash.
    /// </remarks>
    /// <param name="state">the state instance that contains the loaded texture.</param>
    /// <param name="textureIndex">the index of the texture.</param>
    /// <param name="width">output for the texture width.</param>
    /// <param name="height">output for the texture height.</param>
    /// <returns>true, if the texture's dimensions were successfully retrieved otherwise false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
    public static void GetTextureDimensionsUnsafe(TextureManagerState state, int textureIndex, ref int width, ref int height)
    {
        ref Texture2D texture = ref state.Textures[textureIndex];
        width = texture.Width;
        height = texture.Height;
    }
}