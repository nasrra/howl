using System;
using System.IO;
using Howl.DataStructures;

namespace Howl.Io;

/// <summary>
///     All functions return a universal path that works on both Windows and Linux.
/// </summary>
/// <remarks>
///     The universal path will always be formatted using '/' for unix operating systems, replacing windows '\'.
/// </remarks>
public static class PathUtils
{
    /// <summary>
    ///     Flattens a file path that contains relative path traversal.
    /// </summary>
    /// <remarks>
    ///     Example file path: "assets/levels/../textures/image.png" flattens to "assets/textures/image.png"
    /// <remarks>
    /// <param name="basePath">the base folder path: e.g. "assets/textures/player/"</param>
    /// <param name="relativePath">the relative file path: e.g. "../../fonts/cool-font.ttf"</param>
    /// <returns>the flattened file path.</returns>
    public static string FlattenPath(string basePath, string relativePath)
    {
        // Resolve the path logic
        string fullPath = Path.GetFullPath(Path.Combine(basePath, relativePath.TrimStart('/')));
        string cleanPath = Path.GetRelativePath(Directory.GetCurrentDirectory(), fullPath);
        UniversifyPath(cleanPath);        
        return cleanPath;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    public static string GetDirectoryName(string filePath)
    {
        string directoryPath = Path.GetDirectoryName(filePath);
        UniversifyPath(directoryPath);
        return directoryPath;
    }
    
    /// <summary>
    ///     Converts the forward slashes of a file path to always be backward slashes.
    /// </summary>
    /// <remarks>
    ///     This is done to ensure that file path strings are cross platform between linux and windows.
    /// </remarks>
    /// <param name="filePath"></param>
    public static void UniversifyPath(string filePath)
    {
        filePath.Replace('\\', '/');
    }
}