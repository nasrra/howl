using System;
using System.Runtime.CompilerServices;
using Howl.Collections;
using static Howl.Math.Shapes.ShapeUtils;

namespace Howl.Math;

public class Soa_Transform
{
    public Soa_Vector2 Positions;
    public Soa_Vector2 Scales;
    public float[] Sines;
    public float[] Cosines;
    public float[] RotationRadians;

    /// <summary>
    /// Creates a new SoaTransform instance.
    /// </summary>
    /// <param name="length"></param>
    public Soa_Transform(int length)
    {
        Positions    = new(length);
        Scales       = new(length);
        Sines         = new float[length];
        Cosines        = new float[length];
        RotationRadians = new float[length];
    }

    /// <summary>
    /// Copies an soa transform entry into a transform struct.
    /// </summary>
    /// <param name="soa">the soa collection containing the data.</param>
    /// <param name="transform">the transform struct to mutate.</param>
    /// <param name="index">the index in the soa collection to copy.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void CopySoaToTransform(Soa_Transform soa, ref Transform transform, int index)
    {
        transform.Position.X = soa.Positions.X[index];
        transform.Position.Y = soa.Positions.Y[index];
        transform.Scale.X = soa.Scales.X[index];
        transform.Scale.Y = soa.Scales.Y[index];
        transform.Sine = soa.Sines[index];
        transform.Cosine = soa.Cosines[index];
        transform.RotationRadians = soa.RotationRadians[index];
    }

    /// <summary>
    ///     Copies an transform struct into a soa transform.
    /// </summary>
    /// <param name="soa">the soa collection containing the data.</param>
    /// <param name="transform">the transform struct to mutate.</param>
    /// <param name="index">the index in the soa collection to copy.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void CopyTransformToSoa(Soa_Transform soa, ref Transform transform, int index)
    {
        soa.Positions.X[index] = transform.Position.X;
        soa.Positions.Y[index] = transform.Position.Y;
        soa.Scales.X[index] = transform.Scale.X;
        soa.Scales.Y[index] = transform.Scale.Y;
        soa.Sines[index] = transform.Sine;
        soa.Cosines[index] = transform.Cosine;
        soa.RotationRadians[index] = transform.RotationRadians;
    }

    /// <summary>
    ///     Inserts a transform into a soa instance.
    /// </summary>
    /// <param name="soa">the soa instance to insert into..</param>
    /// <param name="transform">the transform to insert.</param>
    /// <param name="insertIndex">the index in the soa backing arrays to insert into.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void Insert(Soa_Transform soa, int insertIndex, Transform transform)
    {
        Insert(soa, insertIndex, transform.Position.X, transform.Position.Y, transform.Scale.X, transform.Scale.Y, transform.Sine, 
            transform.Cosine, transform.RotationRadians
        );
    }

    /// <summary>
    ///     Inserts a transform into a soa instance.
    /// </summary>
    /// <param name="soa">the soa instance to insert into.</param>
    /// <param name="insertIndex">the index in the soa backing arrays to insert into.</param>
    /// <param name="posX">the x-component of the position.</param>
    /// <param name="posY">the y-component of the position.</param>
    /// <param name="scaleX">the x-component of the scale.</param>
    /// <param name="scaleY">the y-component of the scale.</param>
    /// <param name="sin">the sin of rotation.</param>
    /// <param name="cos">the cos of roation.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void Insert(Soa_Transform soa, int insertIndex, float posX, float posY, float scaleX, float scaleY, float sin, float cos, 
        float rotationRadians
    )
    {
        soa.Positions.X[insertIndex] = posX;
        soa.Positions.Y[insertIndex] = posY;
        soa.Scales.X[insertIndex] = scaleX;
        soa.Scales.Y[insertIndex] = scaleY;
        soa.Sines[insertIndex] = sin;
        soa.Cosines[insertIndex] = cos;
        soa.RotationRadians[insertIndex] = rotationRadians;
    }

    public static void TransformRelative(Soa_Transform src, Soa_Transform dst, int srcReadIndex, int dstWriteIndex, 
        float worldPosX, float worldPosY, float worldScaleX, float worldScaleY, float worldSine, float worldCosine, float worldRotationRadians
    )
    {
        Transform.TransformRelative(src.Positions.X[srcReadIndex], src.Positions.Y[srcReadIndex], src.Scales.X[srcReadIndex], 
            src.Scales.Y[srcReadIndex], src.Sines[srcReadIndex], src.Cosines[srcReadIndex], src.RotationRadians[srcReadIndex], 
            worldPosX, worldPosY, worldScaleX, worldScaleY, worldSine, worldCosine, worldRotationRadians, 
            ref dst.Positions.X[dstWriteIndex], ref dst.Positions.Y[dstWriteIndex], ref dst.Scales.X[dstWriteIndex], 
            ref dst.Scales.Y[dstWriteIndex], ref dst.Sines[dstWriteIndex], ref dst.Cosines[dstWriteIndex], 
            ref dst.RotationRadians[dstWriteIndex] 
        );
    }

    /// <summary>
    ///     Enforces a <c>Nil</c> entry in all underlying arrays of a soa instance.
    /// </summary>
    /// <param name="soa">the soa instance.</param>
    public static void EnforceNil(Soa_Transform soa)
    {
        Nil.Enforce(soa.Cosines);
        Nil.Enforce(soa.Sines);
        Soa_Vector2.EnforceNil(soa.Positions);
        Soa_Vector2.EnforceNil(soa.Scales);
    }
}