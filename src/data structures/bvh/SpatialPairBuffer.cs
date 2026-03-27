using System;
using System.Runtime.CompilerServices;
using Howl.ECS;

namespace Howl.DataStructures.Bvh;

public class SpatialPairBuffer : IDisposable
{
    /// <summary>
    /// The gen-index for data assocaited with the 'owners' of all spatial pairs.
    /// </summary>
    public Soa_GenIndex OwnerGenIndices;

    /// <summary>
    /// The gen-index for the data associated with the 'others' of all spatial pairs.
    /// </summary>
    public Soa_GenIndex OtherGenIndices;

    /// <summary>
    /// The user-defined flags for the 'owners' of all spatial pairs.
    /// </summary>
    public int[] OwnerFlags;

    /// <summary>
    /// The user-defined flags for the 'others' of all spatial pairs.
    /// </summary>
    public int[] OtherFlags;

    /// <summary>
    /// The count of allocated entries; starting from index 0.
    /// </summary>
    public int Count;

    /// <summary>
    /// The length of all the backing arrays of this instance.
    /// </summary>
    public int Length;

    /// <summary>
    /// Whether this instance has been disposed.
    /// </summary>
    public bool Disposed;

    /// <summary>
    /// Creates a new spatial pair bufffer instance.
    /// </summary>
    /// <param name="length">the length of the backing arrays.</param>
    public SpatialPairBuffer(int length)
    {
        OwnerGenIndices = new(length);
        OtherGenIndices = new(length);
        OwnerFlags = new int[length];
        OtherFlags = new int[length];
        Length = length;
    }

    /// <summary>
    /// Appends a spatial pair to a buffer.
    /// </summary>
    /// <param name="buffer">the buffer to append to.</param>
    /// <param name="ownerIndex">the index of the data associated with the spatial pair's 'owner'.</param>
    /// <param name="ownerGeneration">the generation of the data assocaited with the spatial pair's 'owner'.</param>
    /// <param name="ownerFlags">the user-defined flags of the spatial pair's 'owner'.</param>
    /// <param name="otherIndex">the index of the data associated with the spatial pair's 'other'.</param>
    /// <param name="otherGeneration">the generation of the data assocaited with the spatial pair's 'other'.</param>
    /// <param name="otherFlags">the user-defined flags of the spatial pair's 'other'.</param>
    public static void Append(SpatialPairBuffer buffer, int ownerIndex, int ownerGeneration, int ownerFlags, int otherIndex, int otherGeneration,
        int otherFlags
    )
    {
        int count = buffer.Count;
        buffer.OwnerGenIndices.Indices[count] = ownerIndex;
        buffer.OwnerGenIndices.Generations[count] = ownerGeneration;
        buffer.OwnerFlags[count] = ownerFlags;
        buffer.OtherGenIndices.Indices[count] = otherIndex;
        buffer.OtherGenIndices.Generations[count] = otherGeneration;
        buffer.OtherFlags[count] = otherFlags;
        buffer.Count++;
    }

    /// <summary>
    /// Sets the count of a buffer to zero.
    /// </summary>
    /// <param name="buffer">the buffer to clear.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void Clear(SpatialPairBuffer buffer)
    {
        buffer.Count = 0;
    }




    /*******************
    
        Disposal.
    
    ********************/




    public void Dispose()
    {
        Dispose(this);
    }

    public static void Dispose(SpatialPairBuffer buffer)
    {
        if(buffer.Disposed)
            return;

        buffer.Disposed = true;
        Soa_GenIndex.Dispose(buffer.OwnerGenIndices);
        buffer.OwnerGenIndices = null;
        Soa_GenIndex.Dispose(buffer.OtherGenIndices);
        buffer.OtherGenIndices = null;
        buffer.OwnerFlags = null;
        buffer.OtherFlags = null;
        buffer.Count = 0;
        buffer.Length = 0;

        GC.SuppressFinalize(buffer);
    }

    ~SpatialPairBuffer()
    {
        Dispose(this);
    }
}