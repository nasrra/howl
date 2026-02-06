using Howl.Math;

namespace Howl.DataStructures;

public readonly struct SortNode
{
    /// <summary>
    /// Gets the centroid of the node.
    /// </summary>
    public readonly Vector2 Centroid;

    /// <summary>
    /// Gets the associated leaf index.
    /// </summary>
    public readonly int LeafIndex;

    /// <summary>
    /// Constructs a SortNode.
    /// </summary>
    /// <param name="centroid">The centroid of the leaf.</param>
    /// <param name="leafIndex">The associated leaf index.</param>
    public SortNode(Vector2 centroid, int leafIndex)
    {
        Centroid = centroid;
        LeafIndex = leafIndex;
    }
}