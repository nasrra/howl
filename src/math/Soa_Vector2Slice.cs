using System;

namespace Howl.Math;

public ref struct Soa_Vector2Slice
{
    /// <summary>
    /// The x-coordinate values.
    /// </summary>
    public Span<float> X;

    /// <summary>
    /// The y-coordinate values.
    /// </summary>
    public Span<float> Y;

    /// <summary>
    /// The length of all the backing arrays of this instance.
    /// </summary>
    public int Length;

    /// <summary>
    /// Constructs a Structure-Of-Arrays span over the portion of the target soa beginning at a specified position for a specified length.
    /// </summary>
    /// <param name="soa">the target soa instance to get a slice of.</param>
    /// <param name="start">the starting index.</param>
    /// <param name="length">the length of the slice.</param>
    public Soa_Vector2Slice(Soa_Vector2 soa, int start, int length)
    {
        X = soa.X.AsSpan(start, length);
        Y = soa.Y.AsSpan(start, length);
        Length = length;
    }
}