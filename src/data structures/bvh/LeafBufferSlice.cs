using System;
using Howl.Math;
using Howl.Math.Shapes;

namespace Howl.DataStructures.Bvh;

public ref struct LeafBufferSlice
{
    /// <summary>
    /// The Axis-Aligned Bounding-Boxes.
    /// </summary>
    public Soa_AabbSlice Aabbs;

    /// <summary>
    /// The gen indices of the data associated with a leaf.
    /// </summary>
    public Soa_GenIndexSlice GenIndices;
    
    /// <summary>
    /// The user-defined flags.
    /// </summary>
    public Span<int> Flags;

    /// <summary>
    /// The length of all the backing spans of this slice.
    /// </summary>
    public int Length;

    /// <summary>
    /// Constructs a Structure-Of-Arrays span over the portion of the target buffer, beginning at a specified position for a specified length.
    /// </summary>
    /// <param name="buffer">the target buffer instance to get a slice of.</param>
    /// <param name="start">the starting index.</param>
    /// <param name="length">the length of the slice.</param>
    public LeafBufferSlice(LeafBuffer buffer, int start, int length)
    {
        Aabbs = new(buffer.Aabbs, start, length);
        GenIndices = new(buffer.GenIndices, start, length);
        Flags = MemoryExtensions.AsSpan(buffer.Flags, start, length);
    }
}