using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Xml.Serialization;
using Howl.ECS;
using Howl.Math.Shapes;

namespace Howl.Physics.BVH;

public class BoundingVolumeHierarchy
{
    private List<Entry> entries;
    private List<Leaf> leaves;
    private List<Branch> branches;
    private List<QueryResult> queryResults;

    public BoundingVolumeHierarchy()
    {
        entries = new();
        leaves = new();
        branches = new();
        queryResults = new(4096);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public void AddEntry(Entry entry)
    {
        entries.Add(entry);
    }

    public void Construct()
    {
        ConstructLeaves();
        entries.Clear();
    }

    public void Clear()
    {
        entries.Clear();
        leaves.Clear();
        branches.Clear();
        queryResults.Clear();
    }

    public ReadOnlySpan<Leaf> GetLeavesAsReadOnlySpan()
    {
        return CollectionsMarshal.AsSpan(leaves);
    }

    private void ConstructLeaves()
    {
        Span<Entry> entrySpan = CollectionsMarshal.AsSpan(entries);
        
        Span<float> distances   = stackalloc float[Leaf.MaxEntries];
        Span<int> entryIndices  = stackalloc int[Leaf.MaxEntries];
        Span<int> indices       = stackalloc int[Leaf.MaxEntries]; // gen index.
        Span<int> generations   = stackalloc int[Leaf.MaxEntries];
        Span<byte> flags        = stackalloc byte[Leaf.MaxEntries];
        int entriesCount        = 0;

        for(int i = 0; i < entrySpan.Length - 1; i++)
        {
            ref Entry currentEntry = ref entrySpan[i];

            // move onto the next entry if this one has already been leafed.
            if (currentEntry.Leafed)
            {
                continue;
            }


            // set the first entry to the current entry.
            entriesCount    = 1;
            distances[0]    = 0;
            entryIndices[0] = i; 
            indices[0]      = currentEntry.GenIndex.index;
            generations[0]  = currentEntry.GenIndex.generation;
            flags[0]        = currentEntry.Flag;

            // reset the rest of the previous iterations leaf data.
            for(int j = 1; j < Leaf.MaxEntries; j++)
            {
                distances[j]    = float.MaxValue;
                entryIndices[j] = -1; 
                indices[j]      = -1;
                generations[j]  = -1;
                flags[j]        = 0;
            }

            for(int j = i + 1; j < entrySpan.Length; j++)
            {
                ref Entry otherEntry = ref entrySpan[j];

                // move onto the next entry if this one has already been leafed.
                if (otherEntry.Leafed)
                {
                    continue;
                }

                // get the data relative to the other entry.
                float distance  = currentEntry.AABB.GetCentroid().DistanceSquared(otherEntry.AABB.GetCentroid());
                int entryIndex  = j;
                int index       = otherEntry.GenIndex.index;
                int generation  = otherEntry.GenIndex.generation;
                byte flag       = otherEntry.Flag;

                for(int k = 1; k < Leaf.MaxEntries; k++)
                {
                    if(distance < distances[k])
                    {                        
                        // create temp values of the data that is swapped
                        // out, the swapeed entry is used in later iteration loops.
                        // this is to ensure that if the swapped entry is still one 
                        // of the closest entries, its not lost and put back into the 
                        // data list for the leaf node. 

                        float swappedDistance   = distances[k];
                        int swappedIndex        = indices[k];
                        int swappedGeneration   = generations[k];
                        int swappedEntryIndex   = entryIndices[k];
                        byte swappedFlag        = flags[k];

                        distances[k]    = distance;
                        indices[k]      = index;
                        generations[k]  = generation;
                        entryIndices[k] = entryIndex;
                        flags[k]        = flag;

                        distance    = swappedDistance;
                        index       = swappedIndex;
                        generation  = swappedGeneration;
                        entryIndex  = swappedEntryIndex;
                        flag        = swappedFlag;
                    }
                }
            }

            ref Entry firstEntry = ref entrySpan[entryIndices[0]];
            firstEntry.Leafed = true;

            // create a cummulative AABB of the entry AABB's for the leaf
            // the holds them.
            AABB leafAABB = firstEntry.AABB;
            for(int j = 1; j < Leaf.MaxEntries; j++)
            {
                if(entryIndices[j] == -1)
                {
                    break;
                }
                entriesCount++;
                ref Entry nextEntry = ref entrySpan[entryIndices[j]];
                nextEntry.Leafed = true;
                leafAABB = new AABB(leafAABB, nextEntry.AABB);
            }

            // add the new leaf to the leaves list.
            leaves.Add(new Leaf(indices, generations, flags, leafAABB, entriesCount));
        }
    }

    public ReadOnlySpan<QueryResult> Query(AABB aabb)
    {
        queryResults.Clear();

        Span<Leaf> leafSpan = CollectionsMarshal.AsSpan(leaves);
        Span<GenIndex> genIndices = stackalloc GenIndex[Leaf.MaxEntries];

        for(int i = 0; i < leafSpan.Length; i++)
        {
            ref Leaf leaf = ref leafSpan[i];
            if(AABB.Intersect(aabb, leaf.AABB))
            {
                leaf.GetGenIndices(ref genIndices, out int written);
                ReadOnlySpan<byte> flags = leaf.GetFlags();
                for(int j = 0; j < written; j++)
                {
                    queryResults.Add(new(genIndices[j], flags[j]));
                }
            }
        }

        ReadOnlySpan<QueryResult> span = CollectionsMarshal.AsSpan(queryResults);
        return span;
    }

}