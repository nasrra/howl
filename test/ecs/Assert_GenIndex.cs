using Howl.ECS;

namespace Howl.Test.ECS;

public static class Assert_GenIndex
{
    /// <summary>
    /// Asserts the equality of two gen indices.
    /// </summary>
    /// <param name="expected">the expected value.</param>
    /// <param name="actual">the the actual value.</param>
    public static void Equal(GenIndex expected, GenIndex actual)
    {
        Assert.Equal(expected.Index, actual.Index);
        Assert.Equal(expected.Generation, actual.Generation);
    }
}