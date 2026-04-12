using System.Runtime.CompilerServices;
using Howl.Collections;
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
    ///     Inserts a transform into a soa instance.
    /// </summary>
    /// <param name="soa">the soa instance to insert into..</param>
    /// <param name="transform">the transform to insert.</param>
    /// <param name="insertIndex">the index in the soa backing arrays to insert into.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void Insert(Soa_Transform soa, int insertIndex, Transform transform)
    {
        Insert(soa, insertIndex, transform.Position.X, transform.Position.Y, transform.Scale.X, transform.Scale.Y, transform.Sin, transform.Cos);
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
    public static void Insert(Soa_Transform soa, int insertIndex, float posX, float posY, float scaleX, float scaleY, float sin, float cos)
    {
        soa.Positions.X[insertIndex] = posX;
        soa.Positions.Y[insertIndex] = posY;
        soa.Scales.X[insertIndex] = scaleX;
        soa.Scales.Y[insertIndex] = scaleY;
        soa.Sins[insertIndex] = sin;
        soa.Coses[insertIndex] = cos;        
    }

    /// <summary>
    ///     Enforces a <c>Nil</c> entry in all underlying arrays of a soa instance.
    /// </summary>
    /// <param name="soa">the soa instance.</param>
    public static void EnforceNil(Soa_Transform soa)
    {
        Nil.Enforce(soa.Coses);
        Nil.Enforce(soa.Sins);
        Soa_Vector2.EnforceNil(soa.Positions);
        Soa_Vector2.EnforceNil(soa.Scales);
    }
}