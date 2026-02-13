using System;
using Howl.ECS;
using Howl.Generic;
using Howl.Graphics.Text;
using Microsoft.Xna.Framework.Graphics;
using static Howl.ECS.GenIndexListProc;

namespace Howl.Vendors.MonoGame.Text;

public class FontManager : IFontManager
{
    GenIndexAllocator spriteFontIds;
    GenIndexList<SpriteFont> spriteFonts;

    private MonoGameApp monoGameApp;

    private bool disposed;
    public bool IsDisposed => disposed;
    
    public FontManager(MonoGameApp monoGameApp)
    {
        spriteFontIds = new();
        spriteFonts = new();
        this.monoGameApp = monoGameApp;
    }

    private void ValidateDependencies()
    {
        if (monoGameApp.IsDisposed)
        {
            throw new ObjectDisposedException("FontManager cannot operate on/with a diposed MonoGameApp.");
        }
    }

    public void LoadFont(string fontFilePath, out GenIndex genIndex)
    {
        ValidateDependencies();

        spriteFontIds.Allocate(out genIndex, out bool reusedFreeIndex);

        if (reusedFreeIndex == false)
        {
            ResizeSparseEntries(spriteFonts, spriteFontIds.Entries.Count);
        }

        SpriteFont spriteFont = monoGameApp.Content.Load<SpriteFont>(AssetManagement.AssetManager.FontFolder+fontFilePath);

        Allocate(spriteFonts, genIndex, spriteFont);
    }

    public GenIndexResult GetFontReadOnlyRef(in GenIndex genIndex, out ReadOnlyRef<SpriteFont> readOnlyRef)
    {
        return GetDenseReadOnlyRef(spriteFonts, genIndex, out readOnlyRef);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public void Dispose(bool disposing)
    {
        if (disposed)
        {
            return;
        }

        if (disposing)
        {
            spriteFontIds.Dispose();
            spriteFontIds = null;

            // this is fine as SpriteFont does not implement a Dispose method.
            spriteFonts.Dispose();
            spriteFonts = null;
        }

        disposed = true;
    }

    public GenIndexResult IsFontLoaded(GenIndex genIndex)
    {
        return GetFontReadOnlyRef(in genIndex, out ReadOnlyRef<SpriteFont> readOnlyRef);
    }

    ~FontManager()
    {
        Dispose(false);           
    }
}