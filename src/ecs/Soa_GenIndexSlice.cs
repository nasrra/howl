using System;
using Howl.Ecs;

namespace Howl.Math.Shapes;

public ref struct Soa_GenIndexSlice
{
    /// <summary>
    /// The indices.
    /// </summary>
    public Span<int> Indices;

    /// <summary>
    /// The generations.
    /// </summary>
    public Span<int> Generations;

    /// <summary>
    /// The length of all the backing spans.
    /// </summary>
    public int Length;

    /// <summary>
    /// Constructs a Structure-Of-Arrays span over the portion of the target soa beginning at a specified position for a specified length.
    /// </summary>
    /// <param name="soa">the target soa instance to get a slice of.</param>
    /// <param name="start">the starting index.</param>
    /// <param name="length">the length of the slice.</param>
    public Soa_GenIndexSlice(Soa_GenIndex soa, int start, int length)
    {
        Indices = soa.Indices.AsSpan(start, length);
        Generations = soa.Generations.AsSpan(start, length);
        Length = length;
    }
}