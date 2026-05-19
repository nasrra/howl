using System;
using System.Runtime.CompilerServices;

namespace Howl.Math;

/// <summary>
/// NOTE:
/// Transform must not be default-initialised, as Cos and Sin would not be correctly set.
/// You must use a constructor.
/// </summary>
public struct Transform
{
    public static Transform Identity = new Transform(new Vector2(0,0), new Vector2(1,1), 0);

    /// <summary>
    ///     the position.
    /// </summary>
    /// <remarks>
    ///     <para>Remarks:</para> 
    ///     <para>this should only be directly modified in special cases.</para>
    ///     <para>instead use the functions:</para>
    ///     <list type = "bullet">
    ///         <item><see cref="Translate(ref Transform, Vector2)"/></item>
    ///         <item><see cref="Warp(ref Transform, Vector2)"/></item>
    ///     </list>
    /// </remarks>
    public Vector2 Position;

    /// <summary>
    ///     The scale.
    /// </summary>
    /// <remarks>
    ///     <para>Remarks:</para>
    ///     <para>this should only be directly modified in special cases.</para>
    ///     <para>instead use the functions:</para>
    ///     <list type = "bullet">
    ///         <item><see cref="SetScale(ref Transform, Vector2)"/></item>
    ///     </list>
    /// </remarks>
    public Vector2 Scale;

    /// <summary>
    ///     the rotational value - in radians.
    /// </summary>
    /// <remarks>
    ///     <para>Remarks:</para>
    ///     <para>this should only be directly modified in special cases.</para>
    ///     <para>instead use the functions:</para>
    ///     <list type = "bullet">
    ///         <item><see cref="Rotate(ref Transform, float)"/></item>
    ///     </list>
    /// </remarks>
    public float Rotation;

    /// <summary>
    ///     the sin value of the rotation.
    /// </summary>
    /// <remarks>
    ///     <para>Remarks:</para>
    ///     <para>this should only be directly modified in special cases.</para>
    ///     <para>instead use the functions:</para>
    ///     <list type = "bullet">
    ///         <item><see cref="Rotate(ref Transform, float)"/></item>
    ///     </list>
    /// </remarks>
    public float Sin;

    /// <summary>
    ///     the cos value of the rotation.
    /// </summary>
    /// <remarks>
    ///     <para>Remarks:</para>
    ///     <para>this should only be directly modified in special cases.</para>
    ///     <para>instead use the functions:</para>
    ///     <list type = "bullet">
    ///         <item><see cref="Rotate(ref Transform, float)"/></item>
    ///     </list>
    /// </remarks>
    public float Cos;

    /// <summary>
    /// Constructs a Transform.
    /// </summary>
    /// <param name="position">The positional x and y-coordinate values.</param>
    /// <param name="scale">The horizontal (x) and vertical (y) scaling values.</param>
    /// <param name="rotation">The rotation - in radians.</param>
    public Transform(Vector2 position, Vector2 scale, float rotation)
    : this(position.X, position.Y, scale.X, scale.Y, rotation, MathF.Sin(rotation), MathF.Cos(rotation)){}

    /// <summary>
    /// Constructs a Transform.
    /// </summary>
    /// <param name="position">The positional x and y-coordinate values.</param>
    /// <param name="scale">The horizontal (x) and vertical (y) scaling values.</param>
    /// <param name="rotation">The rotation - in radians.</param>
    public Transform(Vector2Int position, Vector2 scale, float rotation)
    : this(position.X, position.Y, scale.X, scale.Y, rotation, MathF.Sin(rotation), MathF.Cos(rotation)){}   

    /// <summary>
    /// Constructs a Transform.
    /// </summary>
    /// <param name="position">The positional x and y-coordinate values.</param>
    /// <param name="scale">The horizontal (x) and vertical (y) scaling values.</param>
    /// <param name="rotation">The rotation - in radians.</param>
    public Transform(Vector2 position, float scale, float rotation)
    : this(position.X, position.Y, scale, scale, rotation, MathF.Sin(rotation), MathF.Cos(rotation)){}
    
    /// <summary>
    /// Constructs a Transform.
    /// </summary>
    /// <param name="positionX">the x-component of the positional vector.</param>
    /// <param name="positionY">the y-component of the positional vector.</param>
    /// <param name="scaleX">the x-component of the scaling vector.</param>
    /// <param name="scaleY">the y-component of the scaling vector.</param>
    /// <param name="rotation">the rotational value - in radians.</param>
    /// <param name="sin">the sin of the rotation.</param>
    /// <param name="cos">the cos of the rotation.</param>
    public Transform(float positionX, float positionY, float scaleX, float scaleY, float rotation, float sin, float cos)
    {
        Position.X = positionX;
        Position.Y = positionY;
        Scale.X = scaleX;
        Scale.Y = scaleY;
        Rotation = rotation;
        Sin = sin;
        Cos = cos;

        if(float.IsNaN(Cos) || float.IsNaN(Sin))
        {
            System.Diagnostics.Debug.Assert(false);
        }
    }

    /// <summary>
    ///     Translates a transform's position.
    /// </summary>
    /// <param name="transform">the transform to translate.</param>
    /// <param name="traslation">the displacement to add to the position.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void Translate(ref Transform transform, Vector2 traslation)
    {
        transform.Position += traslation;
    }

    /// <summary>
    ///     Sets a transform's position.
    /// </summary>
    /// <param name="transform">the transform to set.</param>
    /// <param name="position">the position to warp to.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void Warp(ref Transform transform, Vector2 position)
    {
        transform.Position = position;
    }

    /// <summary>
    ///     Sets the scale of a transform.
    /// </summary>
    /// <param name="transform">the transform to set.</param>
    /// <param name="scale">the scale to set to.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void SetScale(ref Transform transform, Vector2 scale)
    {
        transform.Scale = scale;
    }

    /// <summary>
    ///     Rotates a transform.
    /// </summary>
    /// <param name="transform">the transform to rotate.</param>
    /// <param name="radians">the amount in radians to rotate by.</param>
    public static void Rotate(ref Transform transform, float radians)
    {
        transform.Rotation += radians;
        transform.Sin = MathF.Sin(transform.Rotation);
        transform.Cos = MathF.Cos(transform.Rotation);
    }

    /// <summary>
    ///     Offsets a transform's position and rotation by another's.
    /// </summary>
    /// <param name="source"></param>
    /// <param name="offset"></param>
    /// <returns>the newly offseted transform.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static Transform Combine(Transform source, Transform offset)
    {
        source.Position += offset.Position;
        source.Scale += offset.Scale;
        Rotate(ref source, offset.Rotation);
        return source;
    }
}