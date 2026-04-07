using Howl.ECS;

/// <summary>
/// This is used as a component link between Howl ECS and the Howl physics simulation.
/// </summary>
public struct PhysicsBodyId
{
    /// <summary>
    /// Gets and sets the gen id of the associated physics body in the physics simulation.
    /// </summary>
    public GenId GenId;

    /// <summary>
    /// Constructs a new Physics Body Id.
    /// </summary>
    /// <param name="genId">The gen id of the associated physics body in the physics simulation</param>
    public PhysicsBodyId(GenId genId)
    {
        GenId = genId;
    }
}