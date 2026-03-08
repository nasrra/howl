using System;

namespace Howl.Physics;

[Flags]
public enum PhysicsBodyType : byte
{
    None = 0,

    CircleShape = 1<<0,
    
    RectangleShape = 1<<1,
    
    PolygonShape = 1<<2,

    /// <summary>
    /// Resolves collisions by separating from the colliding object.
    /// </summary>
    SolidCollider = 1<<3,

    /// <summary>
    /// Responds to collisions by recording the intersection of the colliding object.
    /// </summary>
    TriggerCollider = 1<<4,

    /// <summary>
    /// Resolves collisions be separating the colliding object from this collider.
    /// </summary>
    KinematicCollider = 1<<5,

    DynamicRigidbody = 1<<6,

    KinematicRigidbody = 1<<7,
}