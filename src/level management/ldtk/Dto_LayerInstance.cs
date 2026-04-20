using System.Text.Json.Serialization;

namespace Howl.LevelManagement.Ldtk;

public class Dto_LayerInstance
{

    /// <summary>
    ///     The identifier/name of the layer.
    /// </summary>
    [JsonPropertyName("__identifier")]
    public string Identifier {get; set;}

    /// <summary>
    ///     The file path to the tileset used; relative to the ldtk project.
    /// </summary>
    /// <remarks>
    ///     This can be <c>Null</c> if the layer instance does not use a tileset.
    /// </remarks>
    [JsonPropertyName("__tilesetRelPath")]
    public string TilesetRelPath {get; set;}

    /// <summary>
    ///     The auto layer tiles data.
    /// </summary>
    /// <remarks>
    ///     This can be <c>Null</c> if the layer instance does not use auto layering tilesets.
    /// </remarks>
    [JsonPropertyName("autoLayerTiles")]
    public Dto_AutoLayerTile[] AutoLayerTiles {get; set;}

    /// <summary>
    ///     The int grid csv data.
    /// </summary>
    /// <remarks>
    ///     
    /// </remarks>
    [JsonPropertyName("intGridCsv")]
    public int[] IntGridCsv {get; set;}

    /// <summary>
    ///     The size of a cell in the grid.
    /// </summary>
    [JsonPropertyName("__gridSize")]
    public int GridSize {get; set;}

    /// <summary>
    ///     The amount of cells along the x-axis.
    /// </summary>
    [JsonPropertyName("__cWid")]
    public int Width{get; set;}

    /// <summary>
    ///     The amount of cells along the y-axis.
    /// </summary>
    [JsonPropertyName("__cHei")]
    public int Height{get; set;}

    /// <summary>
    ///     Whether or not this instance has been disposed.
    /// </summary>
    public bool Disposed;

    public static void Dispose(Dto_LayerInstance layer)
    {
        layer.Disposed = true;
    }

    ~Dto_LayerInstance()
    {
        Dispose(this);
    }
}