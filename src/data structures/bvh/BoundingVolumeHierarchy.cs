using Howl.Algorithms;
using Howl.Algorithms.Sorting;
using Howl.Math;
using Howl.Math.Shapes;
using Howl.Unmanaged.Collections;

namespace Howl.DataStructures.Bvh;

public struct BoundingVolumeHierarchy
{

    /// <summary>
    ///     The radix sort buffer used when sorting this leaf buffer.
    /// </summary>
    public RadixSortBuffer RadixSortBuffer;

    /// <summary>
    ///     The constructed branches from the inserted leaves.
    /// </summary>
    /// <remarks>
    ///     Use a <c>branchIndex</c> integer to access elements.
    /// </remarks>
    public Soa_Branch Branches;

    /// <summary>
    ///     The leaves to construct branches from.
    /// </summary>
    /// <remarks>
    ///     Use a <c>leafIndex</c> integer to get access elements.
    /// </remarks>
    public Soa_Leaf Leaves;

    /// <summary>
    ///     The morton codes for all leaf centroids.
    /// </summary>
    /// <remarks>
    ///     Use a <c>MortonLeafIds</c> entry to access elements.
    /// </remarks>
    public Array<uint> MortonCentroids;

    /// <summary>
    ///     Used as an index for an element in <c>MortonCentroids</c> to get its associated leaf data in <c>Leaves</c>.
    /// </summary>
    /// <remarks>
    ///     Elements in <c>MortonLeafIds</c> and <c>MortonCentroids</c> are associated via index.
    /// </remarks>
    public Array<int> MortonLeafIds;

    public bool IsInitialised;

    public static bool Initialise(ref BoundingVolumeHierarchy bvh, ref Memory.Arena arena, int length)
    {
        if (bvh.IsInitialised)
        {
            Debug.Panic("Already Initialised.");
            return false;
        }

        // mapping each spatial pair onto one another (without duplicates) gives a length * 2 possible spatial pairs.
        int branchesLength = length*2;

        Soa_Leaf.Initialise(ref bvh.Leaves, ref arena, length);
        Soa_Branch.Initialise(ref bvh.Branches, ref arena, branchesLength);
        Array.Initialise(ref bvh.MortonCentroids, ref arena, length);
        Array.Initialise(ref bvh.MortonLeafIds, ref arena, length);
        RadixSortBuffer.Initialise(ref bvh.RadixSortBuffer, ref arena, length);

        bvh.IsInitialised = true;
        return true;
    }

    /// <summary>
    /// Sets the count of the bounding volume hierarchy's internal arrays to zero.
    /// </summary>
    /// <param name="bvh">the bvh to clear.</param>
    public static void Clear(ref BoundingVolumeHierarchy bvh)
    {
        Soa_Leaf.ResetCount(ref bvh.Leaves);
        Soa_Branch.ResetCount(ref bvh.Branches);
    }




    /*******************
    
        Tree Construction.
    
    ********************/

    /// <summary>
    /// Constructs a tree of branches from the leaves store in a bvh instance.
    /// </summary>
    /// <param name="bvh">the bvh instance.</param>
    public static void ConstructTree(ref BoundingVolumeHierarchy bvh)
    {
        Soa_Branch.ResetCount(ref bvh.Branches);
        
        // get the spatial data for morton code calculations.
        float minX = float.MaxValue;
        float minY = float.MaxValue;
        float maxX = float.MinValue;
        float maxY = float.MinValue;
        for (int i = 0; i < bvh.Leaves.AppendCount; i++) {
            float cx = bvh.Leaves.Centroids.X[i];
            float cy = bvh.Leaves.Centroids.Y[i];
            if (cx < minX) minX = cx;
            if (cx > maxX) maxX = cx;
            if (cy < minY) minY = cy;
            if (cy > maxY) maxY = cy;
        }
        float rangeX = Math.Math.Abs(maxX - minX);
        float rangeY = Math.Math.Abs(maxY - minY);

        // get the morton code for sorting each of the centroids.
        float scaleX = 0;
        float scaleY = 0;
        MortonCode.CalculateScaleFactor(rangeX, rangeY, ref scaleX, ref scaleY);
        for(int i = 0; i < bvh.Leaves.AppendCount; i++)
        {
            bvh.MortonCentroids[i] = MortonCode.CalculateMortonCode(bvh.Leaves.Centroids.X[i], bvh.Leaves.Centroids.Y[i], minX, minY, scaleX, scaleY);
        }

        // reset leaf indices.
        for(int i = 0; i < bvh.Leaves.AppendCount; i++)
        {
            bvh.MortonLeafIds[i] = i;
        }
        
        RadixSort.IndexedAscend(Array.AsSpan(bvh.MortonCentroids), Array.AsSpan(bvh.MortonLeafIds), 
            ref bvh.RadixSortBuffer, 0, bvh.Leaves.AppendCount
        );
    
        int branchCount = 0;
        int parentIndex = -1; // this will have to change to zero when we start enforcing Nils.
        float aabbMinX = 0;
        float aabbMinY = 0;
        float aabbMaxX = 0;
        float aabbMaxY = 0;

        ConstructBranches(ref bvh.Branches, bvh.MortonLeafIds, bvh.Leaves.Aabbs.MinX, bvh.Leaves.Aabbs.MinY, bvh.Leaves.Aabbs.MaxX, bvh.Leaves.Aabbs.MaxY, 
            ref bvh.Leaves.BranchIndices, 0, bvh.Leaves.AppendCount, parentIndex, ref branchCount, ref aabbMinX, ref aabbMinY, ref aabbMaxX, ref aabbMaxY
        );

        // we set the branch count manually as the branches are inserted into the soa manually
        // without using the Append() function; this is okay as branch insertion in Construct branches
        // inserts branches in a 'subtree size' relative order for each branch; meaning, at the end of the 
        // construction of all branches, the data is contiguous (no holes in the array entries).
        bvh.Branches.AppendCount = branchCount;
    }

    /// <summary>
    /// A recursive function that constructs branches from a given data set of leaves.
    /// </summary>
    /// <remarks>
    /// Note: this is a destructive process on <paramref name="branches"/>, entries within the soa instance will be overwritten.
    /// </remarks>
    /// <param name="branches">output soa instance for writing generated branches to.</param>
    /// <param name="leafIndices">A span of leaf indices sorted so that neighbouring entries are neighbouring leaves (within close proximity) in world-space.</param>
    /// <param name="leavesMinX">the x-component of all leaves minimum vertices.</param>
    /// <param name="leavesMinY">the y-component of all leaves minimum vertices.</param>
    /// <param name="leavesMaxX">the x-component of all leaves maximum vertices.</param>
    /// <param name="leavesMaxY">the y-component of all leaves maximum vertices.</param>
    /// <param name="leafBranchIndices">a span containing the branch indices that all leaves are parented to.</param>
    /// <param name="start">the index to start at when processing the leaf indices.</param>
    /// <param name="length">the total amount of leaf indices to process after <c><paramref name="start"/></c></param>
    /// <param name="parentIndex">the index of the branch that this newly constructed branch will be parented to.</param>
    /// <param name="writeIndex">the index of the most recently written entry in <c><paramref name="branches"/></c>.</param>
    /// <param name="aabbMinX">the x-component of the minimum vertex of the currently constructed branch.</param>
    /// <param name="aabbMinY">the y-component of the minimum vertex of the currently constructed branch.</param>
    /// <param name="aabbMaxX">the x-component of the maximum vertex of the currently constructed branch.</param>
    /// <param name="aabbMaxY">the y-component of the maximum vertex of the currently constructed branch.</param>
    public static void ConstructBranches(ref Soa_Branch branches, Array<int> leafIndices, 
        Array<float> leavesMinX, Array<float> leavesMinY, Array<float> leavesMaxX, Array<float> leavesMaxY, ref Array<int> leafBranchIndices,
        int start, int length, int parentIndex, ref int writeIndex, ref float aabbMinX, ref float aabbMinY, ref float aabbMaxX, ref float aabbMaxY
    )
    {
        // reserve space.
        int branchIndex = writeIndex++;

        // == leaf ==
        if (length <= 2)
        {
            // build leaf aabb.
            int leftLeafIndex = leafIndices[start];
            int rightLeafIndex = 0;
            int leafCount;
            aabbMinX = leavesMinX[leftLeafIndex];
            aabbMinY = leavesMinY[leftLeafIndex];
            aabbMaxX = leavesMaxX[leftLeafIndex];
            aabbMaxY = leavesMaxY[leftLeafIndex];
            leafBranchIndices[leftLeafIndex] = branchIndex;

            if(length == 2)
            {
                // union the sibling leaf if there is one.
                rightLeafIndex = leafIndices[start + 1];
                Aabb.Union(aabbMinX, aabbMinY, aabbMaxX, aabbMaxY,
                    leavesMinX[rightLeafIndex], leavesMinY[rightLeafIndex], leavesMaxX[rightLeafIndex], leavesMaxY[rightLeafIndex],
                    out aabbMinX, out aabbMinY, out aabbMaxX, out aabbMaxY
                );
                 
                // set the leaf branch.
                leafBranchIndices[rightLeafIndex] = branchIndex;

                leafCount = 2;
            }
            else
            {
                leafCount = 1;
            }

            // insert the leaf.
            // note: subtree size for leaves is always one as subtree size is inclusive of then entry; and a leaf is the final in a branch chain.
            Soa_Branch.Insert(ref branches, branchIndex, aabbMinX, aabbMinY, aabbMaxX, aabbMaxY, leftLeafIndex, rightLeafIndex, 1, leafCount, parentIndex);
        }
        else
        {
            // == internal branch. ==
            
            // split at the mid point.
            int mid = length/2;
            
            int leftStart = start;
            int leftLength = mid;
            float leftMinX = 0;
            float leftMinY = 0;
            float leftMaxX = 0;
            float leftMaxY = 0;

            int rightStart = start + mid;
            int rightLength = length - mid;
            float rightMinX = 0;
            float rightMinY = 0;
            float rightMaxX = 0;
            float rightMaxY = 0;

            // == recurse (children are written contiguously after parent). ==

            parentIndex++;

            // left branch.
            ConstructBranches(ref branches, leafIndices, leavesMinX, leavesMinY, leavesMaxX, leavesMaxY, ref leafBranchIndices,
                leftStart, leftLength, parentIndex, ref writeIndex, ref leftMinX, ref leftMinY, ref leftMaxX, ref leftMaxY
            );

            // right branch.
            ConstructBranches(ref branches, leafIndices, leavesMinX, leavesMinY, leavesMaxX, leavesMaxY, ref leafBranchIndices,
                rightStart, rightLength, parentIndex, ref writeIndex, ref rightMinX, ref rightMinY, ref rightMaxX, ref rightMaxY
            );

            // get the aabb of both branches.
            Aabb.Union(leftMinX, leftMinY, leftMaxX, leftMaxY,
                rightMinX, rightMinY, rightMaxX, rightMaxY,
                out aabbMinX, out aabbMinY, out aabbMaxX, out aabbMaxY 
            );

            // set the sub tree.
            // note: subtree = everything written since this node.
            int subtreeSize = writeIndex - branchIndex;

            // set the branch.
            Soa_Branch.Insert(ref branches, branchIndex, aabbMinX, aabbMinY, aabbMaxX, aabbMaxY, 0, 0, subtreeSize, 0, parentIndex);
        }

    }

    /// <summary>
    ///     Finds any leaves that overlap with eachother within a set of constructed branches and leaves.
    /// </summary>
    /// <remarks>
    ///     <para>Remarks:</para> 
    ///     <para>There are no duplicate elements in the output overlap data.</para>
    /// </remarks>
    /// <param name="branches">the constructed tree of branches to query.</param>
    /// <param name="leaves">the leaf data associated with the branches.</param>
    /// <param name="overlaps">output for the overlap data.</param>
    public static void FindOverlaps(Soa_Branch branches, Soa_Leaf leaves, Soa_Overlap overlaps)
    {
        // clear any garbage data.
        Soa_Overlap.ClearAppendCount(overlaps);

        // hoisting of inavriance.
        System.Span<float> leafMinX = Array.AsSpan(leaves.Aabbs.MinX);
        System.Span<float> leafMinY = Array.AsSpan(leaves.Aabbs.MinY);
        System.Span<float> leafMaxX = Array.AsSpan(leaves.Aabbs.MaxX);
        System.Span<float> leafMaxY = Array.AsSpan(leaves.Aabbs.MaxY);
        System.Span<float> branchMinX = Array.AsSpan(branches.Aabbs.MinX);
        System.Span<float> branchMinY = Array.AsSpan(branches.Aabbs.MinY);
        System.Span<float> branchMaxX = Array.AsSpan(branches.Aabbs.MaxX);
        System.Span<float> branchMaxY = Array.AsSpan(branches.Aabbs.MaxY);
        System.Span<int> branchSubtreeSizes = Array.AsSpan(branches.SubtreeSizes);
        System.Span<int> branchLeafCounts = Array.AsSpan(branches.LeafCounts);
        System.Span<int> rightLeafIndices = Array.AsSpan(branches.RightLeafIndices);
        System.Span<int> leftLeafIndices = Array.AsSpan(branches.LeftLeafIndices);
        float minX;
        float minY; 
        float maxX;
        float maxY;
        int otherLeaf;

        for(int ownerLeaf = 0; ownerLeaf < leaves.AppendCount; ownerLeaf++)
        {
            minX = leafMinX[ownerLeaf];
            minY = leafMinY[ownerLeaf];
            maxX = leafMaxX[ownerLeaf];
            maxY = leafMaxY[ownerLeaf];

            int otherBranch = 0;
            while(otherBranch < branches.AppendCount)
            {
                if(!Aabb.Intersect(minX, branchMinX[otherBranch], minY, branchMinY[otherBranch], maxX, branchMaxX[otherBranch], maxY, branchMaxY[otherBranch]))
                {
                    // skip the entire subtree.
                    otherBranch+= branchSubtreeSizes[otherBranch];
                    continue;
                }

                int leafCount = branchLeafCounts[otherBranch];

                switch (leafCount)
                {
                    case 1:
                        // left leaf index should always be set to a leaf index for branches with leaf(s) attatched; it is the default leaf to set first.
                        otherLeaf = leftLeafIndices[otherBranch];
                        if(ownerLeaf < otherLeaf && Soa_Leaf.Intersects(leaves, otherLeaf, minX, minY, maxX, maxY)){
                            Soa_Overlap.Append(overlaps, ownerLeaf, otherLeaf);
                        }
                        break;
                    case 2:
                        otherLeaf = leftLeafIndices[otherBranch];
                        // ensure that there are no duplicates.
                        if(ownerLeaf < otherLeaf && Soa_Leaf.Intersects(leaves, otherLeaf, minX, minY, maxX, maxY)){
                            Soa_Overlap.Append(overlaps, ownerLeaf, otherLeaf);
                        }
                        otherLeaf = rightLeafIndices[otherBranch];
                        // ensure that there are no duplicates.
                        if(ownerLeaf < otherLeaf && Soa_Leaf.Intersects(leaves, otherLeaf, minX, minY, maxX, maxY)){
                            Soa_Overlap.Append(overlaps, ownerLeaf, otherLeaf);
                        }
                        break;
                    case 0:
                        // do nothing..., just go to next branch in the tree..
                        break;
                    default:
                        System.Diagnostics.Debug.Assert(false);
                        break;
                }

                otherBranch++;
            }
        }
    }

    /// <summary>
    ///     Finds any leaves that overlap with eachother within a set of constructed branches and leaves.
    /// </summary>
    /// <remarks>
    ///     <para>Remarks:</para> 
    ///     <para>There are no duplicate elements in the output overlap data.</para>
    /// </remarks>
    /// <param name="branches">the constructed tree of branches to query.</param>
    /// <param name="leaves">the leaf data associated with the branches.</param>
    /// <param name="overlaps">output for the overlap data.</param>
    public static void FindOverlaps(Soa_Branch branches, Soa_Leaf leaves, CategorisedLeafOverlaps overlaps)
    {
        // clear any garbage data.
        CategorisedLeafOverlaps.ClearCounts(ref overlaps);

        // hoisting of inavriance.
        System.Span<float> leafMinX = Array.AsSpan(leaves.Aabbs.MinX);
        System.Span<float> leafMinY = Array.AsSpan(leaves.Aabbs.MinY);
        System.Span<float> leafMaxX = Array.AsSpan(leaves.Aabbs.MaxX);
        System.Span<float> leafMaxY = Array.AsSpan(leaves.Aabbs.MaxY);
        System.Span<int> leafCategories = Array.AsSpan(leaves.Categories);
        System.Span<float> branchMinX = Array.AsSpan(branches.Aabbs.MinX);
        System.Span<float> branchMinY = Array.AsSpan(branches.Aabbs.MinY);
        System.Span<float> branchMaxX = Array.AsSpan(branches.Aabbs.MaxX);
        System.Span<float> branchMaxY = Array.AsSpan(branches.Aabbs.MaxY);
        System.Span<int> branchSubtreeSizes = Array.AsSpan(branches.SubtreeSizes);
        System.Span<int> rightLeafIndices = Array.AsSpan(branches.RightLeafIndices);
        System.Span<int> branchLeafCounts = Array.AsSpan(branches.LeafCounts);
        System.Span<int> leftLeafIndices = Array.AsSpan(branches.LeftLeafIndices);
        float minX;
        float minY; 
        float maxX;
        float maxY;
        int otherLeaf;

        for(int ownerLeaf = 0; ownerLeaf < leaves.AppendCount; ownerLeaf++)
        {
            minX = leafMinX[ownerLeaf];
            minY = leafMinY[ownerLeaf];
            maxX = leafMaxX[ownerLeaf];
            maxY = leafMaxY[ownerLeaf];

            int otherBranch = 0;
            while(otherBranch < branches.AppendCount)
            {
                if(!Aabb.Intersect(minX, branchMinX[otherBranch], minY, branchMinY[otherBranch], maxX, branchMaxX[otherBranch], maxY, branchMaxY[otherBranch]))
                {
                    // skip the entire subtree.
                    otherBranch+= branchSubtreeSizes[otherBranch];
                    continue;
                }

                int leafCount = branchLeafCounts[otherBranch];

                switch (leafCount)
                {
                    case 1:
                        // left leaf index should always be set to a leaf index for branches with leaf(s) attatched; it is the default leaf to set first.
                        otherLeaf = leftLeafIndices[otherBranch];
                        if(ownerLeaf < otherLeaf && Soa_Leaf.Intersects(leaves, otherLeaf, minX, minY, maxX, maxY)){
                            // Soa_Overlap.Append(overlaps, ownerLeaf, otherLeaf);
                            CategorisedLeafOverlaps.Append(overlaps, ownerLeaf, otherLeaf, leafCategories[ownerLeaf], leafCategories[otherLeaf]);
                        }
                        break;
                    case 2:
                        otherLeaf = leftLeafIndices[otherBranch];
                        // ensure that there are no duplicates.
                        if(ownerLeaf < otherLeaf && Soa_Leaf.Intersects(leaves, otherLeaf, minX, minY, maxX, maxY)){
                            CategorisedLeafOverlaps.Append(overlaps, ownerLeaf, otherLeaf, leafCategories[ownerLeaf], leafCategories[otherLeaf]);
                        }
                        otherLeaf = rightLeafIndices[otherBranch];
                        // ensure that there are no duplicates.
                        if(ownerLeaf < otherLeaf && Soa_Leaf.Intersects(leaves, otherLeaf, minX, minY, maxX, maxY)){
                            CategorisedLeafOverlaps.Append(overlaps, ownerLeaf, otherLeaf, leafCategories[ownerLeaf], leafCategories[otherLeaf]);
                        }
                        break;
                    case 0:
                        // do nothing..., just go to next branch in the tree..
                        break;
                    default:
                        System.Diagnostics.Debug.Assert(false);
                        break;
                }

                otherBranch++;
            }
        }
    }




    /*******************
    
        Area Querying.
    
    ********************/




    /// <summary>
    ///     Queries a constructed tree of branches for any leaves that overlap within a given area.
    /// </summary>
    /// <remarks>
    ///     <paramref name="overlapsOutput"/> is overwritten from index 0 onwards for the amount of overlaps appended; it is a destructive process.
    /// </remarks>
    /// <param name="branches">the constructed tree of branches to query.</param>
    /// <param name="leaves">the leaf data associated with the branches.</param>
    /// <param name="overlapsOutput">output for the indices of the overlapping leaves found.</param>
    /// <param name="appendedOverlapsOutput">output for the amount of indices that were appended to the overlaps array.</param>
    /// <param name="minX">the x-component of the query area minimum vertex.</param>
    /// <param name="minY">the y-component of the query area minimum vertex.</param>
    /// <param name="maxX">the x-component of the query area maximum vertex.</param>
    /// <param name="maxY">the y-component of the query area maximum vertex.</param>
    public static void AreaQuery(Soa_Branch branches, Soa_Leaf leaves, System.Span<int> overlapsOutput, ref int appendedOverlapsOutput, 
        float minX, float minY, float maxX, float maxY
    )
    {
        // reset to remove any garbage data.
        appendedOverlapsOutput = 0;

        // hoisting of invariance.
        System.Span<float> branchMinX = Array.AsSpan(branches.Aabbs.MinX);
        System.Span<float> branchMinY = Array.AsSpan(branches.Aabbs.MinY);
        System.Span<float> branchMaxX = Array.AsSpan(branches.Aabbs.MaxX);
        System.Span<float> branchMaxY = Array.AsSpan(branches.Aabbs.MaxY);
        System.Span<int> branchSubtreeSizes = Array.AsSpan(branches.SubtreeSizes);
        System.Span<int> leftLeafIndices = Array.AsSpan(branches.LeftLeafIndices);
        System.Span<int> branchLeafCounts = Array.AsSpan(branches.LeafCounts);
        System.Span<int> rightLeafIndices = Array.AsSpan(branches.RightLeafIndices);
                
        int otherLeaf;

        int otherBranch = 0;
        while(otherBranch < branches.AppendCount)
        {
            if(!Aabb.Intersect(minX, branchMinX[otherBranch], minY, branchMinY[otherBranch], maxX, branchMaxX[otherBranch], maxY, branchMaxY[otherBranch]))
            {
                // skip the entire subtree.
                otherBranch+= branchSubtreeSizes[otherBranch];
                continue;
            }

            int leafCount = branchLeafCounts[otherBranch];

            switch (leafCount)
            {
                case 1:
                    // left leaf index should always be set to a leaf index for branches with leaf(s) attatched; it is the default leaf to set first.
                    otherLeaf = leftLeafIndices[otherBranch];
                    if(Soa_Leaf.Intersects(leaves, otherLeaf, minX, minY, maxX, maxY)){
                        overlapsOutput[appendedOverlapsOutput] = otherLeaf;
                        appendedOverlapsOutput+=1;
                    }
                    // incemrent appended overlaps
                    break;
                case 2:
                    // add the left leaf.
                    otherLeaf = leftLeafIndices[otherBranch];
                    if(Soa_Leaf.Intersects(leaves, otherLeaf, minX, minY, maxX, maxY)){
                        overlapsOutput[appendedOverlapsOutput] = otherLeaf;
                        appendedOverlapsOutput+=1;
                    }
                    // add the right leaf.
                    otherLeaf = rightLeafIndices[otherBranch];
                    if(Soa_Leaf.Intersects(leaves, otherLeaf, minX, minY, maxX, maxY)){
                        overlapsOutput[appendedOverlapsOutput] = otherLeaf;
                        appendedOverlapsOutput+=1;
                    }
                    break;
                case 0:
                    // do nothing..., just go to next branch in the tree..
                    break;
                default:
                    System.Diagnostics.Debug.Assert(false);
                    break;
            }

            otherBranch++;
        }
    }

    /// <summary>
    ///     Queries a constructed tree of branches for any leaves that overlap within a given area.
    /// </summary>
    /// <remarks>
    ///     <paramref name="overlapsOutput"/> is overwritten from index 0 onwards for the amount of overlaps appended; it is a destructive process.
    /// </remarks>
    /// <param name="bvh">the bounding-volume-hierarchy instance.</param>
    /// <param name="overlapsOutput">output for the indices of the overlapping leaves found.</param>
    /// <param name="appendedOverlapsOutput">output for the amount of indices that were appended to the overlaps array.</param>
    /// <param name="minX">the x-component of the query area minimum vertex.</param>
    /// <param name="minY">the y-component of the query area minimum vertex.</param>
    /// <param name="maxX">the x-component of the query area maximum vertex.</param>
    /// <param name="maxY">the y-component of the query area maximum vertex.</param>
    public static void AreaQuery(BoundingVolumeHierarchy bvh, System.Span<int> overlapsOutput, ref int appendedOverlapsOutput, float minX, float minY, float maxX, float maxY)
    {
        AreaQuery(bvh.Branches, bvh.Leaves, overlapsOutput, ref appendedOverlapsOutput, minX, minY, maxX, maxY);
    }




    /*******************
    
        Raycasting.
    
    ********************/



    /// <summary>
    ///     Queries a constructed tree of branches for any leaves that overlap with a raycast.
    /// </summary>
    /// <remarks>
    ///     <paramref name="overlapsOutput"/> is overwritten from index 0 onwards for the amount of overlaps appended; it is a destructive process.
    /// </remarks>
    /// <param name="branches">the constructed tree of branches to query.</param>
    /// <param name="leaves">the leaf data associated with the branches.</param>
    /// <param name="overlapsOutput">output for the indices of the overlapping leaves found.</param>
    /// <param name="appendedOverlapsOutput">output for the amount of indices that were appended to the overlaps array.</param>
    /// <param name="startX">the x-component of the query rqycast starting vertex.</param>
    /// <param name="startY">the y-component of the query rqycast starting vertex.</param>
    /// <param name="endX">the x-component of the query rqycast ending vertex.</param>
    /// <param name="endY">the y-component of the query rqycast ending vertex.</param>
    public static void RaycastQuery(Soa_Branch branches, Soa_Leaf leaves, System.Span<int> overlapsOutput, ref int appendedOverlapsOutput, float startX, float startY, float endX, float endY)
    {
        // reset to remove any garbage data.
        appendedOverlapsOutput = 0;

        // hoisting of invariance.
        System.Span<float> branchMinX = Array.AsSpan(branches.Aabbs.MinX);
        System.Span<float> branchMinY = Array.AsSpan(branches.Aabbs.MinY);
        System.Span<float> branchMaxX = Array.AsSpan(branches.Aabbs.MaxX);
        System.Span<float> branchMaxY = Array.AsSpan(branches.Aabbs.MaxY);
        System.Span<int> branchSubtreeSizes = Array.AsSpan(branches.SubtreeSizes);
        System.Span<int> leftLeafIndices = Array.AsSpan(branches.LeftLeafIndices);
        System.Span<int> branchLeafCounts = Array.AsSpan(branches.LeafCounts);
        System.Span<int> rightLeafIndices = Array.AsSpan(branches.RightLeafIndices);
                
        int otherLeaf;

        int otherBranch = 0;

        int i = 0;
        while (otherBranch < branches.AppendCount)
        {
            if (Aabb.LineIntersect(branchMinX[i], branchMinY[i], branchMaxX[i], branchMaxY[i], startX, startY, endX, endY) == false)
            {
                // skip entire subtree
                otherBranch += branchSubtreeSizes[i];
                continue;
            }

            int leafCount = branchLeafCounts[i];
            switch (leafCount)
            {
                case 1:
                    // left leaf index should always be set to a leaf index for branches with leaf(s) attatched; it is the default leaf to set first.
                    otherLeaf = leftLeafIndices[otherBranch];
                    if(Soa_Leaf.LineIntersects(leaves, otherLeaf, startX, startY, endX, endY)){
                        overlapsOutput[appendedOverlapsOutput] = otherLeaf;
                        appendedOverlapsOutput+=1;
                    }
                    break;
                case 2:
                    // add the left leaf.
                    otherLeaf = leftLeafIndices[otherBranch];
                    if(Soa_Leaf.LineIntersects(leaves, otherLeaf, startX, startY, endX, endY)){
                        overlapsOutput[appendedOverlapsOutput] = otherLeaf;
                        appendedOverlapsOutput+=1;
                    }
                    // add the right leaf.
                    otherLeaf = rightLeafIndices[otherBranch];
                    if(Soa_Leaf.LineIntersects(leaves, otherLeaf, startX, startY, endX, endY)){
                        overlapsOutput[appendedOverlapsOutput] = otherLeaf;
                        appendedOverlapsOutput+=1;
                    }
                    break;
                case 0:
                    // do nothing..., just go to next branch in the tree..
                    break;
                default:
                    System.Diagnostics.Debug.Assert(false);
                    break;
            }
            
            otherBranch++;
        }
    }

    /// <summary>
    ///     Queries a constructed tree of branches for any leaves that overlap with a raycast.
    /// </summary>
    /// <remarks>
    ///     <paramref name="overlapsOutput"/> is overwritten from index 0 onwards for the amount of overlaps appended; it is a destructive process.
    /// </remarks>
    /// <param name="bvh">the bounding-volume-hierarchy instance.</param>
    /// <param name="overlapsOutput">output for the indices of the overlapping leaves found.</param>
    /// <param name="appendedOverlapsOutput">output for the amount of indices that were appended to the overlaps array.</param>
    /// <param name="startX">the x-component of the query rqycast starting vertex.</param>
    /// <param name="startY">the y-component of the query rqycast starting vertex.</param>
    /// <param name="endX">the x-component of the query rqycast ending vertex.</param>
    /// <param name="endY">the y-component of the query rqycast ending vertex.</param>
    public static void RaycastQuery(BoundingVolumeHierarchy bvh, System.Span<int> overlapsOutput, ref int appendedOverlapsOutput, float startX, float startY, float endX, float endY)
    {
        RaycastQuery(bvh.Branches, bvh.Leaves, overlapsOutput, ref appendedOverlapsOutput, startX, startY, endX, endY);
    }




    /*******************
    
        Debug Drawing.
    
    ********************/




    public static void DrawBranches(HowlAppState app, BoundingVolumeHierarchy bvh, Howl.Graphics.Colour colour)
    {
        for(int i = 0; i < bvh.Branches.AppendCount; i++)
        {
            Renderer.DrawWireRect(
                app,
                new Rectangle(
                    new Vector2(bvh.Branches.Aabbs.MinX[i], bvh.Branches.Aabbs.MinY[i]), 
                    new Vector2(bvh.Branches.Aabbs.MaxX[i], bvh.Branches.Aabbs.MaxY[i])
                ), 
                colour,
                Graphics.DrawSpace.World
            );
        }

    }

    public static void DrawLeaves(HowlAppState app, BoundingVolumeHierarchy bvh, Howl.Graphics.Colour colour)
    {
        for(int i = 0; i < bvh.Leaves.AppendCount; i++)
        {
            Renderer.DrawWireRect(
                app,
                new Rectangle(
                    new Vector2(bvh.Leaves.Aabbs.MinX[i], bvh.Leaves.Aabbs.MinY[i]), 
                    new Vector2(bvh.Leaves.Aabbs.MaxX[i], bvh.Leaves.Aabbs.MaxY[i])
                ), 
                colour,
                Graphics.DrawSpace.World
            );
        }

    }
}