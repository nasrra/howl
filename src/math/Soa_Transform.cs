using System.Runtime.CompilerServices;
using Howl.Unmanaged.Collections;

namespace Howl.Math;

public struct Soa_Transform
{
    public Soa_Vector2 Positions;
    public Soa_Vector2 Scales;
    public Array<float> Sines;
    public Array<float> Cosines;
    public Array<float> RotationRadians;

    public bool IsInitialised;

    public static bool Initialise(ref Soa_Transform soa, ref Memory.Arena arena, int length)
    {
        if (soa.IsInitialised)
        {
            Debug.Assert(false,"Already Intialised.");
            return false;
        }
        soa.IsInitialised = true;
        Soa_Vector2.Initialise(ref soa.Positions, ref arena, length);
        Soa_Vector2.Initialise(ref soa.Scales, ref arena, length);
        Array.Initialise(ref soa.Sines, ref arena, length);
        Array.Initialise(ref soa.Cosines, ref arena, length);
        Array.Initialise(ref soa.RotationRadians, ref arena, length);
        return true;
    }

    /// <summary>
    /// Copies an soa transform entry into a transform struct.
    /// </summary>
    /// <param name="index">the index in the soa collection to copy.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void CopySoaToTransform(Soa_Transform source, ref Transform destination, int index)
    {
        destination.Position.X = source.Positions.X[index];
        destination.Position.Y = source.Positions.Y[index];
        destination.Scale.X = source.Scales.X[index];
        destination.Scale.Y = source.Scales.Y[index];
        destination.Sine = source.Sines[index];
        destination.Cosine = source.Cosines[index];
        destination.RotationRadians = source.RotationRadians[index];
    }

    /// <summary>
    ///     Copies an transform struct into a soa transform.
    /// </summary>
    /// <param name="index">the index in the soa collection to copy.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void CopyTransformToSoa(Soa_Transform destintation, ref Transform source, int index)
    {
        destintation.Positions.X[index] = source.Position.X;
        destintation.Positions.Y[index] = source.Position.Y;
        destintation.Scales.X[index] = source.Scale.X;
        destintation.Scales.Y[index] = source.Scale.Y;
        destintation.Sines[index] = source.Sine;
        destintation.Cosines[index] = source.Cosine;
        destintation.RotationRadians[index] = source.RotationRadians;
    }

    /// <summary>
    ///     Inserts a transform into a soa instance.
    /// </summary>
    /// <param name="index">the index in the soa backing arrays to insert into.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void Insert(Soa_Transform destination, Transform source, int index)
    {
        Insert(destination, index, source.Position.X, source.Position.Y, source.Scale.X, source.Scale.Y, source.Sine, 
            source.Cosine, source.RotationRadians
        );
    }

    /// <summary>
    ///     Inserts a transform into a soa instance.
    /// </summary>
    /// <param name="insertIndex">the index in the soa backing arrays to insert into.</param>
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
}