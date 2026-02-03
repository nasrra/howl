using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks.Dataflow;
using Howl.ECS;
using Howl.Math.Shapes;

namespace Howl.Physics.BVH;

public unsafe struct Leaf
{
    /// <summary>
    /// Gets the maximum amount of entries per leaf.
    /// </summary>
    public const int MaxEntries = 4;

    /// <summary>
    /// Gets and sets the indices array from GenIndex.
    /// </summary>
    private fixed int indices[4];

    /// <summary>
    /// Gets and sets the generations array from GenIndex.
    /// </summary>
    private fixed int generations[4];

    /// <summary>
    /// Gets the AABB.
    /// </summary>
    public readonly AABB AABB;

    /// <summary>
    /// Gets the amount of entries stored within this leaf.
    /// </summary>
    public readonly int EntriesCount;

    /// <summary>
    /// Gets and sets the flags array.
    /// </summary>
    private fixed byte flags[4];

    /// <summary>
    /// Constructs a Leaf.
    /// </summary>
    /// <param name="indices">A span of indices from GenIndex.</param>
    /// <param name="generations">A span of generations from GenIndex.</param>
    /// <param name="flags">Any byte flags to associate with the data.</param>
    /// <exception cref="ArgumentException"></exception>
    public Leaf(Span<int> indices, Span<int> generations, Span<byte> flags, AABB aabb)
    {
        if(indices.Length != MaxEntries)
        {
            throw new ArgumentException($"indices length '{indices.Length}' is not eqaul to MaxEntries '{MaxEntries}'");
        }
        if(generations.Length != MaxEntries)
        {
            throw new ArgumentException($"generations length '{generations.Length}' is not eqaul to MaxEntries '{MaxEntries}'");
        }
        if(flags.Length != MaxEntries)
        {
            throw new ArgumentException($"flags length '{flags.Length}' is not eqaul to MaxEntries '{MaxEntries}'");
        }

        // copy indices.
        fixed(int* indicesPtr = this.indices)
        {
            for(int i = 0; i < MaxEntries; i++)
            {
                indicesPtr[i] = indices[i];
            }
        }

        // copy generations.
        fixed(int* generationsPtr = this.generations)
        {
            for(int i = 0; i < MaxEntries; i++)
            {
                generationsPtr[i] = generations[i];
            }
        }

        // copy flags.
        fixed(byte* flagsPtr = this.flags)
        {
            for(int i = 0; i < MaxEntries; i++)
            {
                flagsPtr[i] = flags[i];
            }
        }

        AABB = aabb;
    }

    /// <summary>
    /// Gets the indices stored as a readonly span.
    /// </summary>
    /// <returns>A readonly span.</returns>
    public ReadOnlySpan<int> GetIndices()
    {
        fixed(int* indicesPtr = indices)
        {
            return new ReadOnlySpan<int>(indicesPtr, EntriesCount);
        }
    }

    /// <summary>
    /// Gets the generations stored as a readonly span.
    /// </summary>
    /// <returns>A readonly span.</returns>
    public ReadOnlySpan<int> GetGenerations()
    {
        fixed(int* generationsPtr = generations)
        {
            return new ReadOnlySpan<int>(generationsPtr, EntriesCount);
        }
    }

    /// <summary>
    /// Mutates a given gen index span to contain the gen indexes stored within this leaf.
    /// </summary>
    /// <param name="span">The span to write the gen indexes to.</param>
    /// <param name="written">The count of gen indexes written.</param>
    /// <exception cref="ArgumentException">throws if the span length is not equal to MaxEntries.</exception>
    public void GetGenIndices(ref Span<GenIndex> span, out int written)
    {
        if(span.Length != MaxEntries)
        {
            throw new ArgumentException($"outSpan length '{span.Length}' is not equal to max entries '{MaxEntries}'");
        }

        ReadOnlySpan<int> indices = GetIndices();
        ReadOnlySpan<int> generations = GetGenerations();
        
        written = EntriesCount;

        for(int i = 0; i < EntriesCount; i++)
        {
            span[i] = new GenIndex(indices[i], generations[i]);
        }
    }
}