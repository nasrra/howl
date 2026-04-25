using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;

namespace Howl.Vendors.MonoGame.Graphics;

public class TextureManagerState
{
    /// <summary>
    ///     Instances of monogame texture 2Ds.
    /// </summary>
    /// <remarks>
    ///     Note: this array contains a Nil value.
    /// </remarks>
    public Texture2D[] Textures;

    /// <summary>
    ///     A mapping of all texture file paths to their respective indices within the <c>Textures</c> array. 
    /// </summary>
    public Dictionary<string, int> FilePathToIndex;

    /// <summary>
    ///     The count of textures that have been registered; starting from index 0.
    /// </summary>
    public int RegisteredCount;

    /// <summary>
    ///     The maximum amount of textures this state instance can store.
    /// </summary>
    public int MaxRegisteredCount;

    /// <summary>
    ///     Whether or not this instance has been disposed.
    /// </summary>
    public bool Disposed;

    /// <summary>
    ///     Creates a new texture manager state instance.
    /// </summary>
    /// <param name="maxTextureCount">the maximum amount of registered textures this instance can store.</param>
    public TextureManagerState(int maxTextureCount)
    {
        Textures = new Texture2D[maxTextureCount];
        FilePathToIndex = new();
        MaxRegisteredCount = maxTextureCount;
    }

    public static void Dispose(TextureManagerState state)
    {
        if (state.Disposed)
        {
            return;
        }

        state.Disposed = true;

        // free all textures from video memory.
        for(int i = 0; i < state.Textures.Length; i++)
        {
            state.Textures[i]?.Dispose();
        }
        state.Textures = null;

        state.FilePathToIndex = null;
        state.RegisteredCount = 0;
        state.MaxRegisteredCount = 0;
        
        GC.SuppressFinalize(state);
    }

    ~TextureManagerState()
    {
        Dispose(this);
    }
}