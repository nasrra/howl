using System.IO;
using Howl.AssetManagement;
using Howl.LevelManagement.Ldtk;

namespace Howl.LevelManagement;

public static class LevelManager
{
    public static string LevelsFolder = AssetManager.AssetsFolder + "levels/";

    public static bool LoadProject(LevelManagerState state, string projectPath)
    {
        string path = AssetManagement.AssetManager.AssetsFolder + projectPath;

        Dto_Project project = LdtkParser.LoadProject(path);
        
        if(project == null)
        {
            return false;
        }

        state.LdtkProject = project;

        // store the loaded level strings with their indices so that the user can
        // use only the name of the level when loading; instead of having to know the
        // unstable index undereath that ldtk changes depending on how the project is structured.
        for(int i = 0; i < project.Levels.Length; i++)
        {
            state.LevelNameToIndex.Add(project.Levels[i].Identifier, i);
        }

        return true;
    }

    public static bool LoadLevel(LevelManagerState state, string levelName)
    {
        if (state.LevelNameToIndex.ContainsKey(levelName) == false)
        {
            return false;
        }

        int levelIndex = state.LevelNameToIndex[levelName];

        string path = LevelsFolder+state.LdtkProject.Levels[levelIndex].ExternalRelPath;

        Dto_Level level = LdtkParser.LoadLevel(path);

        // do stuff with the level.

        if(level == null)
        {
            return false;
        }

        // should return a gen id for the newly allocated level.
        return true;
    }
}