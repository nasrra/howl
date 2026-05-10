namespace Howl.Physics.Collisions;

/// <summary>
///     A pair of indices that are registered within a collision manifold.
/// </summary>
public struct CollisionIndexPair
{
    public int AToB;
    public int BToA;

    public CollisionIndexPair(int aToB, int bToA)
    {
        AToB = aToB;
        BToA = bToA;
    }
}