using System;
using System.ComponentModel.DataAnnotations;
using Howl.Math.Shapes;

namespace Howl.Math.Shapes;

public ref struct Soa_AabbSlice
{
    /// <summary>
    /// The x-components of the minimum vertex.
    /// </summary>
    public Span<float> MinX;

    /// <summary>
    /// the y-components of the minimum vertex.
    /// </summary>
    public Span<float> MinY;
    
    /// <summary>
    /// the x-components of the maximum vertex.
    /// </summary>
    public Span<float> MaxX;

    /// <summary>
    /// The y-components of the maximum vertex.
    /// </summary>
    public Span<float> MaxY;

    /// <summary>
    /// The length of all the backing spans.
    /// </summary>
    public int Length;

    /// <summary>
    /// Constructs a Structure-Of-Arrays span over the portion of the target soa, beginning at a specified position for a specified length.
    /// </summary>
    /// <param name="soa">the target soa instance to get a slice of.</param>
    /// <param name="start">the starting index.</param>
    /// <param name="length">the length of the slice.</param>
    public Soa_AabbSlice(Soa_Aabb soa, int start, int length)
    {
        MinX = soa.MinX.AsSpan(start, length);
        MinY = soa.MinY.AsSpan(start, length);
        MaxX = soa.MaxX.AsSpan(start, length);
        MaxY = soa.MaxY.AsSpan(start, length);
        Length = length;
    }
}