
namespace Howl.Physics;

public struct ColliderParameters
{
    /// <summary>
    /// Gets and sets the collider mode.
    /// </summary>
    public ColliderMode Mode;

    /// <summary>
    /// Gets and sets whether or not to calcuate contact points for collisions.
    /// </summary>
    public bool CalculateContactPoints;

    /// <summary>
    /// Constructs a ColliderParameters.
    /// </summary>
    /// <param name="mode">the collider mode.</param>
    /// <param name="calculateContactPoints">whether or not to calculat contact points for collisions.</param>
    public ColliderParameters(ColliderMode mode, bool calculateContactPoints)
    {
        Mode = mode;
        CalculateContactPoints = calculateContactPoints;
    }
}