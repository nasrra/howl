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
    public float RotationRadians;

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
    public float Sine;

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
    public float Cosine;

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
        RotationRadians = rotation;
        Sine = sin;
        Cosine = cos;

        if(float.IsNaN(Cosine) || float.IsNaN(Sine))
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
        transform.RotationRadians += radians;
        transform.Sine = MathF.Sin(transform.RotationRadians);
        transform.Cosine = MathF.Cos(transform.RotationRadians);
    }

    /// <summary>
    ///     Offsets a transform by another.
    /// </summary>
    /// <param name="local"></param>
    /// <param name="world"></param>
    /// <returns>the newly offseted transform.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static Transform TransformRelative(Transform local, Transform world)
    {
        return TransformRelative(local, world.Position.X, world.Position.Y, world.Scale.X, world.Scale.Y, world.Sine, world.Cosine, world.RotationRadians);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static Transform TransformRelative(Transform local, float worldPosX, float worldPosY, float worldScaleX, 
        float worldScaleY, float worldSine, float worldCosine, float worldRotationRadians
    )
    {
        Transform transform = default;
        
        TransformRelative(local.Position.X, local.Position.Y, local.Scale.X, local.Scale.Y, local.Sine, local.Cosine, local.RotationRadians, 
            worldPosX, worldPosY, worldScaleX, worldScaleY, worldSine, worldCosine, worldRotationRadians, ref transform.Position.X, 
            ref transform.Position.Y, ref transform.Scale.X, ref transform.Scale.Y, ref transform.Sine, ref transform.Cosine, 
            ref transform.RotationRadians 
        );

        return transform;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void TransformRelative(float localPosX, float localPosY, float localScaleX, float localScaleY, float localSine, float localCosine,
        float localRotationRadians, float worldPosX, float worldPosY, float worldScaleX, float worldScaleY, float worldSine, float worldCosine, 
        float worldRotationRadians, ref float posXOutput, ref float posYOutput, ref float scaleXOutput, ref float scaleYOutput, ref float sineOutput,
        ref float cosineOutput, ref float rotationRadiansOutput 
    )
    {
        // scale the local offset relative to the world.
        float scaledX = localPosX * worldScaleX;
        float scaledY = localPosY * worldScaleY;
        
        // rotate the local scaled offset around the parents origin.
        //      Standard 2D rotation matrix formula:
        //          x' = x * cos - y * sin
        //          y' = x * sin + y * cos
        float rotatedX = (scaledX * worldCosine) - (scaledY * worldSine);
        float rotatedY = (scaledX * worldSine) + (scaledY * worldCosine);

        // translate the rotated offset to the world position.
        posXOutput = rotatedX + worldPosX;
        posYOutput = rotatedY + worldPosY;

        // combine the scale properties
        scaleXOutput = localScaleX * worldScaleX;
        scaleYOutput = localScaleY * worldScaleY;

        // combine the rotation properties.
        //      Use the trigonometrix identity formulas for combining angles:
        //          sin(a + b) = sin(a)cos(b) + cos(a)sin(b)
        //          cos(a + b) = cos(a)cos(b) - sin(a)sin(b)
        sineOutput = (localSine * worldCosine) + (localCosine * worldSine);
        cosineOutput = (localCosine * worldCosine) + (localSine * worldSine);
        rotationRadiansOutput = localRotationRadians + worldRotationRadians;
    }
}
