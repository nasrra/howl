namespace Howl.Physics;

/// <remarks>
///     Note:
///     <list type="bullet">
///         <item>
///             The default assumed state for any collision type associated between two pysics bodies is
///             that there was none. When no flags are set at all - excluding <c><see cref="SetThisStep"/></c> -  it means that there was
///             no collision. 
///         </item>
///         <item>
///             No more than two flags can be set at a time, and one of them must be <c><see cref="SetThisStep"/></c>.
///         </item>
///     </list>
/// </remarks>
public enum CollisionType : byte
{
    None = 0,

    /// <summary>
    ///     Whether or not this collision type has had a flag set during a given system step.
    /// </summary>
    /// <remarks>
    ///     This is an internal system flag so the physics system knows when to turn off all flags and set the collision type back to empty; 
    ///     signifying that the colliders are not colliding with eachother.
    /// </remarks>
    SetThisStep = 1<<0,   

    /// <summary>
    ///     The two colliders have just entered one or the other's trigger collider area.
    /// </summary>
    TriggerEnter = 1<<1,

    /// <summary>
    ///     The two colliders are within one or the other's trigger collider area.
    /// </summary>
    Triggering = 1<<2,

    /// <summary>
    ///     The two colliders have left one or the other's trigger collider area.
    /// </summary>
    TriggerExit = 1<<3,

    /// <summary>
    ///     The two colliders have just collided with one another.
    /// </summary>
    CollisionEnter = 1<<4,

    /// <summary>
    ///     The two colliders have remained colliding/in contact with one another.
    /// </summary>
    Colliding = 1<<5,

    /// <summary>
    ///     The two colliders have just stopped colliding with one another.
    /// </summary>
    CollisionExit = 1<<6
}