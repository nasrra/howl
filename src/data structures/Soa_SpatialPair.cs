using System.Runtime.CompilerServices;
using Howl.ECS;

namespace Howl.DataStructures;

public class Soa_SpatialPair
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
    public byte[] OwnerFlags;

    /// <summary>
    /// Gets and sets any user defined flags to distinguish an 'other' gen index.
    /// </summary>
    public byte[] OtherFlags;

    /// <summary>
    /// Gets and sets the count of valid entries in the backing arrays; starting from index 0.
    /// </summary>
    public int Count;

    /// <summary>
    /// Creates a new Structure-Of-Arrays SpatialPair instance.
    /// </summary>
    /// <param name="capacity">the capacity of the backing arrays.</param>
    public Soa_SpatialPair(int capacity)
    {
        OwnerGenIndices = new(capacity);
        OtherGenIndices = new(capacity);
        OwnerFlags = new byte[capacity];
        OtherFlags = new byte[capacity];
    }

    /// <summary>
    /// Clears all entries in a soa spatial pair by setting its count to zero.
    /// </summary>
    /// <param name="soa">the soa spatial pair to clear.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void Clear(Soa_SpatialPair soa)
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
    public static void AppendSpatialPair(Soa_SpatialPair soa, int ownerIndex, int ownerGeneration, int otherIndex, int otherGeneration, byte ownerFlags, byte otherFlags)
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
    public static void AppendSpatialPair(Soa_SpatialPair soa, GenIndex ownerIndex, GenIndex otherIndex, byte ownerFlags, byte otherFlags)
    {
        AppendSpatialPair(soa, ownerIndex.Index, ownerIndex.Generation, otherIndex.Index, otherIndex.Generation, ownerFlags, otherFlags);
    }
}