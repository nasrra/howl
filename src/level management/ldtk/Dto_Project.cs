using System.Text.Json.Serialization;

namespace Howl.LevelManagement.Ldtk;

public class Dto_Project
{
    [JsonPropertyName("externalLevels")]
    public bool ExternalLevels {get; set;}

    [JsonPropertyName("levels")]
    public Dto_ProjectLevel[] Levels{get; set;}
}