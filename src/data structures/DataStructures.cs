using System.Runtime.CompilerServices;
using Howl.Algorithms;
using Howl.Algorithms.Sorting;
using N_Howl.N_Graphics;
using N_Howl.N_Math;
using N_Howl.N_Collections;
using Memory = Howl.Memory;

namespace N_Howl.N_DataStructures;
public static class DataStructures{

/**##########################################################################################################################################
    div: CategorisedLeafOverlaps.
##########################################################################################################################################**/

/// <param name="categoryCount">the amount of categories the overlap data can be filtered into.</param>
/// <param name="maxOverlaps">the maximum amount of overlap data that this instance can hold.</param>
public static bool Init(
    ref CategorisedLeafOverlaps overlaps, ref Memory.Arena arena, int categoryCount, int maxOverlaps
){
    if (overlaps.IsInitialised){
        Howl.Debug.Panic("Already Initialised");
        return false;
    }

    overlaps.CategoriesTriangularSum = Math.CalculateTriangularSum(categoryCount);

    Collections.Init(ref overlaps.CategoryLengths, ref arena, categoryCount);
    Collections.Init(ref overlaps.SubCategoryStartIndices, ref arena, overlaps.CategoriesTriangularSum);
    Collections.Init(ref overlaps.SubCategoryCounts, ref arena, overlaps.CategoriesTriangularSum);
    Collections.Init(ref overlaps.OwnerLeafIndices, ref arena, maxOverlaps);
    Collections.Init(ref overlaps.OtherLeafIndices, ref arena, maxOverlaps);

    overlaps.IsInitialised = true;
    return true;
}

/// <summary>
///     Calculates the starting indices for each sub category to write to in the data array's. 
/// </summary>
/// <param name="state"></param>
public static void BuildChunks(
    CategorisedLeafOverlaps state
){
    Collections.BuildChunks(ref state.CategoryLengths, ref state.SubCategoryStartIndices, state.MaxOverlaps);        
}

/// <summary>
///     Appends an overlap to an instance.
/// </summary>
/// <param name="overlaps">the instance to append to.</param>
/// <param name="ownerLeafIndex">the index of the leaf that is the <c>owner</c> of the overlap.</param>
/// <param name="otherLeafIndex">the index of the leaf that is the <c>other</c> of the overlap.</param>
/// <param name="ownerCategory">the category of the <c>owner</c> leaf.</param>
/// <param name="otherCategory">the category of the <c>other</c> leaf.</param>
/// <returns>true, if the overlap was successfully appended; otherwise false.</returns>
public static bool Append(
    ref CategorisedLeafOverlaps overlaps, int ownerLeafIndex, int otherLeafIndex, 
    int ownerCategory, int otherCategory
){
    int writeIndex = 0;
    if(Collections.IncrementSubCategoryCount(ref overlaps.CategoryLengths, ref overlaps.SubCategoryCounts, 
        ref overlaps.SubCategoryStartIndices, overlaps.CategoriesTriangularSum, ownerCategory, otherCategory, ref writeIndex
    ))
    {
        // write the data to the index.
        overlaps.OwnerLeafIndices[writeIndex] = ownerLeafIndex;
        overlaps.OtherLeafIndices[writeIndex] = otherLeafIndex;
        return true;
    }
    else
    {
        return false;
    }
}

/// <summary>
///     Sets the count values in a overlaps instance to zero. 
/// </summary>
/// <param name="overlaps">the instance to clear.</param>
[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
public static void Clear(
    ref CategorisedLeafOverlaps overlaps
){
    for(int i = 0; i < overlaps.SubCategoryCounts.Length; i++){
        overlaps.SubCategoryCounts[i] = 0;
    }
}

/// <summary>
///     Gets the overlap info between two categories.
/// </summary>
/// <param name="overlaps"></param>
/// <param name="categoryA"></param>
/// <param name="categoryB"></param>
/// <returns>the data that overlaps between the two categories.</returns>
public static OverlapInfo GetOverlaps(
    CategorisedLeafOverlaps overlaps, int categoryA, int categoryB
){
    int elementIndex = Collections.GetCategorisedOverlapArrayElementIndex(categoryA, categoryB, overlaps.CategoriesTriangularSum);
    int startIndex = overlaps.SubCategoryStartIndices[elementIndex];
    int count = overlaps.SubCategoryCounts[elementIndex];
    System.Span<int> ownerIndices = Collections.AsSpan(overlaps.OwnerLeafIndices, startIndex, count);
    System.Span<int> otherIndices = Collections.AsSpan(overlaps.OtherLeafIndices, startIndex, count);
    OverlapInfo info = default;
    info.OwnerLeafIndices = ownerIndices;
    info.OtherLeafIndices = otherIndices;
    info.Length = count; 
    return info;
}

/**##########################################################################################################################################
    div: Soa_Leaf
##########################################################################################################################################**/

public static bool Init(
    ref Soa_Leaf soa, ref Memory.Arena arena, int length
){
    if (soa.IsInitialised){
        Howl.Debug.Panic("Already Initialised.");
        return false;
    }

    Math.Init(ref soa.Aabbs, ref arena, length);
    Math.Init(ref soa.Centroids, ref arena, length);
    Collections.Init(ref soa.BranchIndices, ref arena, length);
    Collections.Init(ref soa.Categories, ref arena, length);
    soa.Length = length;

    soa.IsInitialised = true;
    return true;
}

/// <summary>
///     Appends an entry into a soa at the soa instance's <c>AppendCount</c> index.
/// </summary>
/// <returns> the index the entry was appended to in the backing arrays.</returns>
[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
public static int Append(
    ref Soa_Leaf soa, float leafMinX, float leafMinY, float leafMaxX, float leafMaxY, float leafCentroidX, float leafCentroidY, int leafCategory
){
    Math.Append(ref soa.Aabbs, leafMinX, leafMinY, leafMaxX, leafMaxY);
    Math.Append(ref soa.Centroids, leafCentroidX, leafCentroidY);
    soa.Categories[soa.AppendCount] = leafCategory;
    soa.AppendCount++;
    return soa.AppendCount-1;
}

/// <summary>
///     Sets a soa instance's <c>AppendCount</c> to zero.
/// </summary>
[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
public static void Clear(
    ref Soa_Leaf soa
){
    Math.ResetCount(ref soa.Aabbs);
    Math.ResetCount(ref soa.Centroids);
    soa.AppendCount = 0;
}

/// <summary>
///     Checks whether or not a leaf overlaps within a given area.
/// </summary>
/// <returns>true, if the leaf intersects with the query area; otherwise false.</returns>
[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
public static bool LeafIntersectsAabb(
    Soa_Leaf leaves, int leafIndex, float queryAreaMinX, float queryAreaMinY, float queryAreaMaxX, float queryAreaMaxY
){
    // hoisting of invariance.
    Soa_Aabb aabbs = leaves.Aabbs;
    System.Span<float> aabbsMinX = Collections.AsSpan(aabbs.MinX);
    System.Span<float> aabbsMinY = Collections.AsSpan(aabbs.MinY);
    System.Span<float> aabbsMaxX = Collections.AsSpan(aabbs.MaxX);
    System.Span<float> aabbsMaxY = Collections.AsSpan(aabbs.MaxY);

    return Math.AabbsIntersect(
        aabbsMinX[leafIndex], queryAreaMinX, aabbsMinY[leafIndex], queryAreaMinY, 
        aabbsMaxX[leafIndex], queryAreaMaxX, aabbsMaxY[leafIndex], queryAreaMaxY
    );
}

/// <summary>
///     Checks whether or not a leaf overlaps with a given line segment.
/// </summary>
/// <returns>true, if the leaf intersects with the line segment; otherwise false.</returns>
public static bool LeafIntersectsLine(
    Soa_Leaf leaves, int leafIndex, float startX, float startY, float endX, float endY
){
    // hoisting of invariance.
    Soa_Aabb aabbs = leaves.Aabbs;
    System.Span<float> aabbsMinX = Collections.AsSpan(aabbs.MinX);
    System.Span<float> aabbsMinY = Collections.AsSpan(aabbs.MinY);
    System.Span<float> aabbsMaxX = Collections.AsSpan(aabbs.MaxX);
    System.Span<float> aabbsMaxY = Collections.AsSpan(aabbs.MaxY);

    return Math.AabbIntersectsLine(
        aabbsMinX[leafIndex], aabbsMinY[leafIndex], aabbsMaxX[leafIndex], aabbsMaxY[leafIndex], 
        startX, startY, endX, endY
    );        
}

/**##########################################################################################################################################
    div: Soa_Branch
##########################################################################################################################################**/

public static bool Init(
    ref Soa_Branch soa, ref Memory.Arena arena, int length
){
    if (soa.IsInitialised){
        Howl.Debug.Panic("Already Initialised.");
        return false;
    }

    Math.Init(ref soa.Aabbs, ref arena, length);
    Collections.Init(ref soa.LeftLeafIndices, ref arena, length);
    Collections.Init(ref soa.RightLeafIndices, ref arena, length);
    Collections.Init(ref soa.SubtreeSizes, ref arena, length);
    Collections.Init(ref soa.LeafCounts, ref arena, length);
    Collections.Init(ref soa.ParentIndices, ref arena, length);
    soa.Length = length;

    soa.IsInitialised = true;
    return true;
}

/// <summary>
///     Appends an entry into a soa at the soa instance's <c>AppendCount</c> index.
/// </summary>
public static void Append(
    ref Soa_Branch soa, float branchMinX, float branchMinY, float branchMaxX, float branchMaxY, int branchLeftLeafIndex, 
    int branchRightLeafIndex, int branchSubtreeSize, int branchLeafCount, int branchParentIndex
){
    Insert(ref soa, soa.AppendCount, branchMinX, branchMinY, branchMaxX, branchMaxY, branchLeftLeafIndex, branchRightLeafIndex, branchSubtreeSize, branchLeafCount, branchParentIndex);
    soa.AppendCount++;
}

/// <summary>
///     Inserts an entry into a soa instance.
/// </summary>
public static void Insert(ref Soa_Branch soa, int elementIndex, float branchMinX, float branchMinY, float branchMaxX, 
    float branchMaxY, int branchLeftLeafIndex, int branchRightLeafIndex, int branchSubtreeSize, int branchLeafCount, int branchParentIndex
){
    soa.Aabbs.MinX[elementIndex] = branchMinX;
    soa.Aabbs.MinY[elementIndex] = branchMinY;
    soa.Aabbs.MaxX[elementIndex] = branchMaxX;
    soa.Aabbs.MaxY[elementIndex] = branchMaxY;
    soa.LeftLeafIndices[elementIndex] = branchLeftLeafIndex;
    soa.RightLeafIndices[elementIndex] = branchRightLeafIndex;
    soa.SubtreeSizes[elementIndex] = branchSubtreeSize;
    soa.LeafCounts[elementIndex] = branchLeafCount;       
    soa.ParentIndices[elementIndex] = branchParentIndex;
}

/// <summary>
///     Sets a soa's <c>AppendCount</c> to zero.
/// </summary>
/// <param name="soa">the soa instance to clear.</param>
public static void Clear(
    ref Soa_Branch soa
){
    soa.AppendCount = 0;
}

/**##########################################################################################################################################
    div: Soa_Overlap
##########################################################################################################################################**/

/// <summary>
///     Creates a new Overlapsoa  instance.
/// </summary>
/// <param name="length">the maximum amount of overlaps this instance can hold; i.e. the length of the backing arrays.</param>
public static void Init(
    ref Soa_Overlap soa, ref Memory.Arena arena, int length
){
    Collections.Init(ref soa.OwnerLeafIndices, ref arena, length);
    Collections.Init(ref soa.OtherLeafIndices, ref arena, length);
    soa.Length = length;
}

/// <summary>
///     Appends a overlap to a overlap soa instance.
/// </summary>
public static void Append(
    ref Soa_Overlap soa , int ownerLeafIndex, int otherleafIndex
){
    int index = soa.AppendCount;
    soa.OwnerLeafIndices[index] = ownerLeafIndex;
    soa.OtherLeafIndices[index] = otherleafIndex;
    soa.AppendCount++;
}

/// <summary>
///     Sets the append count of an overlap soa instance to zero. 
/// </summary>
[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
public static void Clear(
    ref Soa_Overlap soa
){
    soa.AppendCount = 0;
}

/**##########################################################################################################################################
    div: Soa_QueryResult
##########################################################################################################################################**/

public static bool Init(
    ref Soa_QueryResult soa, ref Memory.Arena arena, int length
){
    if (soa.IsInitialised)
    {
        Howl.Debug.Panic("Already Initialised!");
        return false;
    }

    Collections.Init(ref soa.OwnerLeafIndices, ref arena, length);
    Collections.Init(ref soa.OtherLeafIndices, ref arena, length);
    Math.Init(ref soa.OwnerAabbs, ref arena, length);
    Math.Init(ref soa.OtherAabbs, ref arena, length);
    soa.Length = length;
    soa.IsInitialised = true;
    return true;
}

/// <summary>
///     Appends an entry into a soa at the soa instance's <c>AppendCount</c> index.
/// </summary>
[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
public static void Append(
    ref Soa_QueryResult soa, int resultOwnerLeafIndex, float resultOwnerMinX, float resultOwnerMinY, 
    float resultOwnerMaxX, float resultOwnerMaxY, int resultOtherLeafIndex, float resultOtherMinX, 
    float resultOtherMinY, float resultOtherMaxX, float resultOtherMaxY
){
    int count = soa.AppendCount;
    
    soa.OwnerLeafIndices[count] = resultOwnerLeafIndex;
    soa.OwnerAabbs.MinX[count]  = resultOwnerMinX;
    soa.OwnerAabbs.MinY[count]  = resultOwnerMinY;
    soa.OwnerAabbs.MaxX[count]  = resultOwnerMaxX;
    soa.OwnerAabbs.MaxY[count]  = resultOwnerMaxY;
    
    soa.OtherLeafIndices[count] = resultOtherLeafIndex;
    soa.OtherAabbs.MinX[count]  = resultOtherMinX;
    soa.OtherAabbs.MinY[count]  = resultOtherMinY;
    soa.OtherAabbs.MaxX[count]  = resultOtherMaxX;
    soa.OtherAabbs.MaxY[count]  = resultOtherMaxY;
    
    soa.AppendCount++;
}

/// <summary>
///     Sets a soa instance's <c>AppendCount</c> to zero.
/// </summary>
/// <param name="soa">the soa to clear.</param>
[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
public static void Clear(
    ref Soa_QueryResult soa
){
    soa.AppendCount = 0; 
}

/**##########################################################################################################################################
    div: BoundingVolumeHierarchy.
##########################################################################################################################################**/

public static bool Init(
    ref BoundingVolumeHierarchy bvh, ref Memory.Arena arena, int length
){
    if (bvh.IsInitialised)
    {
        Howl.Debug.Panic("Already Initialised.");
        return false;
    }

    // mapping each spatial pair onto one another (without duplicates) gives a length * 2 possible spatial pairs.
    int branchesLength = length*2;

    Init(ref bvh.Leaves, ref arena, length);
    Init(ref bvh.Branches, ref arena, branchesLength);
    Collections.Init(ref bvh.MortonCentroids, ref arena, length);
    Collections.Init(ref bvh.MortonLeafIds, ref arena, length);
    RadixSortBuffer.Initialise(ref bvh.RadixSortBuffer, ref arena, length);

    bvh.IsInitialised = true;
    return true;
}

/// <summary>
/// Sets the count of the bounding volume hierarchy's internal arrays to zero.
/// </summary>
/// <param name="bvh">the bvh to clear.</param>
public static void Clear(
    ref BoundingVolumeHierarchy bvh
){
    Clear(ref bvh.Leaves);
    Clear(ref bvh.Branches);
}

/*******************

    Tree Construction.

********************/

/// <summary>
/// Constructs a tree of branches from the leaves store in a bvh instance.
/// </summary>
/// <param name="bvh">the bvh instance.</param>
public static void ConstructTree(
    ref BoundingVolumeHierarchy bvh
){
    Clear(ref bvh.Branches);
    
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
    float rangeX = Math.Abs(maxX - minX);
    float rangeY = Math.Abs(maxY - minY);

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
    
    RadixSort.IndexedAscend(Collections.AsSpan(bvh.MortonCentroids), Collections.AsSpan(bvh.MortonLeafIds), 
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
            Math.UnionAabbs(
                aabbMinX, aabbMinY, aabbMaxX, aabbMaxY,
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
        Insert(ref branches, branchIndex, aabbMinX, aabbMinY, aabbMaxX, aabbMaxY, leftLeafIndex, rightLeafIndex, 1, leafCount, parentIndex);
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
        Math.UnionAabbs(
            leftMinX, leftMinY, leftMaxX, leftMaxY,
            rightMinX, rightMinY, rightMaxX, rightMaxY,
            out aabbMinX, out aabbMinY, out aabbMaxX, out aabbMaxY 
        );

        // set the sub tree.
        // note: subtree = everything written since this node.
        int subtreeSize = writeIndex - branchIndex;

        // set the branch.
        Insert(ref branches, branchIndex, aabbMinX, aabbMinY, aabbMaxX, aabbMaxY, 0, 0, subtreeSize, 0, parentIndex);
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
public static void FindOverlaps(Soa_Branch branches, Soa_Leaf leaves, ref Soa_Overlap overlaps)
{
    // clear any garbage data.
    Clear(ref overlaps);

    // hoisting of inavriance.
    System.Span<float> leafMinX         = Collections.AsSpan(leaves.Aabbs.MinX);
    System.Span<float> leafMinY         = Collections.AsSpan(leaves.Aabbs.MinY);
    System.Span<float> leafMaxX         = Collections.AsSpan(leaves.Aabbs.MaxX);
    System.Span<float> leafMaxY         = Collections.AsSpan(leaves.Aabbs.MaxY);
    System.Span<float> branchMinX       = Collections.AsSpan(branches.Aabbs.MinX);
    System.Span<float> branchMinY       = Collections.AsSpan(branches.Aabbs.MinY);
    System.Span<float> branchMaxX       = Collections.AsSpan(branches.Aabbs.MaxX);
    System.Span<float> branchMaxY       = Collections.AsSpan(branches.Aabbs.MaxY);
    System.Span<int> branchSubtreeSizes = Collections.AsSpan(branches.SubtreeSizes);
    System.Span<int> branchLeafCounts   = Collections.AsSpan(branches.LeafCounts);
    System.Span<int> rightLeafIndices   = Collections.AsSpan(branches.RightLeafIndices);
    System.Span<int> leftLeafIndices    = Collections.AsSpan(branches.LeftLeafIndices);
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
            if(!Math.AabbsIntersect(minX, branchMinX[otherBranch], minY, branchMinY[otherBranch], maxX, branchMaxX[otherBranch], maxY, branchMaxY[otherBranch]))
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
                    if(ownerLeaf < otherLeaf && LeafIntersectsAabb(leaves, otherLeaf, minX, minY, maxX, maxY)){
                        Append(ref overlaps, ownerLeaf, otherLeaf);
                    }
                    break;
                case 2:
                    otherLeaf = leftLeafIndices[otherBranch];
                    // ensure that there are no duplicates.
                    if(ownerLeaf < otherLeaf && LeafIntersectsAabb(leaves, otherLeaf, minX, minY, maxX, maxY)){
                        Append(ref overlaps, ownerLeaf, otherLeaf);
                    }
                    otherLeaf = rightLeafIndices[otherBranch];
                    // ensure that there are no duplicates.
                    if(ownerLeaf < otherLeaf && LeafIntersectsAabb(leaves, otherLeaf, minX, minY, maxX, maxY)){
                        Append(ref overlaps, ownerLeaf, otherLeaf);
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
    Clear(ref overlaps);

    // hoisting of inavriance.
    System.Span<float> leafMinX         = Collections.AsSpan(leaves.Aabbs.MinX);
    System.Span<float> leafMinY         = Collections.AsSpan(leaves.Aabbs.MinY);
    System.Span<float> leafMaxX         = Collections.AsSpan(leaves.Aabbs.MaxX);
    System.Span<float> leafMaxY         = Collections.AsSpan(leaves.Aabbs.MaxY);
    System.Span<int> leafCategories     = Collections.AsSpan(leaves.Categories);
    System.Span<float> branchMinX       = Collections.AsSpan(branches.Aabbs.MinX);
    System.Span<float> branchMinY       = Collections.AsSpan(branches.Aabbs.MinY);
    System.Span<float> branchMaxX       = Collections.AsSpan(branches.Aabbs.MaxX);
    System.Span<float> branchMaxY       = Collections.AsSpan(branches.Aabbs.MaxY);
    System.Span<int> branchSubtreeSizes = Collections.AsSpan(branches.SubtreeSizes);
    System.Span<int> rightLeafIndices   = Collections.AsSpan(branches.RightLeafIndices);
    System.Span<int> branchLeafCounts   = Collections.AsSpan(branches.LeafCounts);
    System.Span<int> leftLeafIndices    = Collections.AsSpan(branches.LeftLeafIndices);
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
            if(!Math.AabbsIntersect(minX, branchMinX[otherBranch], minY, branchMinY[otherBranch], maxX, branchMaxX[otherBranch], maxY, branchMaxY[otherBranch]))
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
                    if(ownerLeaf < otherLeaf && LeafIntersectsAabb(leaves, otherLeaf, minX, minY, maxX, maxY)){
                        // Soa_Overlap.Append(overlaps, ownerLeaf, otherLeaf);
                        Append(ref overlaps, ownerLeaf, otherLeaf, leafCategories[ownerLeaf], leafCategories[otherLeaf]);
                    }
                    break;
                case 2:
                    otherLeaf = leftLeafIndices[otherBranch];
                    // ensure that there are no duplicates.
                    if(ownerLeaf < otherLeaf && LeafIntersectsAabb(leaves, otherLeaf, minX, minY, maxX, maxY)){
                        Append(ref overlaps, ownerLeaf, otherLeaf, leafCategories[ownerLeaf], leafCategories[otherLeaf]);
                    }
                    otherLeaf = rightLeafIndices[otherBranch];
                    // ensure that there are no duplicates.
                    if(ownerLeaf < otherLeaf && LeafIntersectsAabb(leaves, otherLeaf, minX, minY, maxX, maxY)){
                        Append(ref overlaps, ownerLeaf, otherLeaf, leafCategories[ownerLeaf], leafCategories[otherLeaf]);
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
public static void AreaQuery(Soa_Branch branches, Soa_Leaf leaves, System.Span<int> overlapsOutput, ref int appendedOverlapsOutput, 
    float areaMinX, float areaMinY, float areaMaxX, float areaMaxY
){
    // reset to remove any garbage data.
    appendedOverlapsOutput = 0;

    // hoisting of invariance.
    System.Span<float> branchMinX       = Collections.AsSpan(branches.Aabbs.MinX);
    System.Span<float> branchMinY       = Collections.AsSpan(branches.Aabbs.MinY);
    System.Span<float> branchMaxX       = Collections.AsSpan(branches.Aabbs.MaxX);
    System.Span<float> branchMaxY       = Collections.AsSpan(branches.Aabbs.MaxY);
    System.Span<int> branchSubtreeSizes = Collections.AsSpan(branches.SubtreeSizes);
    System.Span<int> leftLeafIndices    = Collections.AsSpan(branches.LeftLeafIndices);
    System.Span<int> branchLeafCounts   = Collections.AsSpan(branches.LeafCounts);
    System.Span<int> rightLeafIndices   = Collections.AsSpan(branches.RightLeafIndices);
            
    int otherLeaf;

    int otherBranch = 0;
    while(otherBranch < branches.AppendCount)
    {
        if(!Math.AabbsIntersect(areaMinX, branchMinX[otherBranch], areaMinY, branchMinY[otherBranch], areaMaxX, branchMaxX[otherBranch], areaMaxY, branchMaxY[otherBranch]))
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
                if(LeafIntersectsAabb(leaves, otherLeaf, areaMinX, areaMinY, areaMaxX, areaMaxY)){
                    overlapsOutput[appendedOverlapsOutput] = otherLeaf;
                    appendedOverlapsOutput+=1;
                }
                // incemrent appended overlaps
                break;
            case 2:
                // add the left leaf.
                otherLeaf = leftLeafIndices[otherBranch];
                if(LeafIntersectsAabb(leaves, otherLeaf, areaMinX, areaMinY, areaMaxX, areaMaxY)){
                    overlapsOutput[appendedOverlapsOutput] = otherLeaf;
                    appendedOverlapsOutput+=1;
                }
                // add the right leaf.
                otherLeaf = rightLeafIndices[otherBranch];
                if(LeafIntersectsAabb(leaves, otherLeaf, areaMinX, areaMinY, areaMaxX, areaMaxY)){
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
public static void TreeQueryArea(
    BoundingVolumeHierarchy bvh, System.Span<int> overlapsOutput, ref int appendedOverlapsOutput, 
    float areaMinX, float areaMinY, float areaMaxX, float areaMaxY
){
    AreaQuery(bvh.Branches, bvh.Leaves, overlapsOutput, ref appendedOverlapsOutput, areaMinX, areaMinY, areaMaxX, areaMaxY);
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
public static void TreeRaycastQuery(
    Soa_Branch branches, Soa_Leaf leaves, System.Span<int> overlapsOutput, ref int appendedOverlapsOutput, 
    float rayStartX, float rayStartY, float rayEndX, float rayEndY
){
    // reset to remove any garbage data.
    appendedOverlapsOutput = 0;

    // hoisting of invariance.
    System.Span<float> branchMinX       = Collections.AsSpan(branches.Aabbs.MinX);
    System.Span<float> branchMinY       = Collections.AsSpan(branches.Aabbs.MinY);
    System.Span<float> branchMaxX       = Collections.AsSpan(branches.Aabbs.MaxX);
    System.Span<float> branchMaxY       = Collections.AsSpan(branches.Aabbs.MaxY);
    System.Span<int> branchSubtreeSizes = Collections.AsSpan(branches.SubtreeSizes);
    System.Span<int> leftLeafIndices    = Collections.AsSpan(branches.LeftLeafIndices);
    System.Span<int> branchLeafCounts   = Collections.AsSpan(branches.LeafCounts);
    System.Span<int> rightLeafIndices   = Collections.AsSpan(branches.RightLeafIndices);
            
    int otherLeaf;

    int otherBranch = 0;

    int i = 0;
    while (otherBranch < branches.AppendCount)
    {
        if (Math.AabbIntersectsLine(branchMinX[i], branchMinY[i], branchMaxX[i], branchMaxY[i], rayStartX, rayStartY, rayEndX, rayEndY) == false
        ){
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
                if(LeafIntersectsLine(leaves, otherLeaf, rayStartX, rayStartY, rayEndX, rayEndY)){
                    overlapsOutput[appendedOverlapsOutput] = otherLeaf;
                    appendedOverlapsOutput+=1;
                }
                break;
            case 2:
                // add the left leaf.
                otherLeaf = leftLeafIndices[otherBranch];
                if(LeafIntersectsAabb(leaves, otherLeaf, rayStartX, rayStartY, rayEndX, rayEndY)){
                    overlapsOutput[appendedOverlapsOutput] = otherLeaf;
                    appendedOverlapsOutput+=1;
                }
                // add the right leaf.
                otherLeaf = rightLeafIndices[otherBranch];
                if(LeafIntersectsLine(leaves, otherLeaf, rayStartX, rayStartY, rayEndX, rayEndY)){
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
public static void TreeRaycastQuery(
    BoundingVolumeHierarchy bvh, System.Span<int> overlapsOutput, ref int appendedOverlapsOutput, 
    float rayStartX, float rayStartY, float rayEndX, float rayEndY
){
    TreeRaycastQuery(bvh.Branches, bvh.Leaves, overlapsOutput, ref appendedOverlapsOutput, rayStartX, rayStartY, rayEndX, rayEndY);
}

/*******************

    Debug Drawing.

********************/

public static void DrawBranches(
    BoundingVolumeHierarchy bvh, Colour colour, float zPosition, int spriteLayer, int materialIndex, int cameraIndex, float wireThickness
){
    for(int i = 0; i < bvh.Branches.AppendCount; i++)
    {
        Rectangle rect = new(){
            X       = bvh.Branches.Aabbs.MinX[i],
            Y       = bvh.Branches.Aabbs.MinY[i],
            Width   = bvh.Branches.Aabbs.MaxX[i] - bvh.Branches.Aabbs.MinX[i],
            Height  = bvh.Branches.Aabbs.MaxY[i] - bvh.Branches.Aabbs.MinY[i],
        };
        N_Debug.Debug.DrawWireRect(rect, colour, zPosition, spriteLayer, materialIndex, cameraIndex, wireThickness);
    }
}

public static void DrawLeaves(
    BoundingVolumeHierarchy bvh, Colour colour, float zPosition, int spriteLayer, int materialIndex, int cameraIndex, float wireThickness
){
    for(int i = 0; i < bvh.Leaves.AppendCount; i++)
    {
        Rectangle rect = new(){
            X       = bvh.Leaves.Aabbs.MinX[i],
            Y       = bvh.Leaves.Aabbs.MinY[i],
            Width   = bvh.Leaves.Aabbs.MaxX[i] - bvh.Leaves.Aabbs.MinX[i],
            Height  = bvh.Leaves.Aabbs.MaxY[i] - bvh.Leaves.Aabbs.MinY[i],
        };
        N_Debug.Debug.DrawWireRect(rect, colour, zPosition, spriteLayer, materialIndex, cameraIndex, wireThickness);
    }
}

/**##########################################################################################################################################
    div: Intrusive List.
##########################################################################################################################################**/

public static bool Init(
    ref IntrusiveList list, ref Memory.Arena arena, int length, bool preserveRootOrder
){
    if (list.IsInitialised){
        Howl.Debug.Panic("Already Initialised.");
        return false;
    }

    Howl.Debug.Assert(length >= IntrusiveList.MinLength, 
        $"IntrusiveList must have a length greater than '{length}'."
    );

    length = Math.Clamp(length, IntrusiveList.MinLength, IntrusiveList.MaxLength);

    Collections.Init(ref list.Nodes, ref arena, length);
    Collections.Init(ref list.RootIndices, ref arena, length);

    Collections.Append(ref list.RootIndices, 0);

    list.IsInitialised=true;
    list.PreserveRootOrder = preserveRootOrder;
    return true;
}

/// <summary>
///     Adds a root node to the tree.
/// </summary>
[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
public static bool IntrusiveListAddRoot(
    ref IntrusiveList list, int nodeIndex
){
    // node cannot be the Nil.
    if(nodeIndex == 0){
        System.Diagnostics.Debug.Assert(false, "node: '{nodeIndex}' cannot be the Nil element.");
        return false;
    }

    ref IntrusiveListNode node = ref list.Nodes[nodeIndex];

    if (node.InTree){
        return false;
    }

    if(!Collections.Append(ref list.RootIndices, nodeIndex)){
        return false;
    }
    node.RootDenseIndex = list.RootIndices.Count-1;
    
    // node has no other siblings.
    node.NextSibling = nodeIndex;
    node.PreviousSibling = nodeIndex;

    node.InTree = true;
    return true;
}

/// <summary>
///     Adds a node to the tree.
/// </summary>
/// <remarks>
///     <para>Remarks:</para>
///     <para>if <c><paramref name="parentIndex"/></c> is zero, this will become a root node.</para>
/// </remarks>
/// <returns>
///     true, if successfully added to the tree; otherwise false if already added.
/// </returns>
public static bool IntrusiveListAddBranch(
    ref IntrusiveList list, int nodeIndex, int parentIndex
){
    // node cannot be the Nil.
    if(nodeIndex == 0){
        System.Diagnostics.Debug.Assert(false, "node: '{nodeIndex}' cannot be the Nil element.");
        return false;
    }

    // add as a root if parent index is zero.
    if(parentIndex == 0){
        return IntrusiveListAddRoot(ref list, nodeIndex);
    }

    ref Array<IntrusiveListNode> nodes = ref list.Nodes;
    ref IntrusiveListNode node = ref nodes[nodeIndex];

    if (node.InTree){
        return false;
    }
    
    ref IntrusiveListNode parent = ref nodes[parentIndex];
    if (parent.InTree == false){
        System.Diagnostics.Debug.Assert(false, " parent: '{parentIndex}' is not in the tree!");
        return false;
    }

    node.Parent = parentIndex;
    // only set if it is pointing to the Nil.
    if(parent.FirstChild == 0){
        parent.FirstChild = nodeIndex;
        
        // node has no other siblings (as it is the first child).
        node.NextSibling = nodeIndex;
        node.PreviousSibling = nodeIndex;
    }
    else{
        // get the last child.
        int lastChildIndex = nodes[parent.FirstChild].PreviousSibling;
        ref IntrusiveListNode lastChild = ref nodes[lastChildIndex];
        
        // get the first child.
        int firstChildIndex = parent.FirstChild;
        ref IntrusiveListNode firstChild = ref nodes[firstChildIndex];

        // connect last child to the new node.
        lastChild.NextSibling = nodeIndex;
        node.PreviousSibling = lastChildIndex;

        node.NextSibling = firstChildIndex;
        firstChild.PreviousSibling = nodeIndex;
    }

    node.InTree = true;
    return true;
}

/// <returns>
///     true, if successfully removed from the tree; otherwise false if already removed.
/// </returns>
public static bool IntrusiveListRemoveNode(
    ref IntrusiveList list, int nodeIndex
){
    // node cannot be the Nil.
    if(nodeIndex == 0){
        System.Diagnostics.Debug.Assert(false, "{nodeIndex} cannot be the Nil element.");
        return false;
    }

    ref Array<IntrusiveListNode> nodes = ref list.Nodes;
    ref Buffer<int> roots = ref list.RootIndices;
    ref IntrusiveListNode node = ref nodes[nodeIndex];

    if (node.InTree == false)
    {
        return false;
    }
    
    int parentIndex = node.Parent;
    int firstChildIndex = node.FirstChild;

    // deallocate from parent.
    if(parentIndex != 0)
    {
        node.Parent = 0;
        ref IntrusiveListNode parent = ref nodes[parentIndex];
        
        // if this node doesnt have any children;
        if(node.FirstChild == 0){
            // nil the parents child.
            if(parent.FirstChild == nodeIndex){
                parent.FirstChild = 0;
            }
        }
        else{
            if(parent.FirstChild == nodeIndex){
                // move the children to the parent.
                parent.FirstChild = node.FirstChild;

                // deallocate from children by setting their parent to this node's parent.
                ref IntrusiveListNode child = ref nodes[node.FirstChild];
                while (true){
                    child.Parent = parentIndex;
                    
                    int nextSiblingIndex = child.NextSibling;
                    
                    if(nextSiblingIndex == firstChildIndex){
                        break;
                    }

                    child = ref nodes[nextSiblingIndex];
                }
            }
            else{
                // append this node's children to it's parents children.

                int parentFirstChildIndex = parent.FirstChild;
                ref IntrusiveListNode parentFirstChild = ref nodes[parentFirstChildIndex];
                
                int parentLastChildIndex = parentFirstChild.PreviousSibling;
                ref IntrusiveListNode parentLastChild = ref nodes[parentLastChildIndex];
                
                parentLastChild.NextSibling = node.FirstChild;

                int currentSiblingIndex = node.FirstChild;
                ref IntrusiveListNode child = ref nodes[currentSiblingIndex];
                child.PreviousSibling = parentLastChildIndex;
                
                while (true)
                {
                    child.Parent = parentIndex;
                    
                    int nextSiblingIndex = child.NextSibling;
                    
                    if(nextSiblingIndex == firstChildIndex)
                    {
                        child.NextSibling = parentFirstChildIndex;
                        parentFirstChild.PreviousSibling = currentSiblingIndex;
                        break;
                    }

                    currentSiblingIndex = nextSiblingIndex;
                    child = ref nodes[nextSiblingIndex];
                }

                // don't perform sibling deallocation at the end of this function.
                // as the re-ordering of siblings in the parent has aready done this.
                // goto End;
            }
        }
    }
    else{

        switch(list.PreserveRootOrder){
            case true:
                // move all root node dense indices backward; reflecting the ordered removal.
                for(int i = node.RootDenseIndex+1; i < roots.Count; i++){
                    ref IntrusiveListNode nextRoot = ref nodes[roots[i]];
                    nextRoot.RootDenseIndex--;
                }
                // remove the root index.
                Collections.OrderedRemoveAt(ref roots, node.RootDenseIndex);
            break;
            case false:
                // remove the node from the roots array.
                // performing the dense index swap as well.
                ref IntrusiveListNode lastRoot = ref nodes[roots[roots.Count-1]];
                lastRoot.RootDenseIndex = node.RootDenseIndex;
                Collections.UnOrderedRemoveAt(ref roots, node.RootDenseIndex);
                node.RootDenseIndex = 0;
            break;
        }


        if (firstChildIndex != 0)
        {
            // deallocate from children by making them root nodes in the tree.
            int currentSiblingIndex = firstChildIndex;
            ref IntrusiveListNode child = ref nodes[currentSiblingIndex]; 
            while (true)
            {
                
                child.Parent = 0;

                // add children to root stack array.
                Collections.Append(ref roots, currentSiblingIndex);
                child.RootDenseIndex = roots.Count;

                // children are now roots, so they should no longer be associated with thier siblings.
                int nextSiblingIndex = child.NextSibling;
                child.NextSibling = currentSiblingIndex;
                child.PreviousSibling = currentSiblingIndex;

                if(nextSiblingIndex == firstChildIndex)
                {
                    break;
                }
                
                currentSiblingIndex = nextSiblingIndex;

                // go to the next sibling of the child.
                child = ref nodes[currentSiblingIndex];
            }

            // no need to deallocate from siblings, as this has already done that.
            goto End;
        }
    }


    // deallocate from siblings.
    ref IntrusiveListNode nextSibling = ref nodes[node.NextSibling];
    nextSibling.PreviousSibling = node.PreviousSibling;

    ref IntrusiveListNode previousSibling = ref nodes[node.PreviousSibling];
    previousSibling.NextSibling = node.NextSibling;

    End:
    node.InTree = false;
    return true;
}

/// <summary>
///     Sends a root node to the front of the <c><see cref="IntrusiveList.RootIndices"/></c> buffer.
/// </summary>
/// <remarks>
///    <para><b>Remarks:</b></para>
///    <para>The 'front' in this context is index <c>1</c> NOT <c>0</c> as the collection stores a <c>Nil Element</c>.</para>
/// </remarks>
[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
public static void IntrusiveListSendRootToFront(
    ref IntrusiveList tree, int rootIndex
){
    /**    
        shift all the node root indices - before the root index to send to the front - 
        forwards to reflect their new element positions after the ordered insertion. 
    **/
    for(int i = rootIndex; i > 0; i--){
        tree.Nodes[tree.RootIndices[i]].RootDenseIndex++;
    }

    // set the root node's root index to 1 (which is the front).
    int nodeIndex = tree.RootIndices[rootIndex];
    tree.Nodes[nodeIndex].RootDenseIndex = 1;

    // send the root node to the front of the root list.
    Collections.OrderedRemoveAt(ref tree.RootIndices, rootIndex);
    Collections.OrderedInsert(ref tree.RootIndices, nodeIndex, 1);
}

[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
public static bool IsIntrusiveListNodeInTree(
    IntrusiveList tree, int nodeIndex
){
    return tree.Nodes[nodeIndex].InTree;
}

public static bool IsIntrusiveListNodeRoot(
    IntrusiveList tree, int nodeIndex
){
    return tree.Nodes[nodeIndex].Parent == 0;
}

/// <summary>
///     Gets the root node of a node within an intrusive list.
/// </summary>
public static ref IntrusiveListNode GetIntrusiveListNodeRoot(
    IntrusiveList tree, int nodeIndex
){
    ref IntrusiveListNode node = ref tree.Nodes[nodeIndex];
    while(node.Parent != 0){
        node = ref tree.Nodes[node.Parent];
    }
    return ref node;
}

}