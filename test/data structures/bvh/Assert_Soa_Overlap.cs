using Howl.DataStructures.Bvh;

namespace Howl.Test.DataStructures.Bvh;

public static class Assert_Soa_Overlap
{
    /// <summary>
    ///     Asserts the length of backing arrays in a soa instance.
    /// </summary>
    /// <param name="length">the expected length of the backing arrays.</param>
    /// <param name="soa">the soa instance.</param>
    public static void LengthEqual(int length, Soa_Overlap soa)
    {
        Assert.Equal(length, soa.OwnerLeafIndices.Length);
        Assert.Equal(length, soa.OtherLeafIndices.Length);
        Assert.Equal(length, soa.Length);
    }

    /// <summary>
    ///     Asserts the equality of a entry in a soa instance.
    /// </summary>
    /// <param name="ownerLeafIndex">the expected <c>owner</c> leaf index.</param>
    /// <param name="otherLeafIndex"the expected <c>other</c> lead index.</param>
    /// <param name="entryIndex">the index of the entry to assert against.</param>
    /// <param name="soa">the soa instance that contains the entry.</param>
    public static void EntryEqual(int ownerLeafIndex, int otherLeafIndex, int entryIndex, Soa_Overlap soa)
    {
        Assert.Equal(ownerLeafIndex, soa.OwnerLeafIndices[entryIndex]);
        Assert.Equal(otherLeafIndex, soa.OtherLeafIndices[entryIndex]);
    }
}