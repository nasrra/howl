using Howl.Text;

namespace Howl.Test.Text;

public static class Assert_StringRegistryState
{
    /// <summary>
    ///     Asserts the equality of backing array lengths of a state instance.
    /// </summary>
    /// <param name="length">the expected length.</param>
    /// <param name="state">the state instance to assert.</param>
    public static void LengthEqual(int length, StringRegistryState state)
    {
        Assert.Equal(length, state.StringAllocators.Length);
        Assert.Equal(length, state.DenseIndices.Length);
        Assert.Equal(length, state.Active.Length);
    }

    /// <summary>
    ///     Asserts that a state instance has been disposed of.
    /// </summary>
    /// <param name="state">the state instance to assert.</param>
    public static void Disposed(StringRegistryState state)
    {
        Assert.Null(state.StringAllocators);
        Assert.Null(state.DenseIndices);
        Assert.Null(state.NilString);
        Assert.Null(state.Active);
        Assert.True(state.Disposed);
    }
}