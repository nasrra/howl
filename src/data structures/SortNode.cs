using Howl.Math;

namespace Howl.DataStructures;

public struct SortNode
{

    /// <summary>
    /// Gets and sets the position x-component.
    /// </summary>
    public float PositionX;

    /// <summary>
    /// Gets and sets the position y-component.
    /// </summary>
    public float PositionY;

    /// <summary>
    /// Gets the associated leaf index.
    /// </summary>
    public int LeafIndex;

    /// <summary>
    /// Constructs a SortNode.
    /// </summary>
    /// <param name="positionX">the positional x-component.</param>
    /// <param name="positionY">the positional y-component.</param>
    /// <param name="leafIndex">the associated leaf index.</param>
    public SortNode(float positionX, float positionY, int leafIndex)
    {
        PositionX = positionX;
        PositionY = positionY;
        LeafIndex = leafIndex;
    }
}