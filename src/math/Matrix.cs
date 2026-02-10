using System;
using System.Runtime.CompilerServices;

namespace Howl.Math;

public struct Matrix
{
    /// <summary>
    /// Gets and sets the first row, first column value.
    /// </summary>
    public float M11;

    /// <summary>
    /// Gets and sets the first row, second column value.
    /// </summary>
    public float M12;

    /// <summary>
    /// Gets and sets the first row, third column value.
    /// </summary>
    public float M13;

    /// <summary>
    /// Gets and sets the first row, fourth column value.
    /// </summary>
    public float M14;

    /// <summary>
    /// Gets and sets the second row, first column value.
    /// </summary>
    public float M21;

    /// <summary>
    /// Gets and sets the second row, second column value.
    /// </summary>
    public float M22;

    /// <summary>
    /// Gets and sets the second row, third column value.
    /// </summary>
    public float M23;

    /// <summary>
    /// Gets and sets the second row, fourth column value.
    /// </summary>
    public float M24;

    /// <summary>
    /// Gets and sets the third row, first column value.
    /// </summary>
    public float M31;

    /// <summary>
    /// Gets and sets the third row, second column value.
    /// </summary>
    public float M32;

    /// <summary>
    /// Gets and sets the third row, third column value.
    /// </summary>
    public float M33;

    /// <summary>
    /// Gets and sets the third row, fourth column value.
    /// </summary>
    public float M34;

    /// <summary>
    /// Gets and sets the fourth row, first column value.
    /// </summary>
    public float M41;

    /// <summary>
    /// Gets and sets the fourth row, second column value.
    /// </summary>
    public float M42;

    /// <summary>
    /// Gets and sets the fourth row, third column value.
    /// </summary>
    public float M43;

    /// <summary>
    /// Gets and sets the fourth row, fourth column value.
    /// </summary>
    public float M44;

    public readonly static Matrix Identity = new Matrix(1f, 0f, 0f, 0f, 0f, 1f, 0f, 0f, 0f, 0f, 1f, 0f, 0f, 0f, 0f, 1f);

    /// <summary>
    /// Constructs a matrix.
    /// </summary>
    /// <param name="m11">first row, first column value.</param>
    /// <param name="m12">first row, second column value.</param>
    /// <param name="m13">first row, third column value.</param>
    /// <param name="m14">first row, fourth column value.</param>
    /// <param name="m21">second row, first column value.</param>
    /// <param name="m22">second row, second column value.</param>
    /// <param name="m23">second row, third column value.</param>
    /// <param name="m24">second row, fourth column value.</param>
    /// <param name="m31">third row, first column value.</param>
    /// <param name="m32">third row, second column value.</param>
    /// <param name="m33">third row, third column value.</param>
    /// <param name="m34">third row, fourth column value.</param>
    /// <param name="m41">fourth row, first column value.</param>
    /// <param name="m42">fourth row, second column value.</param>
    /// <param name="m43">fourth row, third column value.</param>
    /// <param name="m44">fourth row, fourth column value.</param>
    public Matrix(
        float m11, 
        float m12,
        float m13,
        float m14,
        float m21,
        float m22,
        float m23,
        float m24,
        float m31,
        float m32,
        float m33,
        float m34,
        float m41,
        float m42,
        float m43,
        float m44
    )
    {
        M11 = m11;
        M12 = m12;
        M13 = m13;
        M14 = m14;
        M21 = m21;
        M22 = m22;
        M23 = m23;
        M24 = m24;
        M31 = m31;
        M32 = m32;
        M33 = m33;
        M34 = m34;
        M41 = m41;
        M42 = m42;
        M43 = m43;
        M44 = m44;
    }
    
    /// <summary>
    /// Creates a new rotation matrix around Y axis.
    /// </summary>
    /// <param name="radians">Angle in radians.</param>
    /// <returns>The rotation matrix around the y-axis as an output parameter.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static Matrix CreateRotationY(float radians)
    {
        Matrix result = Identity;
        float num = MathF.Cos(radians);
        float num2 = MathF.Sin(radians);
        result.M11 = num;
        result.M13 = 0f - num2;
        result.M31 = num2;
        result.M33 = num;
        return result;
    }
    
    /// <summary>
    /// Contstructs a new <see cref="Matrix"/> for customized orthographics view.
    /// </summary>
    /// <param name="left">the lower x-value at the near plane.</param>
    /// <param name="right">the upper x-value at the near pane.</param>
    /// <param name="bottom">the lower y-value at the near plane.</param>
    /// <param name="top">the upper y-value at the near plane. </param>
    /// <param name="zNearPlane">the depth of the near plane.</param>
    /// <param name="zFarPlane">the depth of the far plane.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static Matrix CreateOrthographicOffCenter(float left, float right, float bottom, float top, float zNearPlane, float zFarPlane)
    {
        Matrix matrix;
        matrix.M11 = (float)(2.0 / ((double)right - (double)left));
        matrix.M12 = 0f;
        matrix.M13 = 0f;
        matrix.M14 = 0f;
        matrix.M21 = 0f;
        matrix.M22 = (float)(2.0 / ((double)top - (double)bottom));
        matrix.M23 = 0f;
        matrix.M24 = 0f;
        matrix.M31 = 0f;
        matrix.M32 = 0f;
        matrix.M33 = (float)(1.0 / ((double)zNearPlane - (double)zFarPlane));
        matrix.M34 = 0f;
        matrix.M41 = (float)(((double)left + (double)right) / ((double)left - (double)right));
        matrix.M42 = (float)(((double)top + (double)bottom) / ((double)bottom - (double)top));
        matrix.M43 = (float)((double)zNearPlane / ((double)zNearPlane - (double)zFarPlane));
        matrix.M44 = 1f;
        return matrix;
    }
}