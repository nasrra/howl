using System;
using System.Runtime.CompilerServices;
using Howl.Math;
using Howl.Math.Shapes;
using static Howl.Math.Shapes.PolygonRectangle;

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

    public const float MinFriction = 0f;
    public const float MaxFriction = 1f;

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
    /// Gets and sets the physics material.
    /// </summary>
    public PhysicsMaterial PhysicsMaterial;

    /// <summary>
    /// Gets and sets the angular velocity - in radians - this body is currently travelling in.
    /// </summary>
    private float angularVelocity;
    
    /// <summary>
    /// Gets the angular velocity - in radians - this body is currently travelling in.
    /// </summary>
    public readonly float AngularVelocity => angularVelocity;

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
    /// Gets and sets the inverse mass.
    /// </summary>
    private float inverseMass;

    /// <summary>
    /// Gets the inverse mass.
    /// </summary>
    public readonly float InverseMass => inverseMass;

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
    /// Gets and sets the inverse rotational inertia.
    /// </summary>
    public float inverseRotationalInertia;

    /// <summary>
    /// Gets the inverse rotational intertia.
    /// </summary>
    public readonly float InverseRotationalInertia => inverseRotationalInertia; 

    /// <summary>
    /// Gets and sets the friction value.
    /// </summary>
    private float friction;

    /// <summary>
    /// Gets the friction value;
    /// </summary>
    public readonly float Friction => friction;

    /// <summary>
    /// Gets and sets the behvaiour of this body.
    /// </summary>
    public RigidBodyMode Mode;

    /// <summary>
    /// Gets and sets whether or not this rigidbody uses rotational physics.
    /// </summary>
    public bool RotationalPhysics;

    /// <summary>
    /// Constructs a rigidbody.
    /// </summary>
    /// <param name="physicsMaterial">The physics material for collision resolution between bodies.</param>
    /// <param name="restitution">the restitution ('bounciness').</param>
    /// <param name="density">the density of this body.</param>
    /// <param name="rigidBodyMode">the behaviour to exhibit within the physics system and in relation to other bodies.</param>
    /// <param name="roationalPhysics">whether or not this rigidbody uses rotational physics.</param>
    public RigidBody(PhysicsMaterial physicsMaterial, float restitution, float density, RigidBodyMode rigidBodyMode, bool roationalPhysics)
    {
        PhysicsMaterial = physicsMaterial;
        SetRestitution(restitution);
        SetDensity(density);
        Mode = rigidBodyMode;
        RotationalPhysics = roationalPhysics;
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
        inverseMass = 1f / mass;

        //  calculate inertia.
        rotationalInertia = RectangleInertiaConst * mass * (rectangle.Width * rectangle.Width + rectangle.Height * rectangle.Height);
        inverseRotationalInertia = 1f / rotationalInertia;
    }

    /// <summary>
    /// Sets the shape of this rigidbody.
    /// </summary>
    /// <param name="rectangle">The rectangle shape to set this rigidbody with.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public void SetShape(in PolygonRectangle rectangle)
    {
        SetShape(in rectangle, density);
    }

    /// <summary>
    /// Sets the shape of this rigidbody.
    /// </summary>
    /// <param name="rectangle">The rectangle shape to set this rigidbody with.</param>
    /// <param name="density">The density to set to.</param>
    public void SetShape(in PolygonRectangle rectangle, float density)
    {
        float width = Width(rectangle);
        float height = Height(rectangle);

        // calculate and set the area.
        SetArea(width * height);

        // only set density if it is different.
        if(Math.Math.NearlyEqual(Density, density, 1e-5f) == false)
            SetDensity(density);

        // recalculate mass.
        mass = area * density;
        inverseMass = 1f / mass;

        //  calculate inertia.
        rotationalInertia = RectangleInertiaConst * mass * (width * width + height * height);
        inverseRotationalInertia = 1f / rotationalInertia;
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
        inverseMass = 1f/mass;

        //  calculate inertia.
        rotationalInertia = CircleInertiaConst * mass * radiusSqrd;        
        inverseRotationalInertia = 1f/rotationalInertia;
    }

    /// <summary>
    /// Sets the friction value of this body.
    /// </summary>
    /// <remarks>
    /// Note: this function will clamp the passed argument to be between the min and max friction values.
    /// </remarks>
    /// <param name="friction">the friction value to set to.</param>
    public void SetFriction(float friction)
    {
        this.friction = Math.Math.Clamp(friction, MinFriction, MaxFriction);
#if DEBUG
        if(Friction != friction)
            System.Diagnostics.Debug.Assert(false, $"Friction '{friction}' is not between the friction range of '{MinFriction}' and '{MaxFriction}'");
#endif
    }

    /// <summary>
    /// Sets the density of this body.
    /// </summary>
    /// <remarks>
    /// Note: this function will clamp the passed argument to be between the min and max density values.
    /// </remarks>
    /// <param name="density">the density value to set to.</param>
    private void SetDensity(float density)
    {
        this.density = Math.Math.Clamp(density, MinDensity, MaxDensity);
        mass = area * density; // recaculate mass.            
#if DEBUG
        if(Density != density)
            System.Diagnostics.Debug.Assert(false, $"Density '{density}' is not between the density range of '{MinDensity}' and '{MaxDensity}'");
#endif
    }

    /// <summary>
    /// Sets the area of this body.
    /// </summary>
    /// <remarks>
    /// Note: this function will clamp the passed argument to be between the min and max body-size values.
    /// </remarks>
    /// <param name="area">the area value to set to.</param>
    private void SetArea(float area)
    {
        this.area = Math.Math.Clamp(area, MinBodySize, MaxBodySize);
        mass = area * density; // recaculate mass.
#if DEBUG
        if(Area != area)
            System.Diagnostics.Debug.Assert(false, $"Area '{area}' is not within the body-size range of '{MinBodySize}' and '{MaxBodySize}'.");
#endif
    }

    /// <summary>
    /// Sets the restitution - 'bounce' - of this body.
    /// </summary>
    /// <remarks>
    /// Note: this function will clamp the passed argument to be between the min and max restitution values. 
    /// </remarks>
    /// <param name="restitution">the restitution value to set to.</param>
    public void SetRestitution(float restitution)
    {
        this.restitution = Math.Math.Clamp(restitution, MinRestitution, MaxRestitution);
#if DEBUG
        if(Restitution != restitution)
            System.Diagnostics.Debug.Assert(false, $"Restitution '{restitution}' is not within the range of '{MinRestitution}' and '{MaxRestitution}'");
#endif
    }

    /// <summary>
    /// Adds a physics tick-applied force to linear velocity. 
    /// </summary>
    /// <param name="force"></param>
    public void AddLinearForce(Vector2 force)
    {
        this.force += force;    
    }

    /// <summary>
    /// Impulses linear velocity by a force.
    /// </summary>
    /// <param name="force">the amount of force to impulse by.</param>
    public void ImpulseLinearForce(Vector2 force)
    {
        linearVelocity += force;
    }

    /// <summary>
    /// Impulses angular velocity by a force.
    /// </summary>
    /// <param name="force">the amount of force to impulse by.</param>
    public void ImpulseAngularForce(float force)
    {
        angularVelocity += force;
    }

    /// <summary>
    /// Clears all physics tick-applied forces to linear velocity. 
    /// </summary>
    public void ClearForces()
    {
        force = Vector2.Zero;
    }

    /// <summary>
    /// Sets the linear velocity to zero.
    /// </summary>
    public void ClearLinearVelocity()
    {
        linearVelocity = Vector2.Zero;
    }

    /// <summary>
    /// Sets the angular velocity to zero.
    /// </summary>
    public void ClearAngularVelocity()
    {
        angularVelocity = 0;
    }
}