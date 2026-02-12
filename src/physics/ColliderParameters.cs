
namespace Howl.Physics;

public struct ColliderParameters
{
    /// <summary>
    /// Gets and sets the collider mode.
    /// </summary>
    public ColliderMode Mode;

    /// <summary>
    /// Constructs a ColliderParameters.
    /// </summary>
    /// <param name="mode">the collider mode.</param>
    public ColliderParameters(ColliderMode mode)
    {
        Mode = mode;
    }
}