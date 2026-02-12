using System;
using System.Runtime.CompilerServices;
using Howl.Math;
using Howl.Math.Shapes;

namespace Howl.Physics;

public struct RigidBody
{
    private const float RectangleInertiaConst = 0.0833333333333f;
    private const float CircleInertiaConst = 0.5f;
    public const float MinBodySize = float.Epsilon;
    public const float MaxBodySize = float.MaxValue;

    public const float MinDensity = float.Epsilon;
    public const float MaxDensity = 22.6f; // osmium density.

    public const float MinRestitution = 0f;
    public const float MaxRestitution = 1f;

    /// <summary>
    /// Gets and sets the force to be applied to this body on the next physics system tick.
    /// </summary>
    private Vector2 force;

    /// <summary>
    /// Gets the force to be applied to this body on the next physics system tick.
    /// </summary>
    public readonly Vector2 Force => force;

    /// <summary>
    /// Gets and sets the linear velocity this body is currently travelling in.
    /// </summary>
    private Vector2 linearVelocity;
    
    /// <summary>
    /// Gets and sets the linear velocity this body is currently travelling in.
    /// </summary>
    public readonly Vector2 LinearVelocity => linearVelocity;

    /// <summary>
    /// Gets and sets the rotational vecloty this body is currently travelling in.
    /// </summary>
    private float rotationalVelocity;
    
    /// <summary>
    /// Gets the rotational vecloty this body is currently travelling in.
    /// </summary>
    public readonly float RotationalVelocity => rotationalVelocity;

    /// <summary>
    /// Gets and sets the density.
    /// </summary>
    private float density;

    /// <summary>
    /// Gets the density.
    /// </summary>
    public readonly float Density => density;

    /// <summary>
    /// Gets and sets the mass.
    /// </summary>
    private float mass;

    /// <summary>
    /// Gets the mass.
    /// </summary>
    public readonly float Mass => mass;

    /// <summary>
    /// Gets and sets the restitution.
    /// </summary>
    /// <remarks>
    /// Note: restitution is how 'bouncy' a body is.
    /// </remarks>
    private float restitution;

    /// <summary>
    /// Gets the restitution.
    /// </summary>
    /// <remarks>
    /// Note: restitution is how 'bouncy' a body is.
    /// </remarks>
    public readonly float Restitution => restitution;

    /// <summary>
    /// Gets and sets the area.
    /// </summary>
    private float area;

    /// <summary>
    /// Gets the area.
    /// </summary>
    public readonly float Area => area;

    /// <summary>
    /// Gets and sets the rotational inertia.
    /// </summary>
    private float rotationalInertia;

    /// <summary>
    /// Gets the rotational inertia.
    /// </summary>
    public readonly float RotationalInertia => rotationalInertia;

    /// <summary>
    /// Gets and sets the behvaiour of this body.
    /// </summary>
    public RigidBodyMode Mode;

    /// <summary>
    /// Constructs a rigidbody.
    /// </summary>
    /// <param name="restitution">the restitution ('bounciness').</param>
    /// <param name="density">the density.</param>
    /// <param name="rigidBodyMode">the behaviour to exhibit within the physics system and in relation to other bodies.</param>
    public RigidBody(float restitution, float density, RigidBodyMode rigidBodyMode)
    {
        SetRestitution(restitution);
        SetDensity(density);
        Mode = rigidBodyMode;
    }
    
    /// <summary>
    /// Sets the shape of this rigidbody.
    /// </summary>
    /// <param name="rectangle">The rectangle shape to set this rigidbody with.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public void SetShape(in Rectangle rectangle)
    {
        SetShape(in rectangle, density);
    }

    /// <summary>
    /// Sets the shape of this rigidbody.
    /// </summary>
    /// <param name="rectangle">The rectangle shape to set this rigidbody with.</param>
    /// <param name="density">The density to set to.</param>
    public void SetShape(in Rectangle rectangle, float density)
    {
        // calculate and set the area.
        SetArea(rectangle.Width * rectangle.Height);

        // only set density if it is different.
        if(Math.Math.NearlyEqual(Density, density, 1e-5f) == false)
            SetDensity(density);

        // recalculate mass.
        mass = area * density;

        //  calculate inertia.
        rotationalInertia = RectangleInertiaConst * mass * (rectangle.Width * rectangle.Width + rectangle.Height * rectangle.Height);
    }

    /// <summary>
    /// Sets the shape of this rigidbody.
    /// </summary>
    /// <param name="circle">The circle shape to set this rigidbody with.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public void SetShape(in Circle circle)
    {
        SetShape(in circle, Density);
    }

    /// <summary>
    /// Sets the shape of this rigidbody.
    /// </summary>
    /// <param name="circle">The circle shape to set this rigidbody with.</param>
    /// <param name="density">The density to set to.</param>
    public void SetShape(in Circle circle, float density)
    {
        float radiusSqrd = circle.Radius * circle.Radius;

        // calculate and set the area.
        SetArea(Math.Math.Pi * radiusSqrd);

        // only set density if it is different.
        if(Math.Math.NearlyEqual(Density, density, 1e-5f) == false)
            SetDensity(density);

        // recalculate mass.
        mass = area * density;

        //  calculate inertia.
        rotationalInertia = CircleInertiaConst * mass * radiusSqrd;        
    }

    private void SetDensity(float density)
    {
        if(density < MinDensity || density > MaxDensity)
        {
            throw new ArgumentException($"Cannot set density to '{density}'; Min density is '{MinDensity}' and Max density is '{MaxDensity}'");
        }
        this.density = density;
        
        // recaculate mass.
        mass = area * density;
    }

    private void SetArea(float area)
    {
        if(area < MinBodySize || area > MaxBodySize)
        {
            throw new ArgumentException($"Cannot set area to '{area}'; Max body size is '{MaxBodySize}' and Min body size is '{MinBodySize}'");
        }
        this.area = area;        
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
}