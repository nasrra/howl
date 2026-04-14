using System.Text.Json;
using Howl.AssetManagement;
using Howl.LevelManagement;
using Howl.LevelManagement.Ldtk;

namespace Howl.Test.LevelManagement.Ldtk;

public class Test_LdtkParser
{
    public static string projectPath = LevelManager.LevelsFolder + "test.ldtk";
    public static string Level1Path = LevelManager.LevelsFolder + "test/Level1.ldtkl";
    public static string Level2Path = LevelManager.LevelsFolder + "test/Level2.ldtkl";

    [Fact]
    public void LoadProject_Test()
    {
        bool expectedExternalLevels = true;
        Dto_ProjectLevel expectedLevel1 = new(){Identifier="Level1", ExternalRelPath="test/Level1.ldtkl"};
        Dto_ProjectLevel expectedLevel2 = new(){Identifier="Level2", ExternalRelPath="test/Level2.ldtkl"};
        Dto_ProjectLevel[] expectedLevels = [expectedLevel1, expectedLevel2];

        Dto_Project? project = LdtkParser.LoadProject(projectPath);
        
        Assert.NotNull(project);
        
        Assert_Dto_Project.Equal(expectedExternalLevels, expectedLevels, project);       
    }

    [Fact]
    public void LoadLevel_Test()
    {
        Dto_Level? level;
        string[]? texturesToLoad;

        level = LdtkParser.LoadLevel(Level1Path);
        Assert.NotNull(level);
        Assert.Equal("Level1", level.Identifier);
        texturesToLoad = JsonSerializer.Deserialize<string[]>(level.FieldInstances[0].Value);
        Assert.NotNull(texturesToLoad);
        Assert.Single(texturesToLoad);
        Assert.Equal("tilemap_packed_0.png", texturesToLoad[0]);

        level = LdtkParser.LoadLevel(Level2Path);
        Assert.NotNull(level);
        Assert.Equal("Level2", level.Identifier);
        texturesToLoad = JsonSerializer.Deserialize<string[]>(level.FieldInstances[0].Value);
        Assert.NotNull(texturesToLoad);
        Assert.Equal(2, texturesToLoad.Length);
        Assert.Equal("tilemap_packed_0.png", texturesToLoad[0]);
        Assert.Equal("tilemap_packed_1.png", texturesToLoad[1]);
    }
}