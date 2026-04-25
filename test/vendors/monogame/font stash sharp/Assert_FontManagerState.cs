using Howl.Vendors.MonoGame.FontStashSharp;

public static class Assert_FontManagerState
{
    /// <summary>
    ///     Asserts the length of backgin arrays within a state instance.
    /// </summary>
    /// <param name="length">the expected length.</param>
    /// <param name="state">the state instance.</param>
    public static void LengthEqual(int length, FontManagerState state)
    {
        Assert.Equal(length, state.Fonts.Length);
        Assert.Equal(length, state.MaxRegisteredCount);
    }

    /// <summary>
    ///     Asserts that a state instance has been disposed of.
    /// </summary>
    /// <param name="state"></param>
    public static void Disposed(FontManagerState state)
    {
        Assert.Null(state.Fonts);
        Assert.Null(state.FilePathToIndex);
        Assert.Equal(0, state.RegisteredCount);
        Assert.Equal(0, state.MaxRegisteredCount);
        Assert.True(state.Disposed);
    }
}