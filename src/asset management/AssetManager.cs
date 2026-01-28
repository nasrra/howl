using System.Diagnostics;
using System.IO;
using Microsoft.Xna.Framework.Graphics;

using Howl;

namespace Howl.AssetManagement;

public class AssetManager
{

    private static string assetsFolder = "assets/";

    /// <summary>
    /// Gets the folder - relative to the compiled executable - where all game assets are stored.
    /// </summary>
    public static string AssetsFolder => assetsFolder;

    private static string audioFolder = "audio/";
    
    /// <summary>
    /// Gets the audio assets path relative to the AssetsFolder.
    /// </summary>
    public static string AudioFolder => Path.Combine(AssetsFolder,audioFolder);

    private static string fontFolder => "fonts/";

    /// <summary>
    /// Gets the font folder path relative to the AssetsFolder.
    /// </summary>
    public static string FontFolder => Path.Combine(AssetsFolder, fontFolder);

    /// <summary>
    /// Sets the assets folder path relative to the compiled executable file.
    /// </summary>
    /// <param name="path">The assets folder path.</param>
    public static void SetAssetsFolderPath(string path)
    {
        assetsFolder = path;
    }

    /// <summary>
    /// Sets the audio folder path relative to the assets folder.
    /// Example:
    ///     Actual Folder Path: assets/audio assets/
    ///     Path to set: "audio assets/"
    /// </summary>
    /// <param name="path">The audio folder path.</param>
    public static void SetAudioFolderPath(string path)
    {
        audioFolder = path;
    }   

}