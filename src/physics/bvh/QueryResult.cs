using Howl.ECS;

namespace Howl.Physics.BVH;

public readonly struct QueryResult
{
    public readonly GenIndex GenIndex;
    public readonly byte Flags;

    public QueryResult(GenIndex genIndex, byte flags)
    {
        GenIndex = genIndex;
        Flags = flags;
    }
}