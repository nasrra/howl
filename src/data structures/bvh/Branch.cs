using System;
using Howl.Math.Shapes;

public unsafe struct Branch
{
    public const int MaxLeaves = 2;
 
    /// <summary>
    /// Gets and sets the AABB of this node.
    /// </summary>
    public AABB AABB;

    /// <summary>
    /// Gets and sets the number of child branches, INCLUDING this one. 
    /// </summary>
    public int SubtreeSize;

    /// <summary>
    /// Gets and sets the children indices. 
    /// </summary>
    private fixed int leafIndices[MaxLeaves]; 

    /// <summary>
    /// Gets the amount of leaves this branch has.
    /// </summary>
    public ushort LeafCount; // 0 if internal

    /// <summary>
    /// Constructs a branch
    /// </summary>
    /// <param name="subtreeSize">the number of child branches, INCLUDING this one</param>
    /// <param name="indices"></param>
    public Branch(int subtreeSize, ReadOnlySpan<int> indices)
    {
        SubtreeSize = subtreeSize;
        SetLeafIndices(indices);
    }

    /// <summary>
    /// Sets the leaf indices.
    /// </summary>
    /// <param name="indices">The indices to copy.</param>
    public void SetLeafIndices(ReadOnlySpan<int> indices)
    {
        fixed(int* ptr = leafIndices)
        {
            for(int i = 0; i < indices.Length; i++)
            {
                ptr[i] = indices[i];
                LeafCount++;
            }
        }

    }

    /// <summary>
    /// Gets the leaf indices.
    /// </summary>
    /// <returns></returns>
    public ReadOnlySpan<int> GetLeafIndices()
    {
        ReadOnlySpan<int> span;
        fixed(int* ptr = leafIndices)
        {
            span = new ReadOnlySpan<int>(ptr, LeafCount);
        }
        return span;
    }
}
