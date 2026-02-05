using System;
using Howl.Math.Shapes;

namespace Howl.Physics.BVH;

public unsafe struct Branch
{
    /// <summary>
    /// Gets the max amount of child branch indices a branach can have.
    /// </summary>
    public const int MaxIndices = 3; // NOTE: this should always be an odd number.

    /// <summary>
    /// Gets the axis aligned bounding box.
    /// </summary>
    public readonly AABB AABB;

    /// <summary>
    /// Gets and sets the child branch indices.
    /// </summary>
    private fixed int childBranchIndices[MaxIndices];

    /// <summary>
    /// Gets the amount of child branch indicies stored within this branch 
    /// </summary>
    public readonly int IndicesCount;

    /// <summary>
    /// Gets the type of children this branch stores.
    /// </summary>
    public readonly ChildrenType ChildrenType;

    /// <summary>
    /// Constructs a Branch.
    /// </summary>
    /// <param name="aabb">The AABB bounds of this branch.</param>
    /// <param name="leftStem">The left-stem.</param>
    /// <param name="rightStem">The right-stem.</param>
    /// <param name="childrenType">the type of children this branch stores</param>
    /// <exception cref="ArgumentException"></exception>
    public Branch(
        AABB aabb,
        Span<int> childBranchIndices,
        int indicesCount,
        ChildrenType childrenType
    )
    {
        if(childBranchIndices.Length != MaxIndices)
        {
            throw new ArgumentException($"childBranchIndices length '{childBranchIndices.Length}' is not equal to MaxIndices '{MaxIndices}'");
        }
        
        AABB        = aabb;
    
        // copy indices
        fixed(int* indicesPtr = childBranchIndices)
        {
            for(int i = 0; i <  MaxIndices; i++)
            {
                this.childBranchIndices[i] = childBranchIndices[i];
            }
        }

        IndicesCount = indicesCount;
        ChildrenType = childrenType;
    }

    public ReadOnlySpan<int> GetChildIndicesAsReadOnlySpan()
    {
        ReadOnlySpan<int> span;
        fixed(int* ptr = childBranchIndices)
        {
            span = new ReadOnlySpan<int>(ptr, IndicesCount);
        }
        return span;
    }
}