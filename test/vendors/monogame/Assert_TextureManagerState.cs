using Howl.Vendors.MonoGame;

public static class Assert_TextureManagerState
{
    /// <summary>
    ///     Asserts the length of the backing arrays of a state instance.
    /// </summary>
    /// <param name="length">the expected length.</param>
    /// <param name="state">the state instance.</param>
    public static void LengthEqual(int length, TextureManagerState state)
    {
        Assert.Equal(length, state.Textures.Length);
        Assert.Equal(length, state.MaxTextureCount);
    }

    /// <summary>
    ///     Asserts that a state instance has been disposed of.
    /// </summary>
    /// <param name="state">the state instance to assert against.</param>
    public static void Disposed(TextureManagerState state)
    {
        Assert.Null(state.Textures);
        Assert.Null(state.FilePathToIndex);
        Assert.Equal(0, state.RegisteredTexturesCount);
        Assert.Equal(0, state.MaxTextureCount);
        Assert.True(state.Disposed);
    }
}