namespace Howl.Physics;

public enum ContactState : byte
{
    /// <summary>
    ///     There is no collision between the two bodies.
    /// </summary>
    None,

    /// <summary>
    ///     The two colliders have just started contacting with one another.
    /// </summary>
    Enter,

    /// <summary>
    ///     The two colliders are in sustained contact with one another.
    /// </summary>
    Sustain,

    /// <summary>
    ///     The two colliders have just left contact with one another.
    /// </summary>
    Exit,
}