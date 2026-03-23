using System.Runtime.CompilerServices;
using static Howl.Math.Shapes.ShapeUtils;

namespace Howl.Math;

public class Soa_Transform
{
    /// <summary>
    /// Gets and sets the positional values.
    /// </summary>
    public Soa_Vector2 Positions;

    /// <summary>
    /// Gets and sets the scaling values.
    /// </summary>
    public Soa_Vector2 Scales;

    /// <summary>
    /// Gets and sets the Sin value of a rotation.
    /// </summary>
    public float[] Sins;

    /// <summary>
    /// Gets and sets the Cos value of a rotation.
    /// </summary>
    public float[] Coses;

    /// <summary>
    /// Creates a new SoaTransform instance.
    /// </summary>
    /// <param name="length"></param>
    public Soa_Transform(int length)
    {
        Positions    = new(length);
        Scales       = new(length);
        Sins         = new float[length];
        Coses         = new float[length];
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
        transform.Sin = soa.Sins[index];
        transform.Cos = soa.Coses[index];
    }

    /// <summary>
    /// Copies an transform struct into a soa transform slot.
    /// </summary>
    /// <param name="soa">the soa collection to copy the transform to.</param>
    /// <param name="transform">the transform to copy.</param>
    /// <param name="index">the index in the soa collection to mutate.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void CopyTransformToSoa(Soa_Transform soa, ref Transform transform, int index)
    {
        soa.Positions.X[index] = transform.Position.X;
        soa.Positions.Y[index] = transform.Position.Y;
        soa.Scales.X[index] = transform.Scale.X;
        soa.Scales.Y[index] = transform.Scale.Y;
        soa.Sins[index] = transform.Sin;
        soa.Coses[index] = transform.Cos;
    }
}