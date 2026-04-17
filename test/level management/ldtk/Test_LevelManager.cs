using Howl.LevelManagement;
using Howl.LevelManagement.Ldtk;

namespace Howl.Test.LevelManagement;

public class Test_LevelManager
{
    public const string ProjectName =  "levels/test.ldtk";
    public const string Level1Name = "Level1";
    public const string Level2Name = "Level2";

    [Fact]
    public void LoadProject_Test()
    {
        LevelManagerState state = new();
        Assert.True(LevelManager.LoadProject(state, ProjectName));
        Assert.True(state.LevelNameToIndex.ContainsKey(Level1Name));
        Assert.True(state.LevelNameToIndex.ContainsKey(Level2Name));
    }

    [Fact]
    public void LoadLevel_Test()
    {
        LevelManagerState state = new();
        LevelManager.LoadProject(state, ProjectName);

        Assert.True(LevelManager.LoadLevel(state, Level1Name));        
    }
}