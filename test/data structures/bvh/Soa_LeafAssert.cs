using Howl.DataStructures.Bvh;
using Howl.Test.ECS;
using Howl.Test.Math;
using Howl.Test.Math.Shapes;

public static class Soa_LeafAssert
{
    /// <summary>
    /// Asserts the equality of a soa entry and expected values.
    /// </summary>
    /// <param name="minX">the expected x-component of the minimum vertex.</param>
    /// <param name="minY">the expected y-component of the minimum vertex.</param>
    /// <param name="maxX">the expected x-component of the maximum vertex.</param>
    /// <param name="maxY">the expected y-component of the maximum vertex.</param>
    /// <param name="centroidX">the expected x-component of the centroid vertex.</param>
    /// <param name="centroidY">the expected x-component of the centroid vertex.</param>
    /// <param name="index">the expected index value.</param>
    /// <param name="generation">the expected generation value.</param>
    /// <param name="flags">the expected flags value.</param>
    /// <param name="branchIndex">the expected branch index.</param>
    /// <param name="entryIndex">the index of the entry in the soa to assert equality against.</param>
    /// <param name="soa">the soa instance containing the entry to assert.</param>
    public static void EntryEqual(float minX, float minY, float maxX, float maxY, float centroidX, float centroidY, int index, int generation, 
        int flags, int branchIndex, int entryIndex, Soa_Leaf soa
    )
    {
        Soa_AabbAssert.EntryEqual(minX, minY, maxX, maxY, entryIndex, soa.Aabbs);
        Soa_GenIndexAssert.EntryEqual(index, generation, entryIndex, soa.GenIndices);
        Soa_Vector2Assert.EntryEqual(centroidX, centroidY, entryIndex, soa.Centroids);
        Assert.Equal(flags, soa.Flags[entryIndex]);
        Assert.Equal(branchIndex, soa.BranchIndices[entryIndex]);
    }

    /// <summary>
    /// Asserts the equality of array lengths in a soa instance.
    /// </summary>
    /// <param name="length">the expected length of the backing arrays.</param>
    /// <param name="soa">the soa instance.</param>
    public static void LengthEqual(int length, Soa_Leaf soa)
    {
        Soa_AabbAssert.LengthEqual(length, soa.Aabbs);
        Soa_GenIndexAssert.LengthEqual(length, soa.GenIndices);
        Soa_Vector2Assert.LengthEqual(length, soa.Centroids);
        Assert.Equal(length, soa.Flags.Length);
        Assert.Equal(length, soa.BranchIndices.Length);
        Assert.Equal(length, soa.Length);
    }
}