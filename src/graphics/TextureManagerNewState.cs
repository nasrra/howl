using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;

namespace Howl.Graphics;

public class TextureManagerNewState
{
    public Texture2D[] Textures;
    public bool[] Allocated;
    public Dictionary<string, int> FilePathToIndex;
    public int RegisteredTexturesCount;
    public int MaxTextureCount;
    public bool Disposed;

    public TextureManagerNewState(int maxTextureCount)
    {
        Textures = new Texture2D[maxTextureCount];
        Allocated = new bool[maxTextureCount];
        FilePathToIndex = new();
        MaxTextureCount = maxTextureCount;
    }

    ~TextureManagerNewState()
    {
        TextureManagerNew.Dispose(this);
    }
}