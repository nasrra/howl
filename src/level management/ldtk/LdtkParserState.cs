using System;
using System.Collections.Generic;
using Howl.DataStructures;
using Howl.LevelManagement.Ldtk;

namespace Howl.LevelManagement;

public unsafe class LdtkParserState
{
    /// <summary>
    ///     The loaded Ldtk Project.
    /// </summary>
    public Ldtk.Dto_Project Project;

    /// <summary>
    ///     The project directory path relative to the C# working directory.
    /// </summary>
    public string ProjectDirectoryPath;

    /// <summary>
    ///     The mapping of ldtk project level identifiers to their indices in the <c>Project</c>.
    /// </summary>
    public Dictionary<string, int> LevelIdentifierToIndex; 

    /// <summary>
    ///     The byte buffer used/reused when loading file data from disc.
    /// </summary>
    public byte[] scratchBuffer;

    /// <summary>
    ///     The pixels per unit of measurement (one meter).
    /// </summary>
    public int PixelsPerUnit;

    /// <summary>
    ///     The function pointer to user-defined code for parsing Level IntGrid data.
    /// </summary>
    public delegate* <HowlApp, IntGridView, void> ParseLevelIntGrid;

    /// <summary>
    ///     Whether this instance has been disposed.
    /// </summary>
    public bool Disposed;

    /// <summary>
    ///     Creates a new LdtkParserState instance.
    /// </summary>
    /// <param name="parseLevelIntGrid">The function pointer to user-defined code for parsing Level IntGrid data</param>
    /// <param name="scratchBufferSizeInMb">the size of the scratch buffer used when read from disc.</param>
    /// <param name="pixelsPerUnit">The pixels per unit of measurement (one meter).</param>
    public LdtkParserState(delegate* <HowlApp, IntGridView, void> parseLevelIntGrid, float scratchBufferSizeInMb, int pixelsPerUnit)
    {
        LevelIdentifierToIndex = new();
        PixelsPerUnit = pixelsPerUnit;
        ParseLevelIntGrid = parseLevelIntGrid;
        scratchBuffer = new byte[(int)(1000000*scratchBufferSizeInMb)];
    }

    /// <summary>
    ///     Disposes a state instance.
    /// </summary>
    /// <param name="state">the state instance to dispose.</param>
    public static void Dispose(LdtkParserState state)
    {
        if (state.Disposed)
        {
            return;
        }
        state.Disposed = true;
        state.Project = null;
        state.LevelIdentifierToIndex.Clear();
        state.LevelIdentifierToIndex = null;
        state.scratchBuffer = null;
        state.PixelsPerUnit = 0;
        GC.SuppressFinalize(state);
    }

    ~LdtkParserState()
    {
        Dispose(this);
    }
}

