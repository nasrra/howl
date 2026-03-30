using System;
using Howl.Algorithms;
using Howl.Algorithms.Sorting;
using Howl.Math;
using Howl.Math.Shapes;

namespace Howl.DataStructures.Bvh;

public class Soa_BoundingVolumeHierarchy : IDisposable
{

    /// <summary>
    /// The radix sort buffer used when sorting this leaf buffer.
    /// </summary>
    public RadixSortBuffer RadixSortBuffer;

    /// <summary>
    /// The spatial pairs of leaves in the constructed tree.
    /// </summary>
    public Soa_SpatialPair SpatialPairs;

    /// <summary>
    /// The constructed branches from the inserted leaves.
    /// </summary>
    public Soa_Branch Branches;

    /// <summary>
    /// The leaves to construct branches from.
    /// </summary>
    public Soa_Leaf Leaves;

    /// <summary>
    /// The query result buffer for constructing spaial pairs after tree construction.
    /// </summary>
    public Soa_QueryResult SpatialPairQueryBuffer;

    /// <summary>
    /// The centroids of all leaf Aabbs
    /// </summary>
    /// <remarks>
    /// Use an element in <c>CentroidIds</> to get the leaf data associated with this centroid.
    /// Elements in <c>CentroidLeafIds</c> and <c>LeafCentroids</c> are associated via index.
    /// </remarks>
    public Soa_Vector2 LeafCentroids;

    /// <summary>
    /// The swapping buffer used for in-place permutation swapping of <c>LeafCentroids</c> xy-components.
    /// </summary>
    public float[] LeafCentroidsSwapBuffer;

    /// <summary>
    /// Used as an index for a centroid element in <c>Centroids</c> to get its associated leaf data in <c>Leaves</c>.
    /// </summary>
    /// <remarks>
    /// Elements in <c>CentroidLeafIds</c> and <c>LeafCentroids</c> are associated via index.
    /// </remarks>
    public int[] CentroidLeafIds;

    /// <summary>
    /// Whether this instance has been disposed of.
    /// </summary>
    public bool Disposed;

    /// <summary>
    /// Creates a new bounding volume hierarchy instance.
    /// </summary>
    /// <param name="length"></param>
    public Soa_BoundingVolumeHierarchy(int length, int spatialPairsLength)
    {
        Leaves = new(length);
        Branches = new(length*2);
        SpatialPairQueryBuffer = new(spatialPairsLength);
        LeafCentroids = new(length);
        CentroidLeafIds = new int[length];
        LeafCentroidsSwapBuffer = new float[length];
        RadixSortBuffer = new(length);
        SpatialPairs = new(spatialPairsLength);
    }

    /// <summary>
    /// Sets the count of the bounding volume hierarchy's internal arrays to zero.
    /// </summary>
    /// <param name="bvh">the bvh to clear.</param>
    public static void Clear(Soa_BoundingVolumeHierarchy bvh)
    {
        Soa_Leaf.Clear(bvh.Leaves);
        Soa_Branch.Clear(bvh.Branches);
    }

    public static void ConstructTree(Soa_BoundingVolumeHierarchy bvh)
    {
        int start = 0;
        int length = bvh.Leaves.AppendCount;

        Soa_Branch.Clear(bvh.Branches);
        Soa_Aabb.CalculateCentroids(bvh.Leaves.Aabbs, bvh.LeafCentroids.X, bvh.LeafCentroids.Y, start, length);
        // set centroid indices.
        for(int i = start; i < length; i++)
        {
            bvh.CentroidLeafIds[i] = i;
        }

        int BranchCount = 0;
        float aabbMinX = 0;
        float aabbMinY = 0;
        float aabbMaxX = 0;
        float aabbMaxY = 0;

        ConstructBranches(bvh.RadixSortBuffer, bvh.Branches, bvh.Leaves.Aabbs.MinX, bvh.Leaves.Aabbs.MinY, 
            bvh.Leaves.Aabbs.MaxX, bvh.Leaves.Aabbs.MaxY, bvh.LeafCentroids, 
            bvh.CentroidLeafIds, bvh.LeafCentroidsSwapBuffer, start, length, ref BranchCount, ref aabbMinX, 
            ref aabbMinY, ref aabbMaxX, ref aabbMaxY
        );

        // we set the branch count manually as the branches are inserted into the soa manually
        // without using the Append() function; this is okay as branch insertion in Construct branches
        // inserts branches in a 'subtree size' relative order for each branch; meaning, at the end of the 
        // construction of all branches, the data is contiguous (no holes in the array entries).
        bvh.Branches.AppendCount = BranchCount;

        ConstructSpatialPairs(bvh.Branches, bvh.Leaves, bvh.SpatialPairs, bvh.SpatialPairQueryBuffer);
    }

    public static void ConstructBranches(RadixSortBuffer radixSortBuffer, Soa_Branch branches, Span<float> leavesMinX, Span<float> leavesMinY, 
        Span<float> leavesMaxX, Span<float> leavesMaxY, Soa_Vector2 leafCentroids, Span<int> centroidLeafIndices, Span<float> centroidPermuteSwapBuffer, 
        int start, int length, ref int writeIndex, ref float aabbMinX, ref float aabbMinY, ref float aabbMaxX, ref float aabbMaxY
    )
    {
        // reserve space.
        int branchIndex = writeIndex++;

        // == leaf ==
        if (length <= 2)
        {
            // build leaf aabb.
            int leftLeafIndex = centroidLeafIndices[start];
            int rightLeafIndex = 0;
            int leafCount;
            aabbMinX = leavesMinX[leftLeafIndex];
            aabbMinY = leavesMinY[leftLeafIndex];
            aabbMaxX = leavesMaxX[leftLeafIndex];
            aabbMaxY = leavesMaxY[leftLeafIndex];

            if(length == 2)
            {
                // union the sibling leaf if there is one.
                rightLeafIndex = centroidLeafIndices[start + 1];
                Aabb.Union(aabbMinX, aabbMinY, aabbMaxX, aabbMaxY,
                    leavesMinX[rightLeafIndex], leavesMinY[rightLeafIndex], leavesMaxX[rightLeafIndex], leavesMaxY[rightLeafIndex],
                    out aabbMinX, out aabbMinY, out aabbMaxX, out aabbMaxY
                );                
                leafCount = 2;
            }
            else
            {
                leafCount = 1;
            }

            // insert the leaf.
            // note: subtree size for leaves is always one as subtree size is inclusive of then entry; and a leaf is the final in a branch chain.
            Soa_Branch.Insert(branches, branchIndex, aabbMinX, aabbMinY, aabbMaxX, aabbMaxY, leftLeafIndex, rightLeafIndex, 1, leafCount);
        }
        else
        {
            // == internal branch ==

            // get whether or not to vertically or horizontally split the branch.
            float minX = float.MaxValue;
            float minY = float.MaxValue;
            float maxX = float.MinValue;
            float maxY = float.MinValue;
            
            float centroidX;
            float centroidY;

            for(int i = start; i < length; i++)
            {
                centroidX = leafCentroids.X[i];
                centroidY = leafCentroids.Y[i];
                if(centroidX < minX && centroidY < minY)
                {
                    minX = centroidX;
                    minY = centroidY;
                }
                if(centroidX > maxX && centroidY > maxY)
                {
                    maxX = centroidX;
                    maxY = centroidY;
                }
            }

            float width = maxX - minX;
            float height = maxY - minY;

            // sort by the longest axis.
            if (width >= height)
            {
                // vertical split.
                RadixSortF.IndexedAscend(leafCentroids.X, centroidLeafIndices, radixSortBuffer, start, length);
                Swap.PermuteInPlace(leafCentroids.Y, centroidLeafIndices, centroidPermuteSwapBuffer, start, length);
            }
            else
            {
                // horizontal split.
                RadixSortF.IndexedAscend(leafCentroids.Y, centroidLeafIndices, radixSortBuffer, start, length);
                Swap.PermuteInPlace(leafCentroids.X, centroidLeafIndices, centroidPermuteSwapBuffer, start, length);
            }

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

            // left branch.
            ConstructBranches(radixSortBuffer, branches, leavesMinX, leavesMinY, 
                leavesMaxX, leavesMaxY, leafCentroids, centroidLeafIndices, centroidPermuteSwapBuffer, 
                leftStart, leftLength, ref writeIndex, ref leftMinX, ref leftMinY, ref leftMaxX, ref leftMaxY
            );

            // right branch.
            ConstructBranches(radixSortBuffer, branches, leavesMinX, leavesMinY, 
                leavesMaxX, leavesMaxY, leafCentroids, centroidLeafIndices, centroidPermuteSwapBuffer, 
                rightStart, rightLength, ref writeIndex, ref rightMinX, ref rightMinY, ref rightMaxX, ref rightMaxY
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
            Soa_Branch.Insert(branches, branchIndex, aabbMinX, aabbMinY, aabbMaxX, aabbMaxY, 0, 0, subtreeSize, 0);
        }

    }

    /// <summary>
    /// Queries a constructed tree of branches for any that overlap within a given area.
    /// </summary>
    /// <remarks>
    /// Appending to <paramref name="results"/> is a destructive process; the buffer will be cleared before it appends.
    /// </remarks>
    /// <param name="branches">the constructed tree of branches to query.</param>
    /// <param name="leaves">the leaf data associated with the branches.</param>
    /// <param name="results">the buffer of results to write overlap data to.</param>
    /// <param name="minX">the x-component of the query area minimum vertex.</param>
    /// <param name="minY">the y-component of the query area minimum vertex.</param>
    /// <param name="maxX">the x-component of the query area maximum vertex.</param>
    /// <param name="maxY">the y-component of the query area maximum vertex.</param>
    public static void AreaQuery(Soa_Branch branches, Soa_Leaf leaves, Soa_QueryResult results, float minX, float minY, float maxX, float maxY)
    {
        Soa_QueryResult.Clear(results);
        Span<float> branchMinX = branches.Aabbs.MinX;
        Span<float> branchMinY = branches.Aabbs.MinY;
        Span<float> branchMaxX = branches.Aabbs.MaxX;
        Span<float> branchMaxY = branches.Aabbs.MaxY;
        Span<int> branchSubtreeSizes = branches.SubtreeSizes;
        Span<int> branchLeafCounts = branches.LeafCounts;
        Span<int> leftLeafIndices = branches.LeftLeafIndices;
        Span<int> rightLeafIndices = branches.RightLeafIndices;

        int i = 0;
        while(i < branches.AppendCount)
        {
            if(!Aabb.Intersect(minX, minY, maxX, maxY, branchMinX[i], branchMinY[i], branchMaxX[i], branchMaxY[i]))
            {
                // skip the entire subtree.
                i+= branchSubtreeSizes[i];
                continue;
            }

            int leafCount = branchLeafCounts[i];

            switch (leafCount)
            {
                case 1:
                    // left leaf index should always be populated for branches with leaf(s) attatched.
                    Soa_Leaf.Query(leaves, results, [leftLeafIndices[i]], minX, minY, maxX, maxY);
                    break;
                case 2:
                    Soa_Leaf.Query(leaves, results, [leftLeafIndices[i], rightLeafIndices[i]], minX, minY, maxX, maxY);
                    break;
                case 0:
                    // do nothing..., just go to next branch in the tree..
                    break;
                default:
                    System.Diagnostics.Debug.Assert(false);
                    break;
            }

            // go to next branch in the tree.
            i++;
        }
    }

    /// <summary>
    /// Queries a build bounding-volume-hierarchy tree for leaves that overlap within a given area. 
    /// </summary>
    /// <remarks>
    /// Appending to <paramref name="results"/> is a destructive process; the buffer will be cleared before it appends.
    /// </remarks>
    /// <param name="bvh">the bounding-volume-hierarchy instance.</param>
    /// <param name="results">output buffer to write overlap data to.</param>
    /// <param name="minX">the x-component of the query area minimum vertex.</param>
    /// <param name="minY">the y-component of the query area minimum vertex.</param>
    /// <param name="maxX">the x-component of the query area maximum vertex.</param>
    /// <param name="maxY">the y-component of the query area maximum vertex.</param>
    public static void AreaQuery(Soa_BoundingVolumeHierarchy bvh, Soa_QueryResult results, float minX, float minY, float maxX, float maxY)
    {
        AreaQuery(bvh.Branches, bvh.Leaves, results, minX, minY, maxX, maxY);
    }

    /// <summary>
    /// Constructs the spatial pair list from a given leaf set.
    /// </summary>
    /// <remarks>
    /// Note: 
    /// - <paramref name="results"/> and <paramref name="spatialPairs"/> writing is destructive, the soa buffers will be cleared and output with this funcitons results.
    /// - This method does not produce duplicate spatial pairings.
    /// </remarks>
    /// <param name="branches">the constructed tree of branches to query.</param>
    /// <param name="leaves">the leaf data associated with the branches.</param>
    /// <param name="spatialPairs">the buffer of spatial pairs to write overlap data to.</param>
    /// <param name="results">the buffer of results to write temporary overlap data to.</param>
    public static void ConstructSpatialPairs(Soa_Branch branches, Soa_Leaf leaves, Soa_SpatialPair spatialPairs, Soa_QueryResult results)
    {
        Soa_SpatialPair.Clear(spatialPairs);
        Span<int> leafIndices = leaves.GenIndices.Indices;
        Span<int> leafGenerations = leaves.GenIndices.Generations;
        Span<int> leafFlags = leaves.Flags; 
        Span<float> leafMinX = leaves.Aabbs.MinX;
        Span<float> leafMinY = leaves.Aabbs.MinY;
        Span<float> leafMaxX = leaves.Aabbs.MaxX;
        Span<float> leafMaxY = leaves.Aabbs.MaxY;

        int ownerIndex;
        int ownerGeneration;
        int ownerFlag;

        for(int i = 0; i < leaves.AppendCount; i++)
        {
            ownerIndex = leafIndices[i];
            ownerGeneration = leafGenerations[i];
            ownerFlag = leafFlags[i];

            // get all near colliders to the owner leaf AABB.
            AreaQuery(branches, leaves, results, leafMinX[i], leafMinY[i], leafMaxX[i], leafMaxY[i]);

            for(int j = 0; j < results.AppendCount; j++)
            {
                int otherIndex = results.GenIndices.Indices[j];
                
                // ensure that spatial pairs are not added twice.
                if(ownerIndex >= otherIndex)
                {
                    continue;
                }

                Soa_SpatialPair.Append(spatialPairs, ownerIndex, ownerGeneration, ownerFlag, otherIndex, results.GenIndices.Generations[j], results.Flags[j]);
            }
        }
    }




    /*******************
    
        Debug Drawing.
    
    ********************/




    public static void DrawBranches(Howl.Graphics.Camera camera, Soa_BoundingVolumeHierarchy bvh, Howl.Graphics.Colour colour)
    {

        for(int i = 0; i < bvh.Branches.AppendCount; i++)
        {
            Debug.Draw.Wireframe(
                camera,
                new Transform(Vector2.Zero, Vector2.One, 0),
                new Rectangle(
                    new Vector2(bvh.Branches.Aabbs.MinX[i], bvh.Branches.Aabbs.MinY[i]), 
                    new Vector2(bvh.Branches.Aabbs.MaxX[i], bvh.Branches.Aabbs.MaxY[i])
                ), 
                colour
            );
        }

    }




    /*******************
    
        Disposal.
    
    ********************/




    public void Dispose()
    {
        Dispose(this);
    }

    public static void Dispose(Soa_BoundingVolumeHierarchy bvh)
    {
        if(bvh.Disposed)
            return;
        
        bvh.Disposed = true;
        
        RadixSortBuffer.Dispose(bvh.RadixSortBuffer);
        bvh.RadixSortBuffer = null;

        Soa_SpatialPair.Dispose(bvh.SpatialPairs);        
        bvh.SpatialPairs = null;
        
        Soa_Branch.Dispose(bvh.Branches);
        bvh.Branches = null;

        Soa_Leaf.Dispose(bvh.Leaves);
        bvh.Leaves = null;

        Soa_Vector2.Dispose(bvh.LeafCentroids);
        bvh.LeafCentroids = null;
        
        Soa_QueryResult.Dispose(bvh.SpatialPairQueryBuffer);
        bvh.SpatialPairQueryBuffer = null;
        
        bvh.CentroidLeafIds = null;
        
        bvh.LeafCentroidsSwapBuffer = null;

        GC.SuppressFinalize(bvh);
    }

    ~Soa_BoundingVolumeHierarchy()
    {
        Dispose(this);
    }
}