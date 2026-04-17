using System.Text.Json.Serialization;

namespace Howl.LevelManagement.Ldtk;

public class Dto_LayerInstance
{
    [JsonPropertyName("__tilesetRelPath")]
    public string TilesetRelPath {get; set;}

    [JsonPropertyName("autoLayerTiles")]
    public Dto_AutoLayerTile[] AutoLayerTiles {get; set;}
}