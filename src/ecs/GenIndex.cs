using System;
using System.Runtime.CompilerServices;

namespace Howl.Ecs;

public struct GenIndex
{
    /// <summary>
    /// Constructs an invalid GenIndex.
    /// </summary>
    public static GenIndex Invalid = new GenIndex(-1,-1);

    /// <summary>
    /// Gets the index.
    /// </summary>
    public int Index;

    /// <summary>
    /// Gets the generation.
    /// </summary>
    public int Generation;

    /// <summary>
    /// Constructs a GenIndex.
    /// </summary>
    /// <param name="index">the index.</param>
    /// <param name="generation">the generation.</param>
    public GenIndex(int index, int generation)
    {
        Index = index;
        Generation = generation;
    }

    public override string ToString()
    {
        return $"[GenIndex]: index: {Index} generation: {Generation}";
    }

    /// <summary>
    /// checks whether two gen indices are equal.
    /// </summary>
    /// <param name="a">gen index a</param>
    /// <param name="b">gen index b</param>
    /// <returns>true, if they are equal; otherwise false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static bool operator ==(GenIndex a, GenIndex b)
    {
        return a.Index == b.Index && a.Generation == b.Generation;
    }

    /// <summary>
    /// checks whether two gen indices are not equal. 
    /// </summary>
    /// <param name="a">gen index a.</param>
    /// <param name="b">gen index b.</param>
    /// <returns>true, if they are not equal; otherwise false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static bool operator !=(GenIndex a, GenIndex b)
    {
        return a.Index != b.Index || a.Generation != b.Generation;        
    }

    /// <summary>
    /// checks whether an object is equal to this.
    /// </summary>
    /// <param name="obj">the object to check equality against.</param>
    /// <returns>true, if the object is equal to this; otherwise false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public override bool Equals(object obj)
    {
        return obj is GenIndex other && other == this; 
    }

    /// <summary>
    /// Gets the hash code.
    /// </summary>
    /// <returns>the hash code.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public override int GetHashCode()
    {
        return HashCode.Combine(Index, Generation);
    }
}

