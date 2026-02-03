using System.ComponentModel;
using Howl.Physics.BVH;

public struct Stem
{

    /// <summary>
    /// Gets the index value that indicates whether or not this Child stores valid data.
    /// </summary>
    private const int InvalidIndex = -1;

    public static Stem Invalid = new Stem(StemType.Leaf, InvalidIndex);

    /// <summary>
    /// Gets the type of data the index stores.
    /// </summary>
    public readonly StemType Type;

    /// <summary>
    /// Gets the index of the associated child data. 
    /// </summary>
    public readonly int Index;

    /// <summary>
    /// Constructs a Child.
    /// </summary>
    /// <param name="type">the type of data the index stores.</param>
    /// <param name="index">The index of the associated child data.</param>
    public Stem(StemType type, int index)
    {
        Type = type;
        Index = index;
    }

    /// <summary>
    /// Gets whether or not the stored index points to valid data.
    /// </summary>
    /// <returns>true, if the index is valid; otherwise false.</returns>
    public bool IsValid()
    {
        return Index != InvalidIndex;
    }

    public static bool operator ==(Stem a, Stem b)
    {
        return a.Index == b.Index && a.Type == b.Type;
    }

    public static bool operator !=(Stem a, Stem b)
    {
        return a.Index != b.Index || a.Type != b.Type;        
    }

    public override bool Equals(object obj)
    {
        return obj is Stem other && other == this;
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }
}