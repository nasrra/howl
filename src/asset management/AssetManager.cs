using System.Diagnostics;
using System.IO;
using Microsoft.Xna.Framework.Graphics;

using Howl;

namespace Howl.AssetManagement;

public class AssetManager
{
    private const string AssetsFolder = "assets/";

    /// <summary>
    /// Loads a Monogame Texture2D from disc.
    /// </summary>
    /// <param name="texturePath">The file path relative to the .csproj</param>
    /// <param name="texture">The texture2D that has been created from the file stream.</param>
    /// <returns>true, if successfully loaded; otherwise false.</returns>

    public static bool LoadTexture2D(string texturePath, out Texture2D texture)
    {
        texture = null;

        string path = Path.Combine(AssetsFolder, texturePath);
        try
        {
            using(FileStream stream = new FileStream(path, FileMode.Open))
            {
                texture = Texture2D.FromStream(HowlApp.GraphicsDevice, stream);
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