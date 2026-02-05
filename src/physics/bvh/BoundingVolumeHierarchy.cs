using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Xml.Serialization;
using Howl.ECS;
using Howl.Math;
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

    public ReadOnlySpan<Branch> GetBranchesAsReadOnlySpan()
    {
        return CollectionsMarshal.AsSpan(branches);
    }

    public void Construct()
    {
        ConstructLeaves();

        Span<Leaf> leafSpan = CollectionsMarshal.AsSpan(leaves);

        // create a branch span with the size of the amount of 
        // branches that will be created ahead of time. This is done as
        // lookups and insertion into a span is faster than using the branches list.
        // copying at the end is neglible in comparison to the direct memory access speeds.
        
        // the amount of branches that will be created in a give step.
        int branchAmountStep = leafSpan.Length;
        int branchesCapacity = 0;
        while (branchAmountStep > 1)
        {
            branchAmountStep =
                (branchAmountStep + Branch.MaxIndices - 1) / Branch.MaxIndices;

            branchesCapacity += branchAmountStep;
        }

        Span<Branch> branchSpan = stackalloc Branch[branchesCapacity]; // <-- add one here for the root branch at the very end. 

        ConstructLeafBranches(ref branchSpan, out int branchesCreated);

        int offset = 0;
        while(branchesCreated > 1)
        {
            ConstructBranches(ref branchSpan, offset, branchesCreated, out offset, out branchesCreated);
        }

        branches.AddRange(branchSpan);

        entries.Clear();
    }

    /// <summary>
    /// Contstructs leaves that hold all registered entry data.
    /// </summary>
    /// <remarks>
    /// This will always make the leaves list an even-number in length, even if the entries dataset is odd.
    /// </remarks>
    private void ConstructLeaves()
    {
        Span<Entry> entrySpan = CollectionsMarshal.AsSpan(entries);
        
        Span<float> distances   = stackalloc float[Leaf.MaxEntries];
        Span<int> entryIndices  = stackalloc int[Leaf.MaxEntries];
        Span<int> indices       = stackalloc int[Leaf.MaxEntries]; // gen index.
        Span<int> generations   = stackalloc int[Leaf.MaxEntries];
        Span<byte> flags        = stackalloc byte[Leaf.MaxEntries];
        Span<bool> leafed       = stackalloc bool[entrySpan.Length];
        int entriesCount        = 0;

        for(int i = 0; i < entrySpan.Length; i++)
        {
            ref Entry currentEntry = ref entrySpan[i];

            // move onto the next entry if this one has already been leafed.
            if (leafed[i])
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
                if (leafed[j])
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
                        // out, the swaped entry is used in later loop iterations.
                        // this ensures that any swapped values cascade up the stack,
                        // so that if the swapped entry is still one of the closest entries, 
                        // its not lost.

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
            leafed[i] = true;

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
                leafed[entryIndices[j]] = true;
                leafAABB = new AABB(leafAABB, nextEntry.AABB);
            }

            // add the new leaf to the leaves list.
            leaves.Add(new Leaf(indices, generations, flags, leafAABB, entriesCount));
        }

        // add a dummy leaf to even the numbers and keep the tree balanced.
        if(leaves.Count % 2 != 0)
        {
            // make sure to use the first leaf AABB as it is guaranteed to be the most stable. 
            leaves.Add(new Leaf([-1,-1,-1,-1], [-1,-1,-1,-1], [0,0,0,0], leaves[0].AABB, 0));
        }

    }

    /// <summary>
    /// Constructs the initial branches for the tree.
    /// </summary>
    /// <remarks>
    /// This should called after constructing the leaves and before constructing the branches of the tree.
    /// This also assumes that the leaves list is of an even-length and not odd.
    /// </remarks>
    /// <param name="branchSpan">The branch span to mutate and add the branches to.</param>
    /// <param name="branchesCreated">The amount of branches that were created.</param>
    private void ConstructLeafBranches(ref Span<Branch> branchSpan, out int branchesCreated)
    {
        Span<Leaf> leafSpan = CollectionsMarshal.AsSpan(leaves);

        Span<float> distances = stackalloc float[Branch.MaxIndices];
        Span<int> childIndices = stackalloc int[Branch.MaxIndices];
        Span<bool> branched = stackalloc bool[leafSpan.Length];
        
        int branchIndex = 0;

        // loop through all the leaves.
        for(int currentIndex = 0; currentIndex < leafSpan.Length - 1; currentIndex++)
        {
            // continue if the leaf has already been branched.
            if(branched[currentIndex])
            {
                continue;
            }

            ref Leaf current = ref leafSpan[currentIndex];
            Vector2 currentCentroid = current.AABB.GetCentroid();

            // set the first entry to the current leaf.
            childIndices[0] = currentIndex;
            distances[0] = 0;

            // reset the previous iterations branch data.
            for(int i = 1; i < Branch.MaxIndices; i++)
            {
                distances[i] = float.MaxValue;
                childIndices[i] = -1;
            }

            // find the sibling leaves that are closest to the current leaf.
            for(int otherIndex = currentIndex + 1; otherIndex < leafSpan.Length; otherIndex++)
            {
                // continue if the leaf has already been branched.
                if(branched[otherIndex])
                {
                    continue;
                }                

                ref Leaf other = ref leafSpan[otherIndex];
                float distance = currentCentroid.DistanceSquared(other.AABB.GetCentroid());
                int childIndex = otherIndex;

                for(int i = 1; i < Branch.MaxIndices; i++)
                {
                    // create temp values of the data that is swapped
                    // out, the swaped entry is used in later loop iterations.
                    // this ensures that any swapped values cascade up the stack,
                    // so that if the swapped entry is still one of the closest entries, 
                    // its not lost.

                    float swappedDistance   = distances[i];
                    int swappedIndex        = childIndices[i];

                    distances[i]    = distance;
                    childIndices[i] = childIndex;

                    distance    = swappedDistance;
                    childIndex  = swappedIndex;
                }
            }
            

            int childIndicesCount = 0;
            AABB branchAABB = current.AABB;
            for(int i = 0; i < Branch.MaxIndices; i++)
            {
                // there is no longer any valid child indices to operate on.
                if(childIndices[i] == -1)
                {
                    break;
                }
                
                // get the child to be assigned a parent branch.
                int childIndex = childIndices[i];
                ref Leaf child = ref leafSpan[childIndex];

                // make sure its not considered for any future parent branches.
                branched[childIndex] = true;

                // cummulate its AABB into the parent branch AABB.
                branchAABB = new AABB(branchAABB, child.AABB);

                childIndicesCount++;
            }

            // add the new branch with the sibling leaves.
            branchSpan[branchIndex] = new Branch(
                branchAABB,
                childIndices,
                childIndicesCount,
                ChildrenType.Leaf
            );

            // increment for the next branch.
            branchIndex++;
        }

        branchesCreated = branchIndex;
    }

    /// <summary>
    /// Constructs branches over a set of child branches.
    /// </summary>
    /// <remarks>
    /// Branch construction should be done after leaf branches have been constructed.
    /// </remarks>
    /// <param name="branchSpan">The branch span with the child branches and where the new parent branches will be written to.</param>
    /// <param name="childBranchesLength">The amount of child branches to create parents for.</param>
    /// <param name="childrenOffset">the offset in the branch span where the children data starts.</param>
    /// <param name="parentBranchesCreated">The amount of parent branches that where created.</param>
    private void ConstructBranches(ref Span<Branch> branchSpan, int offset, int branchesToParentLength, out int newOffset, out int branchesCreated)
    {
        Span<float> distances = stackalloc float[Branch.MaxIndices];
        Span<int> childIndices = stackalloc int[Branch.MaxIndices];
        Span<bool> branched = stackalloc bool[branchesToParentLength];

        int branchIndex = 0;
        int finalChildBranchIndex = offset + branchesToParentLength;

        // loop through all the branches to parent
        for(int currentIndex = offset; currentIndex < finalChildBranchIndex; currentIndex++)
        {
            // continue if the the branch has already panrented to another branched.
            if(branched[currentIndex - offset])
            {
                continue;
            }

            ref Branch current = ref branchSpan[currentIndex];
            Vector2 currentCentroid = current.AABB.GetCentroid();

            // set the first entry to the current branch.
            childIndices[0] = currentIndex;
            distances[0] = 0;

            // reset the preious iterations branch data.
            for(int i = 1; i < Branch.MaxIndices; i++)
            {
                distances[i] = float.MaxValue;
                childIndices[i] = -1;
            }

            // find the sibling branches that are closest to the current branch.
            for(int otherIndex = currentIndex + 1; otherIndex < finalChildBranchIndex; otherIndex++)
            {
                // continue if the branch has already been branched.
                if (branched[otherIndex - offset])
                {
                    continue;
                }

                ref Branch other = ref branchSpan[otherIndex];
                float distance = currentCentroid.DistanceSquared(other.AABB.GetCentroid());
                int childIndex = otherIndex;

                for(int i = 1; i < Branch.MaxIndices; i++)
                {
                    // create temp values of the data that is swapped
                    // out, the swaped entry is used in later loop iterations.
                    // this ensures that any swapped values cascade up the stack,
                    // so that if the swapped entry is still one of the closest entries, 
                    // its not lost.

                    float swappedDistance   = distances[i];
                    int swappedIndex        = childIndices[i];

                    distances[i]    = distance;
                    childIndices[i] = childIndex;

                    distance    = swappedDistance;
                    childIndex  = swappedIndex;  
                }
            } 

            int childIndicesCount = 0;
            AABB branchAABB = current.AABB;
            for(int i = 0; i < Branch.MaxIndices; i++)
            {
                // there is no longer any valid child indices to operate on.
                if(childIndices[i] == -1)
                {
                    break;
                }
                
                // get the child to be assigned a parent branch.
                int childIndex = childIndices[i];
                ref Branch child = ref branchSpan[childIndex];

                // make sure its not considered for any future parent branches.
                branched[childIndex-offset] = true;

                // cummulate its AABB into the parent branch AABB.
                branchAABB = new AABB(branchAABB, child.AABB);

                childIndicesCount++;
            }

            // add the new branch to the end of the span with the other newly created branches in this loop.
            branchSpan[branchIndex+finalChildBranchIndex] = new Branch(
                branchAABB,
                childIndices,
                childIndicesCount,
                ChildrenType.Branch
            );

            // increment for the next branch.
            branchIndex++;
        }

        branchesCreated = branchIndex;
        newOffset = finalChildBranchIndex;
    }


    public ReadOnlySpan<QueryResult> QuerySlow(AABB aabb)
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

    public ReadOnlySpan<QueryResult> QueryFast(AABB aabb)
    {
        queryResults.Clear();

        ReadOnlySpan<Branch> branchSpan = CollectionsMarshal.AsSpan(branches);
        ReadOnlySpan<Leaf> leafSpan = CollectionsMarshal.AsSpan(leaves);
        Span<GenIndex> genIndices = stackalloc GenIndex[Leaf.MaxEntries];
        // start the query branch loop at the root branch.
        QueryBranch(ref genIndices, branchSpan, leafSpan, aabb, branches.Count - 1);

        ReadOnlySpan<QueryResult> span = CollectionsMarshal.AsSpan(queryResults);
        return span;
    }


    private void QueryBranch(
        ref Span<GenIndex> genIndices, 
        in ReadOnlySpan<Branch> branchSpan, 
        in ReadOnlySpan<Leaf> leafSpan, 
        in AABB aabb, 
        int branchIndex)
    {
        
        // short-circuit if the query aabb doesnt intersect with the branch aabb.
        if(AABB.Intersect(branchSpan[branchIndex].AABB, aabb) == false)
        {
            return;
        }

        ReadOnlySpan<int> childBranchIndices = branchSpan[branchIndex].GetChildIndicesAsReadOnlySpan();
        switch (branchSpan[branchIndex].ChildrenType)
        {
            case ChildrenType.Branch:
                for(int i = 0; i < childBranchIndices.Length; i++)
                {
                    QueryBranch(ref genIndices, branchSpan, leafSpan, aabb, childBranchIndices[i]);
                }
            break;
            case ChildrenType.Leaf:
                for(int i = 0; i < childBranchIndices.Length; i++)
                {
                    leafSpan[childBranchIndices[i]].GetGenIndices(ref genIndices, out int genIndicesCount);
                    ReadOnlySpan<byte> flags = leafSpan[childBranchIndices[i]].GetFlags();
                    for(int j = 0; j < genIndicesCount; j++)
                    {
                        queryResults.Add(new QueryResult(genIndices[j], flags[j]));
                    }
                }
            break;
        }

    }
}