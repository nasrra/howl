using System.Runtime.CompilerServices;

namespace Howl.Vendors.MonoGame.Math;

public static class Vector2Extensions
{
    /// <summary>
    /// Translates a Howl vector to a Monogame vector.
    /// </summary>
    /// <param name="vector">The Monogame Vector2 to translate.</param>
    /// <returns>The resultant Howl Vector2.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static Howl.Math.Vector2 ToHowl(Microsoft.Xna.Framework.Vector2 vector) => new(vector.X, vector.Y);

    /// <summary>
    /// Translates a Howl vector to a Monogame vector.
    /// </summary>
    /// <param name="vector">The Howl vector to translate.</param>
    /// <returns>The resultant Monogame vector.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static Microsoft.Xna.Framework.Vector2 ToMonoGame(Howl.Math.Vector2 vector) => new(vector.X, vector.Y);
}