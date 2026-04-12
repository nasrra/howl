using System;
using System.Runtime.CompilerServices;

namespace Howl.DataStructures.Bvh;

/// <summary>
///     Data is structured in a SOA format.
/// </summary>
public class Soa_Overlap
{
    /// <summary>
    ///     The leaf indices of the <c>owner</c> of an overlap between to leaves.
    /// </summary>
    /// <remarks>
    ///     Use a <c>overlapIndex</c> integer to access elements.
    /// </remarks>
    public int[] OwnerLeafIndices;

    /// <summary>
    ///     The leaf indices of the <c>owner</c> of an overlap between to leaves.
    /// </summary>
    /// <remarks>
    ///     Use a <c>overlapIndex</c> integer to access elements.
    /// </remarks>
    public int[] OtherLeafIndices;

    /// <summary>
    ///     The count of allocated entries from appending.
    /// </summary>
    public int AppendCount;

    /// <summary>
    ///     The length of all the backing arrays of this instance.
    /// </summary>
    public int Length;

    /// <summary>
    ///     Whether or not this instance has been disposed.
    /// </summary>
    public bool Disposed;

    /// <summary>
    ///     Creates a new Overlapsoa  instance.
    /// </summary>
    /// <param name="length">the maximum amount of overlaps this instance can hold; i.e. the length of the backing arrays.</param>
    public Soa_Overlap(int length)
    {
        OwnerLeafIndices = new int[length];
        OtherLeafIndices = new int[length];
        Length = length;
    }

    /// <summary>
    ///     Appends a overlap to a overlap soa instance.
    /// </summary>
    /// <param name="soa ">the overlap soa  instance to append to.</param>
    /// <param name="ownerLeafIndex">the leaf index of the <c>owner</c> of the overlap.</param>
    /// <param name="otherleafIndex">the leaf index of the <c>other</c> of the overlap.</param>
    public static void Append(Soa_Overlap soa , int ownerLeafIndex, int otherleafIndex)
    {
        int index = soa.AppendCount;
        soa.OwnerLeafIndices[index] = ownerLeafIndex;
        soa.OtherLeafIndices[index] = otherleafIndex;
        soa.AppendCount++;
    }

    /// <summary>
    ///     Sets the append count of an overlap soa instance to zero. 
    /// </summary>
    /// <param name="soa ">the overlap soa  to clear.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void ClearAppendCount(Soa_Overlap soa)
    {
        soa.AppendCount = 0;
    }

    public static void Dispose(Soa_Overlap soa)
    {
        if (soa .Disposed)
        {
            return;
        }

        soa .Disposed = true;

        soa .OwnerLeafIndices = null;
        soa .OtherLeafIndices = null;

        soa .AppendCount = 0;
        soa .Length = 0;

        GC.SuppressFinalize(soa );
    }

    ~Soa_Overlap()
    {
        Dispose(this);
    }
}