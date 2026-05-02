namespace Howl.Physics;

public static class Constants
{
    /// <summary>
    ///     The maximum amount of colliders 
    /// </summary>
    /// <remarks>
    ///     Remarks: This is because colliders are stored in a one dimensional array, meaning anything higher than 46340 * 46340 will cause an integer overflow.
    /// </remarks>
    public const int MaxColliders = 46340;
}