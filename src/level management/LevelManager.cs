using System;
using System.IO;
using Howl.AssetManagement;
using Howl.DataStructures;
using Howl.LevelManagement.Ldtk;

namespace Howl.LevelManagement;

public static class LevelManager
{
    public static string LevelsFolder = AssetManager.AssetsFolder + "levels/";

    /// <summary>
    ///     Loads an level management project file into a howl app.
    /// </summary>
    /// <param name="app">the howl app instance to load the project into.</param>
    /// <param name="path">the file path relative to <c><see cref="LevelsFolder"/></c></param>
    /// <returns>true, if the project was loaded successfully; otherwise false.</returns>
    public static bool LoadProject(HowlApp app, string projectPath)
    {
        return LdtkParser.LoadProject(app.LdtkParserState, projectPath);
    }

    /// <summary>
    ///     Loads a level from disc and parses it into a Howl application.
    /// </summary>
    /// <param name="app">the howl application instance to parse into.</param>
    /// <param name="levelIdentifier">the name of the level.</param>
    /// <returns>true, if the level was successfully loaded; otherwise false.</returns>
    public static bool LoadLevel(HowlApp app, string levelIdentifier)
    {
        return LdtkParser.LoadLevel(app, app.LdtkParserState, levelIdentifier);
    }
}