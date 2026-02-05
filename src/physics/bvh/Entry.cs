using Howl.ECS;
using Howl.Math.Shapes;

namespace Howl.Physics.BVH;

public struct Entry
{
    /// <summary>
    /// Gets the AABB.
    /// </summary>
    public readonly AABB AABB;

    /// <summary>
    /// Gets the GenIndex.
    /// </summary>
    public readonly GenIndex GenIndex;

    /// <summary>
    /// Gets any byte flags associated with the data.
    /// </summary>
    public readonly byte Flag;

    /// <summary>
    /// Constructs and Entry.
    /// </summary>
    /// <param name="aabb">The AABB.</param>
    /// <param name="genIndex">The gen index associated with this entry.</param>
    /// <param name="flag">Any byte flag data.</param>
    public Entry(AABB aabb, GenIndex genIndex, byte flag)
    {
        AABB = aabb;
        GenIndex = genIndex;
        Flag = flag;
    }
}