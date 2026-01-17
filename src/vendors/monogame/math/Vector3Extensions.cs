using System.Runtime.CompilerServices;

namespace Howl.Vendors.MonoGame.Math;

public static class Vector3Extensions
{
    /// <summary>
    /// Translates a Howl vector to a Monogame vector.
    /// </summary>
    /// <param name="vector2">The Monogame Vector2 to translate.</param>
    /// <returns>The resultant Howl Vector2.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static Howl.Math.Vector3 ToHowl(this Microsoft.Xna.Framework.Vector3 vector) => new(vector.X, vector.Y, vector.Z);

    /// <summary>
    /// Translates a Howl vector to a Monogame vector.
    /// </summary>
    /// <param name="vector2">The Howl vector to translate.</param>
    /// <returns>The resultant Monogame vector.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static Microsoft.Xna.Framework.Vector3 ToMonogame(this Howl.Math.Vector3 vector) => new(vector.X, vector.Y, vector.Z);
}