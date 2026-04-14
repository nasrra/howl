using System.Text.Json.Serialization;

namespace Howl.LevelManagement.Ldtk;

public class Dto_AutoLayerTile
{
    [JsonPropertyName("px")]
    public int[] Px {get; set;} // [x,y]

    [JsonPropertyName("src")]
    public int[] Src {get; set;} // [x,y]
}