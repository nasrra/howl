using Howl.DataStructures.Bvh;
using Howl.ECS;
using Howl.Math;
using Howl.Math.Shapes;
using Howl.Test.ECS;
using Howl.Test.Math;
using Howl.Test.Math.Shapes;

public static class LeafBufferAssert
{
    /// <summary>
    /// Asserts the equality of a buffer entry and expected values.
    /// </summary>
    /// <param name="minX">the expected minimum x value.</param>
    /// <param name="minY">the expected minimum y value.</param>
    /// <param name="maxX">the expected maximum x value.</param>
    /// <param name="maxY">the expected maximum y value.</param>
    /// <param name="centroidX">the expected x-component of the centroid.</param>
    /// <param name="centroidY">the expected y-component of the centroid.</param>
    /// <param name="index">the expected index value.</param>
    /// <param name="generation">the expected generation value.</param>
    /// <param name="flags">the expected flags value.</param>
    /// <param name="entryIndex">the index of the entry in the buffer to assert equality against.</param>
    /// <param name="buffer">the buffer containing the entry to assert.</param>
    public static void EntryEqual(float minX, float minY, float maxX, float maxY, float centroidX, float centroidY, int index, int generation, 
        int flags, int entryIndex, LeafBuffer buffer
    )
    {
        Soa_AabbAssert.EntryEqual(minX, minY, maxX, maxY, entryIndex, buffer.Aabbs);
        Soa_GenIndexAssert.EntryEqual(index, generation, entryIndex, buffer.GenIndices);
        Assert.Equal(flags, buffer.Flags[entryIndex]);
    }

    /// <summary>
    /// Asserts the equality of array lengths in a buffer instance.
    /// </summary>
    /// <param name="length">the expected length of the backing arrays.</param>
    /// <param name="buffer">the buffer instance.</param>
    public static void LengthEqual(int length, LeafBuffer buffer)
    {
        Soa_AabbAssert.LengthEqual(length, buffer.Aabbs);
        Soa_GenIndexAssert.LengthEqual(length, buffer.GenIndices);
        Soa_Vector2Assert.LengthEqual(length, buffer.Centroids);
        Assert.Equal(length, buffer.Flags.Length);
        Assert.Equal(length, buffer.SortingArray.Length);
        Assert.Equal(length, buffer.Length);
    }
}