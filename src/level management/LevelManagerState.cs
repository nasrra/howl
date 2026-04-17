using System.Collections.Generic;

namespace Howl.LevelManagement;

public class LevelManagerState
{
    public Ldtk.Dto_Project LdtkProject;
    public Dictionary<string, int> LevelNameToIndex; 

    public LevelManagerState()
    {
        LevelNameToIndex = new();
    }
}