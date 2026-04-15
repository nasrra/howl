using System;
using System.IO;
using Howl.Collections;
using Howl.Debug;
using Howl.Vendors.MonoGame;
using Howl.Vendors.MonoGame.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Howl.Graphics;

public class TextureManagerNew
{
    public static bool RegisterTexture(TextureManagerNewState state, string filePath, ref int textureIndex)
    {
        // dont register at all if the texture has already been registered.
        if (state.FilePathToIndex.ContainsKey(filePath))
        {
            return false;
        }

        // register an index for the texture.
        textureIndex = state.RegisteredTexturesCount;
        state.RegisteredTexturesCount++;
        return true;
    }

    public static bool LoadTexture(TextureManagerNewState state, string filePath)
    {
        if(state.FilePathToIndex.ContainsKey(filePath) == false)
        {
            Log.WriteLine(LogType.Error, $"Texture '{filePath}' cannot be loaded as it hasn't been registered");
            return false;
        }

        int textureIndex = state.FilePathToIndex[filePath];

        if (state.Allocated[textureIndex])
        {
            Log.WriteLine(LogType.Warn, $"Texture '{filePath}' has already been loaded.");
            return false;
        }

        try
        {
            using(FileStream stream = new FileStream(filePath, FileMode.Open))
            {
                state.Textures[textureIndex] = Texture2D.FromStream(MonoGameApp.Instance.GraphicsDevice, stream);                 
            }
        }
        catch(IOException e)
        {
            Log.WriteLine(LogType.Error, e.Message);
            return false;
        }

        return true;
    }

    public static void EnforceNil(TextureManagerNewState state)
    {
        // dispose of a possible Nil write beforehand as Nil.Enforce() doesnt call
        // dispose at all. Without this, a Nil write and clear WILL cause a memory leak
        // as the graphics resource is not freed.
        state.Textures[0]?.Dispose();

        Nil.Enforce(state.Textures);
        Nil.Enforce(state.Allocated);
    }

    public static void Dispose(TextureManagerNewState state)
    {
        if (state.Disposed)
        {
            return;
        }

        state.Disposed = true;

        for(int i = 0; i < state.RegisteredTexturesCount; i++)
        {
            state.Textures[i].Dispose();
        }
        state.Textures = null;

        state.Allocated = null;
        state.FilePathToIndex = null;
        state.RegisteredTexturesCount = 0;
        
        GC.SuppressFinalize(state);
    }
}