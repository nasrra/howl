namespace Howl.Math;

public static class Util
{
    /// <summary>
    /// Converts degrees to radians.
    /// </summary>
    /// <param name="degrees">The angle in degrees.</param>
    /// <returns>The angle in radians.</returns>
    public static float ToRadians(float degrees)
    {
        return (float)((double)degrees * (System.Math.PI / 180.0));
    }
}