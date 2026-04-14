using Howl.LevelManagement.Ldtk;

public static class Assert_Dto_LdtkProjectLevel
{
    /// <summary>
    ///     Asserts the equality of values in a LdtkProjectLevel instance.
    /// </summary>
    /// <param name="identifier">the expected identifier.</param>
    /// <param name="externalRelPath">the expected external relative path.</param>
    /// <param name="projLevel">the instance to assert against.</param>
    public static void Equal(string identifier, string externalRelPath, Dto_ProjectLevel projLevel)
    {
        Assert.Equal(identifier, projLevel.Identifier);
        Assert.Equal(externalRelPath, projLevel.ExternalRelPath);
    }
}