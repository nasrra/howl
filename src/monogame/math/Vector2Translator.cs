namespace Howl.Monogame.Math;

public class Vector2Translator
{
    /// <summary>
    /// Translates a Howl Vector2 to a Monogame Vector2.
    /// </summary>
    /// <param name="vector2">The Monogame Vector2 to translate.</param>
    /// <returns>The resultant Howl Vector2.</returns>
    public static Howl.Math.Vector2 ToHowlVector2(Microsoft.Xna.Framework.Vector2 vector2) => new(vector2.X, vector2.Y);
    
    /// <summary>
    /// Translates a Howl Vector2 to a Monogame Vector2.
    /// </summary>
    /// <param name="vector2">The Howl Vector to translate.</param>
    /// <returns>The resultant Monogame Vector2.</returns>
    public static Microsoft.Xna.Framework.Vector2 ToMonogameVector2(Howl.Math.Vector2 vector2) => new(vector2.X, vector2.Y);
}