namespace Howl.Physics;

/// <summary>
///     Fixed-Stride Array of Gen Ids.
/// </summary>
public class Fsa_GenId
{
    /// <summary>
    ///     The collision type values.
    /// </summary>
    public GenId[] GenIds;

    /// <summary>
    ///     The append counts for all entries.
    /// </summary>
    public int[] AppendCounts;

    public Fsa_GenId(int stride, int maxEntries)
    {
        GenIds = new GenId[stride * maxEntries];
        AppendCounts = new int[maxEntries];
    }

    public static void Append(Fsa_GenId state, CollisionType collisionType)
    {
        
    }
}