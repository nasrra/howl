using System;
using System.Runtime.CompilerServices;
using Howl.Ecs;

namespace Howl.DataStructures.Bvh;

public class Soa_SpatialPair : IDisposable
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
    /// The count of allocated entries from appending.
    /// </summary>
    public int AppendCount;

    /// <summary>
    /// The length of all the backing arrays of this instance.
    /// </summary>
    public int Length;

    /// <summary>
    /// Whether this instance has been disposed.
    /// </summary>
    public bool Disposed;

    /// <summary>
    /// Creates a new soa instance.
    /// </summary>
    /// <param name="length">the length of the backing arrays.</param>
    public Soa_SpatialPair(int length)
    {
        OwnerGenIndices = new(length);
        OtherGenIndices = new(length);
        OwnerFlags = new int[length];
        OtherFlags = new int[length];
        Length = length;
    }

    /// <summary>
    /// Appends an entry into a soa at the soa instance's <c>AppendCount</c> index.
    /// </summary>
    /// <param name="soa">the soa instance to append to.</param>
    /// <param name="ownerIndex">the index of the data associated with the spatial pair's 'owner'.</param>
    /// <param name="ownerGeneration">the generation of the data assocaited with the spatial pair's 'owner'.</param>
    /// <param name="ownerFlags">the user-defined flags of the spatial pair's 'owner'.</param>
    /// <param name="otherIndex">the index of the data associated with the spatial pair's 'other'.</param>
    /// <param name="otherGeneration">the generation of the data assocaited with the spatial pair's 'other'.</param>
    /// <param name="otherFlags">the user-defined flags of the spatial pair's 'other'.</param>
    public static void Append(Soa_SpatialPair soa, int ownerIndex, int ownerGeneration, int ownerFlags, int otherIndex, int otherGeneration,
        int otherFlags
    )
    {
        int count = soa.AppendCount;
        soa.OwnerGenIndices.Indices[count] = ownerIndex;
        soa.OwnerGenIndices.Generations[count] = ownerGeneration;
        soa.OwnerFlags[count] = ownerFlags;
        soa.OtherGenIndices.Indices[count] = otherIndex;
        soa.OtherGenIndices.Generations[count] = otherGeneration;
        soa.OtherFlags[count] = otherFlags;
        soa.AppendCount++;
    }

    /// <summary>
    /// Sets the <c>AppendCount</c> of a soa instance to zero.
    /// </summary>
    /// <param name="soa">the soa instance to clear.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void Clear(Soa_SpatialPair soa)
    {
        soa.AppendCount = 0;
    }




    /*******************
    
        Disposal.
    
    ********************/




    public void Dispose()
    {
        Dispose(this);
    }

    public static void Dispose(Soa_SpatialPair soa)
    {
        if(soa.Disposed)
            return;

        soa.Disposed = true;
        Soa_GenIndex.Dispose(soa.OwnerGenIndices);
        soa.OwnerGenIndices = null;
        Soa_GenIndex.Dispose(soa.OtherGenIndices);
        soa.OtherGenIndices = null;
        soa.OwnerFlags = null;
        soa.OtherFlags = null;
        soa.AppendCount = 0;
        soa.Length = 0;

        GC.SuppressFinalize(soa);
    }

    ~Soa_SpatialPair()
    {
        Dispose(this);
    }
}