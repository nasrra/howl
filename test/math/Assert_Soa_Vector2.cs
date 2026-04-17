using Howl.Math;

namespace Howl.Test.Math;

public static class Assert_Soa_Vector2
{
    /// <summary>
    ///     Asserts the equality of an element within the soa instance.
    /// </summary>
    /// <param name="vector">the expected vector.</param>
    /// <param name="entryIndex">the index of the element in the soa instance.</param>
    /// <param name="soa">the soa instance.</param>
    public static void EntryEqual(Vector2 vector, int entryIndex, Soa_Vector2 soa)
    {
        EntryEqual(vector.X, vector.Y, entryIndex, soa);        
    }

    /// <summary>
    ///     Asserts the equality of an element within the soa instance.
    /// </summary>
    /// <param name="x">the expected x value.</param>
    /// <param name="y">the expected y value.</param>
    /// <param name="entryIndex">the index of the element in the soa instance.</param>
    /// <param name="soa">the soa instance.</param>
    public static void EntryEqual(float x, float y, int entryIndex, Soa_Vector2 soa)
    {
        Assert.Equal(x, soa.X[entryIndex]);
        Assert.Equal(y, soa.Y[entryIndex]);
    }
}