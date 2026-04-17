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
    /// Gets and sets the position.
    /// </summary>
    public Vector2 Position;

    /// <summary>
    /// Gets and sets the scale.
    /// </summary>
    public Vector2 Scale;

    /// <summary>
    /// The rotation in radians.
    /// </summary>
    private float rotation;

    /// <summary>
    /// Gets and sets the rotational value - in radians.
    /// </summary>
    public float Rotation {
        get => rotation;
        set
        {
            rotation = value;
            Sin = MathF.Sin(value);
            Cos = MathF.Cos(value);
        }
    }

    /// <summary>
    /// Gets and sets the sin value of the rotation.
    /// </summary>
    public float Sin;

    /// <summary>
    /// Gets and sets the cos value of the rotation.
    /// </summary>
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
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public void Translate(Vector2 traslation)
    {
        Position += traslation;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public void TranslateTo(Vector2 position)
    {
        Position = position;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public void SetScale(Vector2 scale)
    {
        Scale = scale;
    }
}