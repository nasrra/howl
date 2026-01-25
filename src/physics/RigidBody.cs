using System;
using Howl.ECS;
using Howl.Math;

namespace Howl.Physics;

public struct RigidBody
{
    public const float MinBodySize = float.Epsilon;
    public const float MaxBodySize = float.MaxValue;

    public const float MinDensity = float.Epsilon;
    public const float MaxDensity = 22.6f; // osmium density.

    public const float MinRestitution = 0f;
    public const float MaxRestitution = 1f;

    public Vector2 Position {get; private set;}
    public Vector2 LinearVelocity {get; private set;}
    public float Rotation {get; private set;}
    public float RotationalVelocity{get; private set;}
    public float Density {get; private set;}
    public float Mass {get; private set;}
    public float Restitution {get; private set;}
    public float Area {get; private set;}
    public bool IsStatic {get; private set;}

    /// <summary>
    /// Constructs a RigidBody.
    /// </summary>
    /// <param name="position"></param>
    /// <param name="rotation"></param>
    /// <param name="area"></param>
    /// <param name="density"></param>
    /// <param name="mass"></param>
    /// <param name="restitution"></param>
    /// <exception cref="InvalidOperationException"></exception>
    public RigidBody(
        Vector2 position,
        float rotation,
        float area,
        float density,
        float mass,
        float restitution
    )
    {
        Position = position;
        LinearVelocity = Vector2.Zero;
        Rotation = rotation;
        RotationalVelocity = 0;        
        Density = density;
        Area = area;

#if DEBUG
        if(density < MinDensity || density > MaxDensity)
        {
            throw new InvalidOperationException($"Cannot create a RigidBody with a density of '{density}'; Min density is '{MinDensity}' and Max density is '{MaxDensity}'");
        }

        if(area < MinBodySize || area > MaxBodySize)
        {
            throw new InvalidOperationException($"Cannot create a RigidBody with a area of '{area}'; Max body size is '{MaxBodySize}' and Min body size is '{MinBodySize}'");
        }

        if(restitution < MinRestitution || restitution > MaxRestitution)
        {
            throw new InvalidOperationException($"Cannot create a RigidBody with a restitution of '{restitution}'; Max restitution is '{MaxRestitution}' and Min restitution is '{MinRestitution}'");
        }
#endif
    }
}