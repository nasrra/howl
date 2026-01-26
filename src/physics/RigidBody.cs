using System;
using System.Runtime.CompilerServices;
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

    private Vector2 position;
    public readonly Vector2 Position => position;

    private Vector2 linearVelocity;
    public readonly Vector2 LinearVelocity => linearVelocity;

    private Vector2 rotationalVelocity;
    public readonly Vector2 RotationalVelocity => rotationalVelocity;

    private float density;
    public readonly float Density => density;

    private float mass;
    public readonly float Mass => mass;

    private float restitution;
    public readonly float Restitution => restitution;

    private float area;
    public readonly float Area => area;

    public bool IsStatic;

    public void SetDensity(float density)
    {
#if DEBUG
        if(density < MinDensity || density > MaxDensity)
        {
            throw new InvalidOperationException($"Cannot create a RigidBody with a density of '{density}'; Min density is '{MinDensity}' and Max density is '{MaxDensity}'");
        }
#endif
        this.density = density;
        mass = CalculateMass(area, this.density);
    }

    public void SetArea(float area)
    {
#if DEBUG
        if(area < MinBodySize || area > MaxBodySize)
        {
            throw new InvalidOperationException($"Cannot create a RigidBody with a area of '{area}'; Max body size is '{MaxBodySize}' and Min body size is '{MinBodySize}'");
        }
#endif
        this.area = area;
        mass = CalculateMass(this.area, density);
    }

    private float CalculateMass(float area, float density)
    {
        return area * density;
    }

    public void SetRestitution(float restitution)
    {
#if DEBUG
        if(restitution < MinRestitution || restitution > MaxRestitution)
        {
            throw new InvalidOperationException($"Cannot create a RigidBody with a restitution of '{restitution}'; Max restitution is '{MaxRestitution}' and Min restitution is '{MinRestitution}'");
        }
#endif
        this.restitution = restitution;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public void Move(Vector2 movement)
    {
        position += movement;
    }

    public RigidBody(Vector2 position, float restitution, float density, float area, bool isStatic)
    {
        this.position = position;
        SetRestitution(restitution);
        SetDensity(density);
        SetArea(area);
        IsStatic = isStatic;
    }
}