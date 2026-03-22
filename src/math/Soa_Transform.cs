using System.Runtime.CompilerServices;
using static Howl.Math.Shapes.ShapeUtils;

namespace Howl.Math;

public class Soa_Transform
{
    /// <summary>
    /// Gets and sets the positional values.
    /// </summary>
    public Soa_Vector2 Position;

    /// <summary>
    /// Gets and sets the scaling values.
    /// </summary>
    public Soa_Vector2 Scale;

    /// <summary>
    /// Gets and sets the Sin value of a rotation.
    /// </summary>
    public float[] Sin;

    /// <summary>
    /// Gets and sets the Cos value of a rotation.
    /// </summary>
    public float[] Cos;

    /// <summary>
    /// Creates a new SoaTransform instance.
    /// </summary>
    /// <param name="length"></param>
    public Soa_Transform(int length)
    {
        Position    = new(length);
        Scale       = new(length);
        Sin         = new float[length];
        Cos         = new float[length];
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
        transform.Position.X = soa.Position.X[index];
        transform.Position.Y = soa.Position.Y[index];
        transform.Scale.X = soa.Scale.X[index];
        transform.Scale.Y = soa.Scale.Y[index];
        transform.Sin = soa.Sin[index];
        transform.Cos = soa.Cos[index];
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
        soa.Position.X[index] = transform.Position.X;
        soa.Position.Y[index] = transform.Position.Y;
        soa.Scale.X[index] = transform.Scale.X;
        soa.Scale.Y[index] = transform.Scale.Y;
        soa.Sin[index] = transform.Sin;
        soa.Cos[index] = transform.Cos;
    }
}