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
    /// The length of all the backing spans of this instance.
    /// </summary>
    public int Length;

    /// <summary>
    /// Construts a Structure-Of-Arays Axis-Aligned-Bounding-Box slice.
    /// </summary>
    /// <param name="soa"></param>
    /// <param name="start"></param>
    /// <param name="length"></param>
    public Soa_AabbSlice(Soa_Aabb soa, int start, int length)
    {
        MinX = soa.MinX.AsSpan(start, length);
        MinY = soa.MinY.AsSpan(start, length);
        MaxX = soa.MaxX.AsSpan(start, length);
        MaxY = soa.MaxY.AsSpan(start, length);
        Length = length;
    }
}