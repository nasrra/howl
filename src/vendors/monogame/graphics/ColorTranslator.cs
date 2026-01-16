using System.Runtime.CompilerServices;

namespace Howl.Vendors.MonoGame.Graphics;

public static class ColorExtensions
{
    /// <summary>
    /// Converts a MonoGame color to a Howl color. 
    /// </summary>
    /// <param name="color">The MonoGame color to convert.</param>
    /// <returns>The resultant Howl color.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static Howl.Graphics.Color ToHowl(this Microsoft.Xna.Framework.Color color)
    {
        return new(color.R, color.G, color.B, color.A);
    }

    /// <summary>
    /// Converts a Howl color to a MonoGame color.
    /// </summary>
    /// <param name="color">The Howl color to convert.</param>
    /// <returns>The resultant Monogame color.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static Microsoft.Xna.Framework.Color ToMonoGame(this Howl.Graphics.Color color)
    {
        return new (color.R, color.G, color.B, color.A);
    }
}