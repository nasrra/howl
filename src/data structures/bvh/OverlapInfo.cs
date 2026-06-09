using System;

namespace Howl.Text.Bvh;

public ref struct OverlapInfo
{
    /// <summary>
    ///     The indices of the <c>owner</c> leaf in the overlaps.
    /// </summary>
    /// <remarks>
    ///     Remarks: Elements should be accessed via <c>overlapIndex</c>.
    /// </remarks>
    public Span<int> OwnerLeafIndices;

    /// <summary>
    ///     The indices of the <c>other</c> leaf in the overlaps. 
    /// </summary>
    /// <remarks>
    ///     Remarks: Elements should be accessed via <c>overlapIndex</c>.
    /// </remarks>
    public Span<int> OtherLeafIndices;

    /// <summary>
    ///     The length of elements in the spans of this instance.
    /// </summary>
    public int Length;

    /// <summary>
    ///     Constructs a new OverlapInfo.
    /// </summary>
    /// <param name="ownerLeafIndices">the indices of the <c>owner</c> leaf in the overlaps.</param>
    /// <param name="otherLeafIndices">the indices of the <c>other</c> leaf in the overlaps.</param>
    /// <param name="length">the length of elements in the spans of this instance.</param>
    public OverlapInfo(Span<int> ownerLeafIndices, Span<int> otherLeafIndices, int length)
    {
        OwnerLeafIndices = ownerLeafIndices;
        OtherLeafIndices = otherLeafIndices;
        Length = length;
    }
}