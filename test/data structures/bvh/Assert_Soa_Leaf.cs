using Howl.DataStructures.Bvh;
using Howl.Test.Ecs;
using Howl.Test.Math;
using Howl.Test.Math.Shapes;

public static class Assert_Soa_Leaf
{
    /// <summary>
    ///     Asserts the equality of a soa entry and expected values.
    /// </summary>
    /// <param name="minX">the expected x-component of the minimum vertex.</param>
    /// <param name="minY">the expected y-component of the minimum vertex.</param>
    /// <param name="maxX">the expected x-component of the maximum vertex.</param>
    /// <param name="maxY">the expected y-component of the maximum vertex.</param>
    /// <param name="centroidX">the expected x-component of the centroid vertex.</param>
    /// <param name="centroidY">the expected x-component of the centroid vertex.</param>
    /// <param name="branchIndex">the expected branch index.</param>
    /// <param name="entryIndex">the index of the entry in the soa to assert equality against.</param>
    /// <param name="soa">the soa instance containing the entry to assert.</param>
    public static void EntryEqual(float minX, float minY, float maxX, float maxY, float centroidX, float centroidY, int branchIndex, 
        int entryIndex, Soa_Leaf soa
    )
    {
        Assert_Soa_Aabb.EntryEqual(minX, minY, maxX, maxY, entryIndex, soa.Aabbs);
        Soa_Vector2Assert.EntryEqual(centroidX, centroidY, entryIndex, soa.Centroids);
        Assert.Equal(branchIndex, soa.BranchIndices[entryIndex]);
    }

    /// <summary>
    ///     Asserts the equality of array lengths in a soa instance.
    /// </summary>
    /// <param name="length">the expected length of the backing arrays.</param>
    /// <param name="soa">the soa instance.</param>
    public static void LengthEqual(int length, Soa_Leaf soa)
    {
        Assert_Soa_Aabb.LengthEqual(length, soa.Aabbs);
        Soa_Vector2Assert.LengthEqual(length, soa.Centroids);
        Assert.Equal(length, soa.BranchIndices.Length);
        Assert.Equal(length, soa.Length);
    }
}