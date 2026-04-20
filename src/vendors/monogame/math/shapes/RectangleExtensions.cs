using System.Runtime.CompilerServices;

namespace Howl.Vendors.MonoGame.Math.Shapes;

public static class RectangleExtensions
{
    /// <summary>
    /// Converts a MonoGame rectangle to a Howl rectangle.
    /// Note: MonoGame rectangles are int values whilst Howl rectangles are floating point values;
    /// the MonoGame int values are casted to floating-point values.
    /// </summary>
    /// <param name="rectangle">The MonoGame rectangle to convert.</param>
    /// <returns>The resultant Howl rectangle.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static Howl.Math.Shapes.Rectangle ToHowl(Microsoft.Xna.Framework.Rectangle rectangle)
    {   
        return new(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);
    }

    /// <summary>
    /// Converts a Howl rectangle to a MonoGame rectangle.
    /// Note: MonoGame rectangles are int values whilst Howl rectangles are floating point values;
    /// the Howl float values are casted to int which looses their decimal values.
    /// </summary>
    /// <param name="rectangle">The Howl rectangle to convert.</param>
    /// <returns>The resultant MonoGame rectangle.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static Microsoft.Xna.Framework.Rectangle ToMonoGame(Howl.Math.Shapes.Rectangle rectangle)
    {
        return new((int)rectangle.X, (int)rectangle.Y, (int)rectangle.Width, (int)rectangle.Height);
    }   
}