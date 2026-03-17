
using System;
using System.Runtime.CompilerServices;

namespace Howl.Math;

public class Soa_Vector2
{
    /// <summary>
    /// Gets and sets the x-coordinate values.
    /// </summary>
    public float[] X;

    /// <summary>
    /// Gets and sets the y-coordinate values.
    /// </summary>
    public float[] Y;

    /// <summary>
    /// Gets and sets the amount of valid entries; starting from index 0.
    /// </summary>
    public int Count;

    /// <summary>
    /// Creates a new SoaVector2 instance.
    /// </summary>
    /// <param name="length">the length of x and y-components to store.</param>
    public Soa_Vector2(int length)
    {
        X = new float[length];
        Y = new float[length];
    }

    /// <summary>
    /// Appends a vector2 to a soa vector2.
    /// </summary>
    /// <param name="soa">the soa vector2 to append to.</param>
    /// <param name="x">the x-component of the vector to append.</param>
    /// <param name="y">the y-component of the vector to append.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void AppendVector2(Soa_Vector2 soa, float x, float y)
    {
        int count = soa.Count;
        soa.X[count] = x;
        soa.Y[count] = y;
        soa.Count++;
    }

    /// <summary>
    /// Appends a vector2 to a soa vector2.
    /// </summary>
    /// <param name="soa">the soa vector2 to append to.</param>
    /// <param name="vector">the vector to append.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void AppendVector2(Soa_Vector2 soa, Vector2 vector)
    {
        AppendVector2(soa, vector.X, vector.Y);
    }
}