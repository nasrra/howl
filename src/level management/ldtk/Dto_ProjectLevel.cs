using System.Text.Json.Serialization;

namespace Howl.LevelManagement.Ldtk;

public class Dto_ProjectLevel
{
    [JsonPropertyName("identifier")]
    public string Identifier{get; set;}

    [JsonPropertyName("externalRelPath")]
    public string ExternalRelPath{get; set;}
}