using System.Text.Json.Serialization;

namespace Howl.LevelManagement.Ldtk;

public class Dto_Level
{
    [JsonPropertyName("identifier")]
    public string Identifier {get; set;}

    [JsonPropertyName("layerInstances")]
    public Dto_LayerInstance[] LayerInstances {get; set;}

    [JsonPropertyName("fieldInstances")]
    public Dto_FieldInstance[] FieldInstances {get; set;}
}