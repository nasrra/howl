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
    /// Gets and sets the x-component of the force to be applied to this body on the next physics system tick.
    /// </summary>
    public float ForceX;

    /// <summary>
    /// Gets and sets the y-component of the force to be applied to this body on the next physics system tick.
    /// </summary>
    public float ForceY;

    /// <summary>
    /// Gets and sets the x-compoennt of the linear velocity this body is currently travelling in.
    /// </summary>
    public float LinearVelocityX;

    /// <summary>
    /// Gets and sets the y-compoennt of the linear velocity this body is currently travelling in.
    /// </summary>
    public float LinearVelocityY;

    /// <summary>
    /// Gets and sets the angular velocity - in radians - this body is currently travelling in.
    /// </summary>
    public float AngularVelocity;
    
    /// <summary>
    /// Gets and sets the density.
    /// </summary>
    public float Density;

    /// <summary>
    /// Gets and sets the mass.
    /// </summary>
    public float Mass;

    /// <summary>
    /// Gets and sets the inverse mass.
    /// </summary>
    public float InverseMass;

    /// <summary>
    /// Gets and sets the restitution.
    /// </summary>
    /// <remarks>
    /// Note: restitution is how 'bouncy' a body is.
    /// </remarks>
    public float Restitution;

    /// <summary>
    /// Gets and sets the area.
    /// </summary>
    public float Area;

    /// <summary>
    /// Gets and sets the rotational inertia.
    /// </summary>
    public float RotationalInertia;

    /// <summary>
    /// Gets and sets the inverse rotational inertia.
    /// </summary>
    public float InverseRotationalInertia;

    /// <summary>
    /// Gets and sets the friction value.
    /// </summary>
    public float Friction;

    /// <summary>
    /// Gets and sets the behvaiour of this body.
    /// </summary>
    public RigidBodyMode Mode;

    /// <summary>
    /// Gets and sets the physics material.
    /// </summary>
    public PhysicsMaterial PhysicsMaterial;

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
        SetRestitution(ref this, restitution);
        SetDensity(ref this, density);
        Mode = rigidBodyMode;
        RotationalPhysics = roationalPhysics;
    }
    
    /// <summary>
    /// Sets the shape of a rigidbody.
    /// </summary>
    /// <param name="rigidBody">The rigidbody.</param>
    /// <param name="rectangle">The rectangle shape to set the rigidbody with.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void SetShape(ref RigidBody rigidBody, in Rectangle rectangle)
    {
        SetShape(ref rigidBody, in rectangle, rigidBody.Density);
    }

    /// <summary>
    /// Sets the shape of a rigidbody.
    /// </summary>
    /// <param name="rigidBody">the rigidbody.</param>
    /// <param name="rectangle">The rectangle shape to set this rigidbody with.</param>
    /// <param name="density">The density to set to.</param>
    public static void SetShape(ref RigidBody rigidBody, in Rectangle rectangle, float density)
    {
        // calculate and set the area.
        SetArea(ref rigidBody, rectangle.Width * rectangle.Height);

        // only set density if it is different.
        if(Math.Math.NearlyEqual(rigidBody.Density, density, 1e-5f) == false)
            SetDensity(ref rigidBody, density);

        // recalculate mass.
        rigidBody.Mass = rigidBody.Area * density;
        rigidBody.InverseMass = 1f / rigidBody.Mass;

        //  calculate inertia.
        rigidBody.RotationalInertia = RectangleInertiaConst * rigidBody.Mass * (rectangle.Width * rectangle.Width + rectangle.Height * rectangle.Height);
        rigidBody.InverseRotationalInertia = 1f / rigidBody.RotationalInertia;
    }

    /// <summary>
    /// Sets the shape of a rigidbody.
    /// </summary>
    /// <param name="rigidBody">the rigid body.</param>
    /// <param name="rectangle">The rectangle shape to set this rigidbody with.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void SetShape(ref RigidBody rigidBody, in PolygonRectangle rectangle)
    {
        SetShape(ref rigidBody, in rectangle, rigidBody.Density);
    }

    /// <summary>
    /// Sets the shape of a rigidbody.
    /// </summary>
    /// <param name="rigidBody">the rigidbody.</param>
    /// <param name="rectangle">the rectangle shape to set this rigidbody with.</param>
    /// <param name="density">the density to set to.</param>
    public static void SetShape(ref RigidBody rigidBody, in PolygonRectangle rectangle, float density)
    {
        float width = Width(rectangle);
        float height = Height(rectangle);

        // calculate and set the area.
        SetArea(ref rigidBody, width * height);

        // only set density if it is different.
        if(Math.Math.NearlyEqual(rigidBody.Density, density, 1e-5f) == false)
            SetDensity(ref rigidBody, density);

        // recalculate mass.
        rigidBody.Mass = rigidBody.Area * density;
        rigidBody.InverseMass = 1f / rigidBody.Mass;

        //  calculate inertia.
        rigidBody.RotationalInertia = RectangleInertiaConst * rigidBody.Mass * (width * width + height * height);
        rigidBody.InverseRotationalInertia = 1f / rigidBody.RotationalInertia;
    }

    /// <summary>
    /// Sets the shape of a rigidbody.
    /// </summary>
    /// <param name="rigidBody">the rigidbody.</param>
    /// <param name="circle">The circle shape to set this rigidbody with.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void SetShape(ref RigidBody rigidBody, in Circle circle)
    {
        SetShape(ref rigidBody, in circle, rigidBody.Density);
    }

    /// <summary>
    /// Sets the shape of a rigidbody.
    /// </summary>
    /// <param name="rigidBody">the rigid body.</param>
    /// <param name="circle">The circle shape to set this rigidbody with.</param>
    /// <param name="density">The density to set to.</param>
    public static void SetShape(ref RigidBody rigidBody, in Circle circle, float density)
    {
        float radiusSqrd = circle.Radius * circle.Radius;

        // calculate and set the area.
        SetArea(ref rigidBody, Math.Math.Pi * radiusSqrd);

        // only set density if it is different.
        if(Math.Math.NearlyEqual(rigidBody.Density, density, 1e-5f) == false)
            SetDensity(ref rigidBody, density);

        // recalculate mass.
        rigidBody.Mass = rigidBody.Area * density;
        rigidBody.InverseMass = 1f / rigidBody.Mass;

        //  calculate inertia.
        rigidBody.RotationalInertia = CircleInertiaConst * rigidBody.Mass * radiusSqrd;        
        rigidBody.InverseRotationalInertia = 1f / rigidBody.RotationalInertia;
    }

    /// <summary>
    /// Sets the friction value of a rigidbody body.
    /// </summary>
    /// <remarks>
    /// Note: this function will clamp the passed argument to be between the min and max friction values.
    /// </remarks>
    /// <param name="rigidBody">the rigidbody.</param>
    /// <param name="friction">the friction value to set to.</param>
    public static void SetFriction(ref RigidBody rigidBody, float friction)
    {
        rigidBody.Friction = Math.Math.Clamp(friction, MinFriction, MaxFriction);
#if DEBUG
        if(rigidBody.Friction != friction)
            System.Diagnostics.Debug.Assert(false, $"Friction '{friction}' is not between the friction range of '{MinFriction}' and '{MaxFriction}'");
#endif
    }

    /// <summary>
    /// Sets the density of a rigidbody.
    /// </summary>
    /// <remarks>
    /// Note: this function will clamp the passed argument to be between the min and max density values.
    /// </remarks>
    /// <param name="rigidBody">the rigid body.</param>
    /// <param name="density">the density value to set to.</param>
    private static void SetDensity(ref RigidBody rigidBody, float density)
    {
        rigidBody.Density = Math.Math.Clamp(density, MinDensity, MaxDensity);
        rigidBody.Mass = rigidBody.Area * density; // recaculate mass.            
#if DEBUG
        if(rigidBody.Density != density)
            System.Diagnostics.Debug.Assert(false, $"Density '{density}' is not between the density range of '{MinDensity}' and '{MaxDensity}'");
#endif
    }

    /// <summary>
    /// Sets the area of a rigid body.
    /// </summary>
    /// <remarks>
    /// Note: this function will clamp the passed argument to be between the min and max body-size values.
    /// </remarks>
    /// <param name="rigidBody">the rigidbody.</param>
    /// <param name="area">the area value to set to.</param>
    private static void SetArea(ref RigidBody rigidBody, float area)
    {
        rigidBody.Area = Math.Math.Clamp(area, MinBodySize, MaxBodySize);
        rigidBody.Mass = area * rigidBody.Density; // recaculate mass.
#if DEBUG
        if(rigidBody.Area != area)
            System.Diagnostics.Debug.Assert(false, $"Area '{area}' is not within the body-size range of '{MinBodySize}' and '{MaxBodySize}'.");
#endif
    }

    /// <summary>
    /// Sets the restitution - 'bounce' - of a rigid body.
    /// </summary>
    /// <remarks>
    /// Note: this function will clamp the passed argument to be between the min and max restitution values. 
    /// </remarks>
    /// <param name="rigidBody">the rigidbody.</param>
    /// <param name="restitution">the restitution value to set to.</param>
    public static void SetRestitution(ref RigidBody rigidBody, float restitution)
    {
        rigidBody.Restitution = Math.Math.Clamp(restitution, MinRestitution, MaxRestitution);
#if DEBUG
        if(rigidBody.Restitution != restitution)
            System.Diagnostics.Debug.Assert(false, $"Restitution '{restitution}' is not within the range of '{MinRestitution}' and '{MaxRestitution}'");
#endif
    }

    /// <summary>
    /// Adds a physics tick-applied force to a rigidbody's linear velocity. 
    /// </summary>
    /// <param name="rigidBody">the rigid body.</param>
    /// <param name="force">the force vector.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void AddLinearForce(ref RigidBody rigidBody, Vector2 force)
    {
        AddLinearForce(ref rigidBody, force.X, force.Y);
    }

    /// <summary>
    /// Adds a physics tick-applied force to a rigidbody's linear velocity. 
    /// </summary>
    /// <param name="rigidBody">the rigid body.</param>
    /// <param name="forceX">the x-component of the force vector.</param>
    /// <param name="forceY">the y-component of the force vector.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void AddLinearForce(ref RigidBody rigidBody, float forceX, float forceY)
    {
        rigidBody.ForceX += forceX;
        rigidBody.ForceY += forceY;        
    }


    /// <summary>
    /// Impulses a rigidbody's linear velocity by a force.
    /// </summary>
    /// <param name="rigidBody">the rigid body.</param>
    /// <param name="force">the amount of force to impulse by.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void ImpulseLinearForce(ref RigidBody rigidBody, Vector2 force)
    {
        ImpulseLinearForce(ref rigidBody, force.X, force.Y);
    }

    /// <summary>
    /// Impulses a rigidbody's linear velocity by a force.
    /// </summary>
    /// <param name="rigidBody">the rigid body.</param>
    /// <param name="forceX">the x-component of the amount of force to impulse by.</param>
    /// <param name="forceY">the y-component of the amount of force to impulse by.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void ImpulseLinearForce(ref RigidBody rigidBody, float forceX, float forceY)
    {
        rigidBody.LinearVelocityX += forceX;
        rigidBody.LinearVelocityY += forceY;
    }

    /// <summary>
    /// Impulses a rigidbody's angular velocity by a force.
    /// </summary>
    /// <param name="rigidBody">the rigid body.</param>
    /// <param name="force">the amount of force to impulse by.</param>
    public static void ImpulseAngularForce(ref RigidBody rigidBody, float force)
    {
        rigidBody.AngularVelocity += force;
    }

    /// <summary>
    /// Clears a rigidbody's physics tick-applied forces to linear velocity. 
    /// </summary>
    /// <param name="rigidBody">the rigidbody.</param>
    public static void ClearForces(ref RigidBody rigidBody)
    {
        rigidBody.ForceX = 0;
        rigidBody.ForceY = 0;
    }

    /// <summary>
    /// Sets a rigidbody's linear velocity to zero.
    /// </summary>
    /// <param name="rigidBody">the rigid body.</param>
    public void ClearLinearVelocity(ref RigidBody rigidBody)
    {
        rigidBody.LinearVelocityX = 0;
        rigidBody.LinearVelocityY = 0;
    }

    /// <summary>
    /// Sets a rigidbody's angular velocity to zero.
    /// </summary>
    public void ClearAngularVelocity(ref RigidBody rigidBody)
    {
        rigidBody.AngularVelocity = 0;
    }
}