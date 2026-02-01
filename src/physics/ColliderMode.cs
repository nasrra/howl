namespace Howl.Physics;

public enum ColliderMode : byte
{
    /// <summary>
    /// Resolves collisions by separating from the colliding object.
    /// </summary>
    Solid,

    /// <summary>
    /// Responds to collisions by recording the intersection of the colliding object.
    /// </summary>
    Trigger,

    /// <summary>
    /// Resolves collisions be separating the colliding object from this collider.
    /// </summary>
    Kinematic
}