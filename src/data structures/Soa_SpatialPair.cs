using System.Runtime.CompilerServices;
using Howl.ECS;

namespace Howl.DataStructures;

public class SpatialPairBuffer
{
    /// <summary>
    /// Gets and sets the 'owner' gen indices.
    /// </summary>
    public Soa_GenIndex OwnerGenIndices;

    /// <summary>
    /// Gets and sets the 'other' gen indices.
    /// </summary>
    public Soa_GenIndex OtherGenIndices;

    /// <summary>
    /// Gets and sets any user-defined flags to distinguish an 'owner' gen index.
    /// </summary>
    public int[] OwnerFlags;

    /// <summary>
    /// Gets and sets any user defined flags to distinguish an 'other' gen index.
    /// </summary>
    public int[] OtherFlags;

    /// <summary>
    /// Gets and sets the count of valid entries in the backing arrays; starting from index 0.
    /// </summary>
    public int Count;

    /// <summary>
    /// Creates a new spatial pair buffer instance.
    /// </summary>
    /// <param name="capacity">the capacity of the backing arrays.</param>
    public SpatialPairBuffer(int capacity)
    {
        OwnerGenIndices = new(capacity);
        OtherGenIndices = new(capacity);
        OwnerFlags = new int[capacity];
        OtherFlags = new int[capacity];
    }

    /// <summary>
    /// Clears all entries in a spatial pair buffer by setting its count to zero.
    /// </summary>
    /// <param name="soa">the soa spatial pair to clear.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void Clear(SpatialPairBuffer soa)
    {
        soa.Count = 0;
    }

    /// <summary>
    /// Appends a spatial pair entry to an Soa Spatial Pair.
    /// </summary>
    /// <param name="soa">the Soa Spatial Pair.</param>
    /// <param name="ownerIndex">the index of the 'owner'.</param>
    /// <param name="ownerGeneration">the generation of the 'owner'.</param>
    /// <param name="otherIndex">the index of the 'other'.</param>
    /// <param name="otherGeneration">the generation of the 'other'.</param>
    /// <param name="ownerFlags">the user-defined flags of the 'owner'.</param>
    /// <param name="otherFlags">the user-defined flags of the 'other'.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void Append(SpatialPairBuffer soa, int ownerIndex, int ownerGeneration, int otherIndex, int otherGeneration, int ownerFlags, int otherFlags)
    {
        int count = soa.Count;
        
        soa.OwnerGenIndices.Indices[count] = ownerIndex;
        soa.OwnerGenIndices.Generations[count] = ownerGeneration;
        soa.OtherGenIndices.Indices[count] = otherIndex;
        soa.OtherGenIndices.Generations[count] = otherGeneration;
        soa.OwnerFlags[count] = ownerFlags;
        soa.OtherFlags[count] = otherFlags;
        
        soa.Count++;
    }

    /// <summary>
    /// Appends a spatial pair entry to an soa spatial pair.
    /// </summary>
    /// <param name="soa">the soa spatial pair</param>
    /// <param name="ownerIndex">the gen index of the owner.</param>
    /// <param name="otherIndex">the gen index of the other.</param>
    /// <param name="ownerFlags">the user-defined flags of the 'owner'.</param>
    /// <param name="otherFlags">the user-defined flags of the 'other'.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void Append(SpatialPairBuffer soa, GenIndex ownerIndex, GenIndex otherIndex, byte ownerFlags, byte otherFlags)
    {
        Append(soa, ownerIndex.Index, ownerIndex.Generation, otherIndex.Index, otherIndex.Generation, ownerFlags, otherFlags);
    }
}