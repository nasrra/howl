using System;
using Howl.ECS;

namespace Howl.Graphics;

public interface ITextureManager : IDisposable
{
    /// <summary>
    /// Loads a new Texute asset into memory.
    /// </summary>
    /// <param name="texturePath">The file path to the Texture asset; relative to AssetManager.AssetsFolder</param>
    /// <param name="genIndex">The GenIndex assigned to the texture that was loaded.</param>
    /// <returns>true, if the texture was successfully loaded; otherwise false.</returns>
    public bool LoadTexture(string texturePath, out GenIndex genIndex);

    /// <summary>
    /// Unloads a Texture asset from memeory.
    /// </summary>
    /// <param name="index">The GenIndex associated with the loaded Texture.</param>
    /// <param name="genIndexResult">The result of modifying the texture ids; may return an error description should this function fail.</param>
    /// <param name="allocatorResult">The result of modifying the loaded texture assets in memory; may return an error description should this function fail.</param>
    /// <returns>true, if the Texture was successfully unloaded; otherwise false.</returns>
    public bool UnloadTexture(in GenIndex index, out GenIndexResult genIndexResult, out AllocatorResult allocatorResult);
}