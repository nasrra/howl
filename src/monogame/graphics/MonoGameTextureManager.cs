using System;
using System.Diagnostics;
using System.IO;
using Howl.Graphics;
using Microsoft.Xna.Framework.Graphics;

namespace Howl.MonoGame.Graphics;

public sealed class MonoGameTextureManager : TextureManager<Texture2D>
{
    WeakReference<MonoGameApp> monogameApp;

    public MonoGameTextureManager(WeakReference<MonoGameApp> monogameApp) : base()
    {
        this.monogameApp = monogameApp; 
    }

    private MonoGameApp GetMonoGameApp()
    {
        if(monogameApp.TryGetTarget(out MonoGameApp app))
        {
            return app;
        }
        else
        {
            throw new NullReferenceException("MonoGameTextureManager cannot operate on a MonoGameApp that is null.");
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
        MonoGameApp app = GetMonoGameApp();

        texture = null;

        string path = Path.Combine(AssetManagement.AssetManager.AssetsFolder, texturePath);
        try
        {
            using(FileStream stream = new FileStream(path, FileMode.Open))
            {
                texture = Texture2D.FromStream(app.GraphicsDevice, stream);
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