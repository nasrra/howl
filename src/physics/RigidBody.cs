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

    private Vector2 force;
    public readonly Vector2 Force => force;

    private Vector2 linearVelocity;
    public readonly Vector2 LinearVelocity => linearVelocity;

    private float rotationalVelocity;
    public readonly float RotationalVelocity => rotationalVelocity;

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
        if(density < MinDensity || density > MaxDensity)
        {
            throw new ArgumentException($"Cannot set density to '{density}'; Min density is '{MinDensity}' and Max density is '{MaxDensity}'");
        }
        this.density = density;
        mass = CalculateMass(area, this.density);
    }

    public void SetArea(float area)
    {
        if(area < MinBodySize || area > MaxBodySize)
        {
            throw new ArgumentException($"Cannot set area to '{area}'; Max body size is '{MaxBodySize}' and Min body size is '{MinBodySize}'");
        }
        this.area = area;
        mass = CalculateMass(this.area, density);
    }

    private float CalculateMass(float area, float density)
    {
        return area * density;
    }

    public void SetRestitution(float restitution)
    {
        if(restitution < MinRestitution || restitution > MaxRestitution)
        {
            throw new ArgumentException($"Cannot set restitution to '{restitution}'; Max restitution is '{MaxRestitution}' and Min restitution is '{MinRestitution}'");
        }
        this.restitution = restitution;
    }

    public void AddForce(Vector2 force)
    {
        this.force += force;    
    }

    public void ImpulseForce(Vector2 force)
    {
        linearVelocity += force;
    }

    public void ClearForces()
    {
        force = Vector2.Zero;
    }

    public void ClearLinearVelocity()
    {
        linearVelocity = Vector2.Zero;
    }

    public void ClearRotationalVelocity()
    {
        rotationalVelocity = 0;
    }

    public RigidBody(float restitution, float density, float area, bool isStatic)
    {
        SetRestitution(restitution);
        SetDensity(density);
        SetArea(area);
        IsStatic = isStatic;
    }
}