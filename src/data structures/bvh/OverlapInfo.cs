using System;

namespace Howl.DataStructures.Bvh;

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
    ///     Constructs a new OverlapInfo.
    /// </summary>
    /// <param name="ownerLeafIndices">the indices of the <c>owner</c> leaf in the overlaps.</param>
    /// <param name="otherLeafIndices">the indices of the <c>other</c> leaf in the overlaps.</param>
    public OverlapInfo(Span<int> ownerLeafIndices, Span<int> otherLeafIndices)
    {
        OwnerLeafIndices = ownerLeafIndices;
        OtherLeafIndices = otherLeafIndices;
    }
}