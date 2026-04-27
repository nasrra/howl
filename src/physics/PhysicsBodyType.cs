using System;

namespace Howl.Physics;

/// <remarks>
/// Note: 
/// the default assumed state for any physics body in the physics system is that they:
/// - have a collider (and will always have one).
/// - are solid (Resolves collisions by separating from the colliding object.)
/// - is of a circle shape.
/// </remarks>
[Flags]
public enum PhysicsBodyFlags : int
{
    None = 0,
        
    /// <summary>
    /// Whether or not a body is a rectangle.
    /// </summary>
    RectangleShape = 1<<0,

    /// <summary>
    /// Whether or not a physics body slot has been allocated in the physics system.
    /// </summary>    
    /// </remarks>
    /// Note: this flag indicates whether or not a slot in a physics body array is free and available for reuse.
    /// <remarks>
    Allocated = 1<<1,

    /// <summary>
    /// Responds to collisions by recording the intersection of a colliding object.
    /// </summary>
    Trigger = 1<<2,

    /// <summary>
    /// Turns off collision resolution for this physics body.
    /// </summary>
    Kinematic = 1<<3,

    // HasPhysicsMaterial = 1<<4,
    RotationalPhysics = 1 << 4,

    /// <summary>
    /// Whether or not a physics body has a rigidbody.
    /// </summary>
    RigidBody = 1<<5,

    /// <summary>
    /// Whether or not a physics body is in active within the physics simulation
    /// </summary>
    Active = 1<<6,

    /// <summary>
    /// Whether or not a phsyics body is allocated and active.
    /// </summary>
    InUse = Allocated | Active,
}