
namespace Howl.DataStructures;

public readonly struct SpatialPair
{
    /// <summary>
    /// The owner of this spatial pair.
    /// </summary>
    public readonly QueryResult Owner;

    /// <summary>
    /// The other of this spatial pair.
    /// </summary>
    public readonly QueryResult Other;

    /// <summary>
    /// Constructs a spatial pair.
    /// </summary>
    /// <param name="owner">The owner of this spatial pair</param>
    /// <param name="other">The other of this spatial pair.</param>
    public SpatialPair(QueryResult owner, QueryResult other)
    {
        Owner = owner;
        Other = other;
    }
}