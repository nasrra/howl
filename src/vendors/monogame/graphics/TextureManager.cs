using System;
using System.Diagnostics;
using System.IO;
using Howl.ECS;
using Howl.Generic;
using Howl.Graphics;
using Howl.Math;
using Microsoft.Xna.Framework.Graphics;
using static Howl.ECS.GenIndexListProc;

namespace Howl.Vendors.MonoGame.Graphics;

public sealed class TextureManager : TextureManager<Texture2D>
{
    MonoGameApp monoGameApp;

    public TextureManager(MonoGameApp monoGameApp) : base()
    {
        this.monoGameApp = monoGameApp; 
    }

    private void ValidateDependencies()
    {
        if (monoGameApp.IsDisposed)
        {
            throw new ObjectDisposedException("TextureManager cannot operate on/with a disposed MonoGameApp.");
        }
    }

    /// <summary>
    /// Loads a Monogame Texture2D from disc.
    /// </summary>
    /// <param name="texturePath">The file path relative to the .csproj</param>
    /// <param name="texture">The texture2D that has been created from the file stream.</param>
    /// <returns>true, if successfully loaded; otherwise false.</returns>
    public override void LoadTextureFromDisc(string texturePath, out Texture2D texture)
    {
        ValidateDependencies();

        texture = null;

        string path = Path.Combine(AssetManagement.AssetManager.AssetsFolder, texturePath);
        try
        {
            using(FileStream stream = new FileStream(path, FileMode.Open))
            {
                texture = Texture2D.FromStream(monoGameApp.GraphicsDevice, stream);
            }
        }
        catch (FileNotFoundException)
        {
            throw new FileNotFoundException($"Texture2D file not found: {path}");
        }
        catch (DirectoryNotFoundException)
        {
            throw new DirectoryNotFoundException($"Directory not found: {path}");
        }
        catch(IOException e)
        {
            throw new IOException($"Error reading file: {path}: {e.Message}");
        }
    }

    public override GenIndexResult GetTextureDimensions(in GenIndex genIndex, out Vector2 dimensions)
    {
        GenIndexResult result = GetDenseReadOnlyRef(textures, genIndex, out ReadOnlyRef<Texture2D> textureRef);
        
        dimensions = result == GenIndexResult.Ok
        ? new(textureRef.Value.Width, textureRef.Value.Height)
        : default;

        return result;
    }
}