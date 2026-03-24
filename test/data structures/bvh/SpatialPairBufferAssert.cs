using Howl.DataStructures.Bvh;

namespace Howl.Test.DataStructures.Bvh;

public static class SpatialPairBufferAssert
{
    /// <summary>
    /// Asserts the equality of values for a buffer entry and expected values.
    /// </summary>
    /// <param name="buffer">the buffer toi assert.</param>
    /// <param name="entryIndex">the index of the entry in the buffer to check equality against.</param>
    /// <param name="ownerIndex">the expected 'owner' index value.</param>
    /// <param name="ownerGeneration">the expected 'owner' generation value.</param>
    /// <param name="ownerFlags">the expected 'owner' flags value.</param>
    /// <param name="otherIndex">the expected 'other' index value.</param>
    /// <param name="otherGeneration">the expected 'other' generation value.</param>
    /// <param name="otherFlags">the expected 'other' flags value.</param>
    public static void EntryEquals(SpatialPairBuffer buffer, int entryIndex, int ownerIndex, int ownerGeneration, int ownerFlags, int otherIndex, 
        int otherGeneration, int otherFlags
    )
    {
        Assert.Equal(ownerIndex, buffer.OwnerGenIndices.Indices[entryIndex]);    
        Assert.Equal(ownerGeneration, buffer.OwnerGenIndices.Generations[entryIndex]);    
        Assert.Equal(ownerFlags, buffer.OwnerFlags[entryIndex]);    
        Assert.Equal(otherIndex, buffer.OtherGenIndices.Indices[entryIndex]);    
        Assert.Equal(otherGeneration, buffer.OtherGenIndices.Generations[entryIndex]);    
        Assert.Equal(otherFlags, buffer.OtherFlags[entryIndex]);    
    }
}