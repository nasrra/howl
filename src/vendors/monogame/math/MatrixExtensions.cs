using System.Runtime.CompilerServices;

namespace Howl.Vendors.MonoGame.Math;

public static class MatrixExtensions
{
    
    /// <summary>
    /// Translates a MonoGame matrix to a Howl matrix.
    /// </summary>
    /// <param name="matrix">The MonoGame matrix.</param>
    /// <returns>the resultant Howl matrix.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static Howl.Math.Matrix ToHowl(this Microsoft.Xna.Framework.Matrix matrix)
    {
        return new(
            matrix.M11,
            matrix.M12,
            matrix.M13,
            matrix.M14,
            matrix.M21,
            matrix.M22,
            matrix.M23,
            matrix.M24,
            matrix.M31,
            matrix.M32,
            matrix.M33,
            matrix.M34,
            matrix.M41,
            matrix.M42,
            matrix.M43,
            matrix.M44
        );
    }

    /// <summary>
    /// Translates a Howl matrix to a MonoGame matrix.
    /// </summary>
    /// <param name="matrix">The Howl matrix.</param>
    /// <returns>The resultant MonoGame matrix.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static Microsoft.Xna.Framework.Matrix ToMonoGame(this Howl.Math.Matrix matrix)
    {
        return new(
            matrix.M11,
            matrix.M12,
            matrix.M13,
            matrix.M14,
            matrix.M21,
            matrix.M22,
            matrix.M23,
            matrix.M24,
            matrix.M31,
            matrix.M32,
            matrix.M33,
            matrix.M34,
            matrix.M41,
            matrix.M42,
            matrix.M43,
            matrix.M44
        );
    }

}