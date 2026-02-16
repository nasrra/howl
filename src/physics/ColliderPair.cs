using Howl.ECS;

namespace Howl.Physics;

public struct ColliderPair
{
    /// <summary>
    /// Gets and sets the gen index for collider A.
    /// </summary>
    public GenIndex ColliderA;

    /// <summary>
    /// Gets and sets the gen index for collider B.
    /// </summary>
    public GenIndex ColliderB;

    /// <summary>
    /// Constructs a new collider pair.
    /// </summary>
    /// <param name="colliderA">the gen index for collider a.</param>
    /// <param name="colliderB">the gen index for collider b.</param>
    public ColliderPair(GenIndex colliderA, GenIndex colliderB)
    {
        ColliderA = colliderA;
        ColliderB = colliderB;
    }
}