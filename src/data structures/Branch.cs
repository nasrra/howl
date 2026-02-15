using System;
using System.IO.Compression;
using Howl.Math.Shapes;

namespace Howl.DataStructures;

public unsafe struct Branch
{
    public const int MaxLeaves = 2;
 
    /// <summary>
    /// Gets and sets the children indices. 
    /// </summary>
    public fixed int LeafIndices[MaxLeaves]; 

    /// <summary>
    /// Gets and sets the x-component of the bounding-box minimum vector.
    /// </summary>
    public float BoundingBoxMinX;

    /// <summary>
    /// Gets and sets the y-component of the bounding-box minium vector.
    /// </summary>
    public float BoundingBoxMinY;

    /// <summary>
    /// Gets and sets the x-component of the bounding-box maximum vector.
    /// </summary>
    public float BoundingBoxMaxX;

    /// <summary>
    /// Gets and sets the y-component of the bounding-box maximum vector.
    /// </summary>
    public float BoundingBoxMaxY;

    /// <summary>
    /// Gets and sets the number of child branches, INCLUDING this one. 
    /// </summary>
    public int SubtreeSize;

    /// <summary>
    /// Gets the amount of leaves this branch has.
    /// </summary>
    /// <remarks>
    /// Note: this will be zero if the branch connects to another branch.
    /// </remarks>
    public int LeafCount; 

    /// <summary>
    /// Constructs a branch
    /// </summary>
    /// <param name="leafIndices">the leaf indices</param>
    /// <param name="boundingBoxMinX">the x-component of the bounding-box minimum vector.</param>
    /// <param name="boundingBoxMinY">the y-component of the bounding-box minimum vector.</param>
    /// <param name="boundingBoxMaxX">the x-component of the bounding-box maximum vector.</param>
    /// <param name="boundingBoxMaxY">the y-component of the bounding-box maximum vector.</param>
    /// <param name="subtreeSize">the number of child branches, INCLUDING this one</param>
    /// <param name="leafCount"></param>
    public Branch(
        ReadOnlySpan<int> leafIndices,
        float boundingBoxMinX,
        float boundingBoxMinY,
        float boundingBoxMaxX,
        float boundingBoxMaxY,
        int subtreeSize,
        int leafCount
    )
    {

        BoundingBoxMinX = boundingBoxMinX;
        BoundingBoxMinY = boundingBoxMinY;
        BoundingBoxMaxX = boundingBoxMaxX;
        BoundingBoxMaxY = boundingBoxMaxY;
        SubtreeSize = subtreeSize;
        LeafCount = leafCount;
        SetLeafIndices(ref this, leafIndices);
    }

    /// <summary>
    /// Sets the leaf indices of a branch.
    /// </summary>
    /// <param name="branch">the branch.</param>
    /// <param name="indices">The indices to copy.</param>
    public static void SetLeafIndices(ref Branch branch, ReadOnlySpan<int> indices)
    {
        fixed(int* ptr = branch.LeafIndices)
        {
            for(int i = 0; i < indices.Length; i++)
            {
                ptr[i] = indices[i];
            }
        }

        branch.LeafCount = indices.Length;
    }

    /// <summary>
    /// Gets the leaf indices of a branch.
    /// </summary>
    /// <param name="branch">the branch.</param>
    /// <returns>the span.</returns>
    public static Span<int> GetLeafIndices(in Branch branch)
    {
        Span<int> span;
        fixed(int* ptr = branch.LeafIndices)
        {
            span = new Span<int>(ptr, branch.LeafCount);
        }
        return span;
    }
}
