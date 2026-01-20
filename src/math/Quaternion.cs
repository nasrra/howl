using System;
using System.Runtime.CompilerServices;

namespace Howl.Math;

public struct Quaternion
{
    /// <summary>
    /// Gets and sets the x-coordinate.
    /// </summary>
    public float X;

    /// <summary>
    /// Gets and sets the y-coordinate.
    /// </summary>
    public float Y;

    /// <summary>
    /// Gets and sets the z-coordinate.
    /// </summary>
    public float Z;

    /// <summary>
    /// Gets and sets the rotational component.
    /// </summary>
    public float W;

    /// <summary>
    /// Constructs a quaternion with X, Y, Z and W from four values.
    /// </summary>
    /// <param name="x">x-coordinate in 3d-space.</param>
    /// <param name="y">y-coordinate in 3d-space.</param>
    /// <param name="z">z-coordinate in 3d-space.</param>
    /// <param name="w">rotational component.</param>
    public Quaternion(float x, float y, float z, float w)
    {
        X = x;
        Y = y;
        Z = z;
        W = w;
    }

    /// <summary>
    /// Constructs a quaternion from the specified axis and angle.
    /// </summary>
    /// <param name="axis">The axis of rotation.</param>
    /// <param name="angle">The angle in radians.</param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static Quaternion CreateFromAxisAngle(Vector3 axis, float angle)
    {
        float x = angle * 0.5f;
        float num = MathF.Sin(x);
        float w = MathF.Cos(x);
        return new Quaternion(axis.X * num, axis.Y * num, axis.Z * num, w);    
    }
}