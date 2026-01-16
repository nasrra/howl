using System.Runtime.CompilerServices;

namespace Howl.Vendors.MonoGame.Math;

public static class Vector2Extensions
{
    /// <summary>
    /// Translates a Howl Vector2 to a Monogame Vector2.
    /// </summary>
    /// <param name="vector2">The Monogame Vector2 to translate.</param>
    /// <returns>The resultant Howl Vector2.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static Howl.Math.Vector2 ToHowl(this Microsoft.Xna.Framework.Vector2 vector2) => new(vector2.X, vector2.Y);

    /// <summary>
    /// Translates a Howl Vector2 to a Monogame Vector2.
    /// </summary>
    /// <param name="vector2">The Howl Vector to translate.</param>
    /// <returns>The resultant Monogame Vector2.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static Microsoft.Xna.Framework.Vector2 ToMonogame(this Howl.Math.Vector2 vector2) => new(vector2.X, vector2.Y);
}