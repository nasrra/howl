using System;
using System.Diagnostics;
using System.IO;
using Howl.Graphics;
using Microsoft.Xna.Framework.Graphics;

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
    public override bool LoadTextureFromDisc(string texturePath, out Texture2D texture)
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
            Debug.WriteLine($"Texture2D file not found: {path}");
            return false;
        }
        catch (DirectoryNotFoundException)
        {
            Debug.WriteLine($"Directory not found: {path}");
            return false;
        }
        catch(IOException e)
        {
            Debug.WriteLine($"Error reading file: {path}: {e.Message}");
            return false;
        }
        
        return true;
    }
}