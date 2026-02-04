public readonly struct CollisionManifoldIndex
{
    /// <summary>
    /// Gets the associated index in the collision manifold.
    /// </summary>
    public readonly int Index;

    /// <summary>
    /// Constructs a CollisionManifoldIndex.
    /// </summary>
    /// <param name="index">the associated index in the collision manifold.</param>
    public CollisionManifoldIndex(int index)
    {
        Index = index;
    }
}