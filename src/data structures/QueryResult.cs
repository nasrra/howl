using Howl.ECS;

namespace Howl.DataStructures;

public readonly struct QueryResult
{
    /// <summary>
    /// Gets the associated gen index.
    /// </summary>
    public readonly GenIndex GenIndex;

    /// <summary>
    /// Gets any user-defined flags to distinguish the gen index. 
    /// </summary>
    public readonly byte Flag;

    /// <summary>
    /// Constructs a QueryResult.
    /// </summary>
    /// <param name="genIndex">the associated gen index.</param>
    /// <param name="flag">any user-defined flags to ditinguish the gen index.</param>
    public QueryResult(GenIndex genIndex, byte flag)
    {
        GenIndex = genIndex;
        Flag = flag;
    }
}