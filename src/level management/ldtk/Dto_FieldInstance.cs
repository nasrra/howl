using System.Text.Json;
using System.Text.Json.Serialization;

namespace Howl.LevelManagement.Ldtk;

public class Dto_FieldInstance
{
    [JsonPropertyName("__value")]
    public JsonElement Value {get; set;}
}