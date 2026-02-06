using Howl.ECS;
using Howl.Math.Shapes;

public readonly struct Leaf
{
    /// <summary>
    /// Gets the AABB.
    /// </summary>
    public readonly AABB AABB;

    /// <summary>
    /// Gets the associated gen index.
    /// </summary>
    public readonly GenIndex GenIndex;
    
    /// <summary>
    /// Gets any user-defined flags to distinguish the gen.
    /// </summary>
    public readonly byte Flag;

    /// <summary>
    /// Constructs an leaf.
    /// </summary>
    /// <param name="aabb">The aabb.</param>
    /// <param name="genIndex">The associated gen index.</param>
    /// <param name="flag">any user-defined flags to distinguish this leaf.</param>
    public Leaf(AABB aabb, GenIndex genIndex, byte flag)
    {
        AABB = aabb;
        GenIndex = genIndex;
        Flag = flag;
    }
}