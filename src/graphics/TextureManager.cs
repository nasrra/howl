using System;
using System.Diagnostics;
using Howl.ECS;
using Howl.Generic;
using Howl.Math;

namespace Howl.Graphics;

public abstract class TextureManager<T> : ITextureManager where T : IDisposable
{
    protected GenIndexAllocator textureIds;
    protected GenIndexList<T> textures;

    protected bool disposed;
    public bool IsDisposed => disposed;

    public TextureManager(){
        textureIds = new();
        textures = new();
    }  

    public GenIndexResult LoadTexture(string texturePath, out GenIndex genIndex)
    {
        textureIds.Allocate(out genIndex, out bool reusedFreeGenIndex);
        if(reusedFreeGenIndex == false)
        {
            // resize the sparse entries to match the texture ids so every texture id has a possible entry point
            // into the textures storage.
            GenIndexListProc.ResizeSparseEntries(textures, textureIds.Entries.Count);
        }

        LoadTextureFromDisc(texturePath, out T texture);
        
        GenIndexListProc.Allocate(textures, genIndex, texture).Ok(out GenIndexResult result);
        return result;
    }

    public abstract void LoadTextureFromDisc(string texturePath, out T texture);

    public GenIndexResult UnloadTexture(in GenIndex index)
    {
        // ensure to dispose of the monogame texture before deallocating it.
        GenIndexResult result;

        if(GenIndexListProc.GetDenseRef(textures, index, out Ref<T> reference).Fail(out result))
            goto Fail;

        reference.Value.Dispose();
        
        if(GenIndexListProc.Deallocate(textures, index).Fail(out result))
            goto Fail;

        if(textureIds.Deallocate(index).Fail(out result))
            goto Fail;

        return result;

        Fail:
            return result;
    }

    public GenIndexResult GetTextureReadonlyRef(in GenIndex index, out ReadOnlyRef<T> readOnlyRef)
    {
        return GenIndexListProc.GetDenseReadOnlyRef(textures, index, out readOnlyRef);
    }

    public abstract GenIndexResult GetTextureDimensions(in GenIndex genIndex, out Vector2 dimensions);

    /// 
    /// Disposal.
    /// 


    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected void Dispose(bool disposing)
    {
        if (disposed)
        {
            return;
        }

        if (disposing)
        {
            Span<DenseEntry<T>> span = GenIndexListProc.GetDenseAsSpan(textures);

            for(int i= 0; i < span.Length; i++)
            {
                span[i].Value.Dispose();
            }

            textures = null;

            textureIds.Dispose();

            textureIds = null;
        }

        disposed = true;
    }

    ~TextureManager()
    {
        Dispose(false);        
    }
}