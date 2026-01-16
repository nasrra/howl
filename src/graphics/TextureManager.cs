using System;
using System.Diagnostics;
using Howl.ECS;
using Howl.Generic;

namespace Howl.Graphics;

public abstract class TextureManager<T> : ITextureManager where T : IDisposable
{
    GenIndexAllocator textureIds;
    GenIndexList<T> textures;

    public TextureManager(){
        textureIds = new();
        textures = new();
    }  

    public bool LoadTexture(string texturePath, out GenIndex genIndex)
    {
        AllocatorResult result = textureIds.Allocate(out genIndex);
        if(result != AllocatorResult.AllocatedNewGenIndex)
        {
            return false;
        }

        // resize the sparse entries to match the texture ids so every texture id has a possible entry point
        // into the textures storage.
        textures.ResizeSparseEntries(textureIds.Entries.Count);
        
        if(LoadTextureFromDisc(texturePath, out T texture) == false)
        {
            return false;
        }

        textures.Allocate(genIndex, texture);
        return true;
    }

    public abstract bool LoadTextureFromDisc(string texturePath, out T texture);

    public bool UnloadTexture(in GenIndex index, out GenIndexResult genIndexResult, out AllocatorResult allocatorResult)
    {
        // ensure to dispose of the monogame texture before deallocating it.

        textures.GetDenseRef(index, out Ref<T> reference);
        reference.Value.Dispose();
        
        genIndexResult = textures.Deallocate(index);
        if(genIndexResult == GenIndexResult.Success)
        {
            allocatorResult = textureIds.Deallocate(index);
            if(allocatorResult == AllocatorResult.DeallocatedGenIndex)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        else
        {
            allocatorResult = AllocatorResult.InvalidGenIndex;
            return false;
        }
    }

    public ReadonlyRef<T> GetTextureReadonlyRef(in GenIndex index)
    {
        GenIndexResult result = textures.GetDenseReadonlyRef(index, out ReadonlyRef<T> readonlyRef);
        if(result == GenIndexResult.Success)
        {
            return readonlyRef;
        }
        else
        {
            Debug.WriteLine($"Failed get readonly ref to texture {index} error code {result}");
            return default;
        }
    }
}