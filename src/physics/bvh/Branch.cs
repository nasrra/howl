using System;
using Howl.Math.Shapes;

namespace Howl.Physics.BVH;

public struct Branch
{
    /// <summary>
    /// Gets the axis aligned bounding box.
    /// </summary>
    public AABB AABB;

    /// <summary>
    /// Gets and sets the index of associated the left-stem.
    /// </summary>
    public Stem LeftStem;

    /// <summary>
    /// Gets ad sets the index of associated the right-stem.
    /// </summary>
    public Stem RightStem;

    /// <summary>
    /// Constructs a Branch.
    /// </summary>
    /// <param name="aabb">The AABB bounds of this branch.</param>
    /// <param name="leftStem">The left-stem.</param>
    /// <param name="rightStem">The right-stem.</param>
    public Branch(
        AABB aabb, 
        Stem leftStem, 
        Stem rightStem
    )
    {
        AABB = aabb;
        LeftStem= leftStem;
        RightStem = rightStem;
    }
}