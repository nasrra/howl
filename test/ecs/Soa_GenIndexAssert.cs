using Howl.ECS;

namespace Howl.Test.ECS;

public static class Soa_GenIndexAssert
{   
    /// <summary>
    /// Asserts the equality of array lengths in a soa gen index instance.
    /// </summary>
    /// <param name="soa">the soa gen index instance to assert against.</param>
    /// <param name="length">the expected length of the backing arrays.</param>
    public static void LengthEqual(Soa_GenIndex soa, int length)
    {
        Assert.Equal(length, soa.Generations.Length);
        Assert.Equal(length, soa.Indices.Length);
    }

    public static void EntryEqual(Soa_GenIndex soa, int index, int generation)
    {
        
    }
}