namespace Howl.Math.Shapes;

public struct AABB
{
    /// <summary>
    /// Gets and sets the minimum vector.
    /// </summary>
    public Vector2 Min;

    /// <summary>
    /// Gets and sets the maximum vector.
    /// </summary>
    public Vector2 Max;

    /// <summary>
    /// Constructs a Axis-Aligned-Bounding-Box.
    /// </summary>
    /// <param name="min">The minimum vector.</param>
    /// <param name="max">The maximum vector.</param>
    public AABB(Vector2 min, Vector2 max)
    {
        Min = min;
        Max = max;
    }

    /// <summary>
    /// Constructs a Axis-Aligned-Bounding-Box.
    /// </summary>
    /// <param name="minX">The x-value of the minimum vector.</param>
    /// <param name="minY">The y-value of the minimum vector.</param>
    /// <param name="maxX">The x-value of the maximum vector.</param>
    /// <param name="maxY">the y-value of the maximum vector.</param>
    public AABB(float minX, float minY, float maxX, float maxY)
    {
        Min = new (minX, minY);
        Max = new (maxX, maxY);
    }

    /// <summary>
    /// Checks whether two Axis-Aligned-Bounding-Boxes are intersecting.
    /// </summary>
    /// <param name="a">The first AABB.</param>
    /// <param name="b">The other AABB.</param>
    /// <returns>true, if there is an intersection; otherwise false.</returns>
    public static bool Intersect(in AABB a, in AABB b)
    {
        if(a.Max.X <= b.Min.X || b.Max.X <= a.Min.X)
        {
            return false;
        }
        if (a.Max.Y <= b.Min.Y || b.Max.Y <= a.Min.Y)
        {
            return false;
        }

        return true;
    }
}