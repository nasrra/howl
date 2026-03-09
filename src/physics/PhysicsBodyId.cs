using Howl.ECS;

/// <summary>
/// This is used as a component link between Howl ECS and the Howl physics simulation.
/// </summary>
public struct PhysicsBodyId
{
    /// <summary>
    /// Gets and sets the gen index of the associated physics body in the physics simulation.
    /// </summary>
    public GenIndex GenIndex;

    /// <summary>
    /// Constructs a new Physics Body Id.
    /// </summary>
    /// <param name="genIndex">The gen index of the associated physics body in the physics simulation</param>
    public PhysicsBodyId(GenIndex genIndex)
    {
        GenIndex = genIndex;
    }
}