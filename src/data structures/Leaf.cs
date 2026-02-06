using System;
using System.Runtime.CompilerServices;
using Howl.ECS;
using Howl.Math.Shapes;

namespace Howl.DataStructures;

public readonly struct Leaf
{
    /// <summary>
    /// Gets the AABB.
    /// </summary>
    public readonly AABB AABB;

    /// <summary>
    /// Gets the associated gen index.
    /// </summary>
    public readonly GenIndex GenIndex;
    
    /// <summary>
    /// Gets any user-defined flags to distinguish the gen.
    /// </summary>
    public readonly byte Flag;

    /// <summary>
    /// Constructs an leaf.
    /// </summary>
    /// <param name="aabb">The aabb.</param>
    /// <param name="genIndex">The associated gen index.</param>
    /// <param name="flag">any user-defined flags to distinguish this leaf.</param>
    public Leaf(AABB aabb, GenIndex genIndex, byte flag)
    {
        AABB = aabb;
        GenIndex = genIndex;
        Flag = flag;
    }

    /// <summary>
    /// checks whether two leaf nodes are equal.
    /// </summary>
    /// <param name="a">leaf a.</param>
    /// <param name="b">leaf b.</param>
    /// <returns>true, if both leaves are equal; otherwise false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(Leaf a ,Leaf b)
    {
        return a.AABB == b.AABB && a.GenIndex == b.GenIndex && a.Flag == b.Flag;
    }

    /// <summary>
    /// checks whether two leaf nodes are not equal.
    /// </summary>
    /// <param name="a">leaf a.</param>
    /// <param name="b">leaf b.</param>
    /// <returns>true, if the leaves are not equal; otherwise false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(Leaf a ,Leaf b)
    {
        return a.AABB != b.AABB || a.GenIndex != b.GenIndex || a.Flag != b.Flag;        
    }

    /// <summary>
    /// checks whether an object is equal to this.
    /// </summary>
    /// <param name="obj">the object to check equality against.</param>
    /// <returns>true, if the object is equal to this; otherwise false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
    public override bool Equals(object obj)
    {
        return obj is Leaf other && other == this;
    }

    /// <summary>
    /// Gets the hash code.
    /// </summary>
    /// <returns>the hash code.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public override int GetHashCode()
    {
        return HashCode.Combine(AABB, GenIndex, Flag);
    }
}