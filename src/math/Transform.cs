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
    /// Gets the Sin value of the rotation.
    /// </summary>
    public float Sin {get; private set;}

    /// <summary>
    /// Gets the Cos value of the rotation.
    /// </summary>
    public float Cos {get; private set;} = 1;

    /// <summary>
    /// Constructs a Transform.
    /// </summary>
    /// <param name="position">The positional x and y-coordinate values.</param>
    /// <param name="scale">The horizontal (x) and vertical (y) scaling values.</param>
    /// <param name="rotation">The rotation - in radians.</param>
    public Transform(Vector2 position, Vector2 scale, float rotation)
    {
        Position = position;
        Scale = scale;
        this.rotation = rotation;
        Sin = MathF.Sin(rotation);
        Cos = MathF.Cos(rotation);
    }   

    /// <summary>
    /// Constructs a Transform.
    /// </summary>
    /// <param name="position">The positional x and y-coordinate values.</param>
    /// <param name="scale">The horizontal (x) and vertical (y) scaling values.</param>
    /// <param name="rotation">The rotation - in radians.</param>
    public Transform(Vector2Int position, Vector2 scale, float rotation)
    {
        Position = new Vector2(position.X, position.Y);
        Scale = scale;
        this.rotation = rotation;
        Sin = MathF.Sin(rotation);
        Cos = MathF.Cos(rotation);
    }   

    /// <summary>
    /// Constructs a Transform.
    /// </summary>
    /// <param name="position">The positional x and y-coordinate values.</param>
    /// <param name="scale">The horizontal (x) and vertical (y) scaling values.</param>
    /// <param name="rotation">The rotation - in radians.</param>
    public Transform(Vector2 position, float scale, float rotation)
    {
        Position = position;
        Scale = new(scale, scale);
        this.rotation = rotation;
        Sin = MathF.Sin(rotation);
        Cos = MathF.Cos(rotation);
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