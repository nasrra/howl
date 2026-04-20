using System;

namespace Howl.LevelManagement.Ldtk;

public ref struct IntGridView
{
    /// <summary>
    ///     The string identifier/name of the int grid.
    /// </summary>
    public string Identifier;

    /// <summary>
    ///     The integer values of all cells in the IntGrid.
    /// </summary>
    public Span<int> CellValues;

    /// <summary>
    ///     The amount of cells along the horizontal axis.
    /// </summary>
    public int Width;
    
    /// <summary>
    ///     The amount of cells along the vertical axis.
    /// </summary>
    public int Height;

    /// <summary>
    ///     The width and height - in pixels - of each cell.
    /// </summary>
    public int CellSize;

    /// <summary>
    ///     Constructs a new IntGridView.
    /// </summary>
    /// <param name="identifier">the identifier/name of the int grid.</param>
    /// <param name="cellValues">the integer values of each cell.</param>
    /// <param name="width">the amount of cells along the horizontal axis.</param>
    /// <param name="height">the width and height - in pixels - of each cell.</param>
    /// <param name="cellSize">the width and height - in pixels - of each cell.</param>
    public IntGridView(string identifier, Span<int> cellValues, int width, int height, int cellSize)
    {
        Identifier = identifier;
        CellValues = cellValues;
        Width = width;
        Height = height;
        CellSize = cellSize;
    }
}