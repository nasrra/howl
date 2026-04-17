namespace Howl.LevelManagement.Ldtk;

public static class Assert_Dto_Project
{
    /// <summary>
    ///     Asserts the equality of a dto ldtk project.
    /// </summary>
    /// <param name="externalLevels">the expected externalLevels flag.</param>
    /// <param name="levels">the expected project levels.</param>
    /// <param name="project">the project instance to assert against.</param>
    public static void Equal(bool externalLevels, Dto_ProjectLevel[] levels, Dto_Project project)
    {
        project.ExternalLevels = externalLevels;
        project.Levels = levels;
    }
}