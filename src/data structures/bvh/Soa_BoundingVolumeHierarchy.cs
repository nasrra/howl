using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
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
    /// <remarks>
    /// Use a <c>spatialPairIndex</c> integer to access elements.
    /// </remarks>
    public Soa_SpatialPair SpatialPairs;

    /// <summary>
    /// The constructed branches from the inserted leaves.
    /// </summary>
    /// <remarks>
    /// Use a <c>branchIndex</c> integer to access elements.
    /// </remarks>
    public Soa_Branch Branches;

    /// <summary>
    /// The leaves to construct branches from.
    /// </summary>
    /// <remarks>
    /// Use a <c>leafIndex</c> integer to get access elements.
    /// </remarks>
    public Soa_Leaf Leaves;

    /// <summary>
    /// The query result buffer for constructing spaial pairs after tree construction.
    /// </summary>
    public Soa_QueryResult SpatialPairQueryBuffer;

    /// <summary>
    /// The morton codes for all leaf centroids.
    /// </summary>
    /// <remarks>
    /// Use a <c>MortonLeafIds</c> entry to access elements.
    /// </remarks>
    public uint[] MortonCentroids;

    /// <summary>
    /// Used as an index for an element in <c>MortonCentroids</c> to get its associated leaf data in <c>Leaves</c>.
    /// </summary>
    /// <remarks>
    /// Elements in <c>MortonLeafIds</c> and <c>MortonCentroids</c> are associated via index.
    /// </remarks>
    public int[] MortonLeafIds;

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
        MortonCentroids = new uint[length];
        MortonLeafIds = new int[length];
        RadixSortBuffer = new(length);
        SpatialPairs = new(spatialPairsLength);
    }

    /// <summary>
    /// Sets the count of the bounding volume hierarchy's internal arrays to zero.
    /// </summary>
    /// <param name="bvh">the bvh to clear.</param>
    public static void Clear(Soa_BoundingVolumeHierarchy bvh)
    {
        Soa_Leaf.ResetCount(bvh.Leaves);
        Soa_Branch.ResetCount(bvh.Branches);
    }




    /*******************
    
        Tree Construction.
    
    ********************/


    static Stopwatch a =  new();
    static Stopwatch b =  new();
    static Stopwatch c =  new();
    static Stopwatch d =  new();

    /// <summary>
    /// Constructs a tree of branches from the leaves store in a bvh instance.
    /// </summary>
    /// <param name="bvh">the bvh instance.</param>
    public static void ConstructTree(Soa_BoundingVolumeHierarchy bvh)
    {


        Soa_Branch.ResetCount(bvh.Branches);
        
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

        a.Restart();

        // get the morton code for sorting each of the centroids.
        float scaleX = 0;
        float scaleY = 0;
        MortonCode.CalculateScaleFactor(rangeX, rangeY, ref scaleX, ref scaleY);
        // MortonCode.CalculateMortonCodes(bvh.Leaves.Centroids.X, bvh.Leaves.Centroids.Y, bvh.MortonCentroids, minX, minY, scaleX, scaleY, 
        //     bvh.MortonCentroids.Length
        // );
        for(int i = 0; i < bvh.Leaves.AppendCount; i++)
        {
            bvh.MortonCentroids[i] = MortonCode.CalculateMortonCode(bvh.Leaves.Centroids.X[i], bvh.Leaves.Centroids.Y[i], minX, minY, scaleX, scaleY);
        }
        a.Stop();

        double at = a.Elapsed.TotalMilliseconds;

        // reset leaf indices.
        for(int i = 0; i < bvh.Leaves.AppendCount; i++)
        {
            bvh.MortonLeafIds[i] = i;
        }



        
        // b.Restart();
        
        RadixSort.IndexedAscend(bvh.MortonCentroids, bvh.MortonLeafIds, bvh.RadixSortBuffer, 0, bvh.Leaves.AppendCount);
    
        int branchCount = 0;
        int parentIndex = -1; // this will have to change to zero when we start enforcing Nils.
        float aabbMinX = 0;
        float aabbMinY = 0;
        float aabbMaxX = 0;
        float aabbMaxY = 0;

        // b.Stop();

        // double bt = b.Elapsed.TotalMilliseconds;

        // c.Restart();

        ConstructBranches(bvh.Branches, bvh.MortonLeafIds, bvh.Leaves.Aabbs.MinX, bvh.Leaves.Aabbs.MinY, bvh.Leaves.Aabbs.MaxX, bvh.Leaves.Aabbs.MaxY, 
            bvh.Leaves.BranchIndices, 0, bvh.Leaves.AppendCount, parentIndex, ref branchCount, ref aabbMinX, ref aabbMinY, ref aabbMaxX, ref aabbMaxY
        );

        // we set the branch count manually as the branches are inserted into the soa manually
        // without using the Append() function; this is okay as branch insertion in Construct branches
        // inserts branches in a 'subtree size' relative order for each branch; meaning, at the end of the 
        // construction of all branches, the data is contiguous (no holes in the array entries).
        bvh.Branches.AppendCount = branchCount;

        // c.Stop();
        // double ct = c.Elapsed.TotalMilliseconds;


        // d.Restart();
        ConstructSpatialPairs(bvh.Branches, bvh.Leaves, bvh.SpatialPairs, bvh.SpatialPairQueryBuffer);
        // d.Stop();
        // double dt = d.Elapsed.TotalMilliseconds;

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
    public static void ConstructBranches(Soa_Branch branches, Span<int> leafIndices, 
        Span<float> leavesMinX, Span<float> leavesMinY, Span<float> leavesMaxX, Span<float> leavesMaxY, Span<int> leafBranchIndices,
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
            Soa_Branch.Insert(branches, branchIndex, aabbMinX, aabbMinY, aabbMaxX, aabbMaxY, leftLeafIndex, rightLeafIndex, 1, leafCount, parentIndex);
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
            ConstructBranches(branches, leafIndices, leavesMinX, leavesMinY, leavesMaxX, leavesMaxY, leafBranchIndices,
                leftStart, leftLength, parentIndex, ref writeIndex, ref leftMinX, ref leftMinY, ref leftMaxX, ref leftMaxY
            );

            // right branch.
            ConstructBranches(branches, leafIndices, leavesMinX, leavesMinY, leavesMaxX, leavesMaxY, leafBranchIndices,
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
            Soa_Branch.Insert(branches, branchIndex, aabbMinX, aabbMinY, aabbMaxX, aabbMaxY, 0, 0, subtreeSize, 0, parentIndex);
        }

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
    
        Area Querying.
    
    ********************/




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




    /*******************
    
        Raycasting.
    
    ********************/




    /// <summary>
    /// Queries a span of leaves for any data the may overlap a given raycast.
    /// </summary>
    /// <remarks>
    /// Note: result writing is non-destructive, any found data will be appended to the results list.
    /// </remarks>
    /// <param name="results">The list of results to write to.</param>
    /// <param name="leaves">the leaf data set.</param>
    /// <param name="leafIndices">The leaves to query.</param>
    /// <param name="raycastStart">The starting position of the raycast.</param>
    /// <param name="raycastEnd">The end position of the raycast.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void RaycastLeaves(Soa_QueryResult results, Soa_Leaf leaves, Span<int> leafIndices, Vector2 raycastStart, Vector2 raycastEnd)
    {
        RaycastLeaves(results, leaves, leafIndices, raycastStart.X, raycastStart.Y, raycastEnd.X, raycastEnd.Y);
    }

    /// <summary>
    /// Queries a span of leaves for any data the may overlap a given raycast.
    /// </summary>
    /// <remarks>
    /// Note: result writing is non-destructive, any found data will be appended to the results list.
    /// </remarks>
    /// <param name="results">The list of results to write to.</param>
    /// <param name="leaves">the leaf data set.</param>
    /// <param name="leafIndices">The leaves to query.</param>
    /// <param name="raycastStartX">the raycast start position x-component.</param>
    /// <param name="raycastStartY">the raycast start position y-component.</param>
    /// <param name="raycastEndX">the raycast end position x-component.</param>
    /// <param name="raycastEndY">the raycast end position y-component.</param>
    private static void RaycastLeaves(Soa_QueryResult results, Soa_Leaf leaves, Span<int> leafIndices, float raycastStartX, float raycastStartY, float raycastEndX, float raycastEndY)
    {        
        for(int i = 0; i < leafIndices.Length; i++)
        {
            int leaf = leafIndices[i];
            if(Aabb.LineIntersect(
                leaves.Aabbs.MinX[leaf], leaves.Aabbs.MinY[leaf], leaves.Aabbs.MaxX[leaf], leaves.Aabbs.MaxY[leaf], 
                raycastStartX, raycastStartY, raycastEndX, raycastEndY
            ))
            {
                Soa_QueryResult.Append(results, leaves.GenIndices.Indices[leaf], leaves.GenIndices.Generations[leaf], leaves.Flags[leaf]);
            }
        }
    }

    /// <summary>
    /// Queries the a built bounding-volume-hierarchy tree for any given leaves that may overlap a given raycast.
    /// </summary>
    /// <remarks>
    /// Note: this function should not be called in parallel.
    /// </remarks>
    /// <param name="bvh">the bounding volume hierarchy.</param>
    /// <param name="raycastStart">The starting position of the raycast.</param>
    /// <param name="raycastEnd">The end position of the raycast.</param>
    /// <returns>the resultant data found stored in the query location.</returns>
    public static Soa_QueryResult RaycastQuery(Soa_BoundingVolumeHierarchy bvh, Vector2 raycastStart, Vector2 raycastEnd)
    {
        RaycastQuery(bvh.SpatialPairQueryBuffer, bvh.Branches, bvh.Leaves, raycastStart.X, raycastStart.Y, raycastEnd.X, raycastEnd.Y);
        return bvh.SpatialPairQueryBuffer;
    }

    /// <summary>
    /// Queries the a built bounding-volume-hierarchy tree for any given leaves that may overlap a given raycast.
    /// </summary>
    /// <remarks>
    /// Note: result writing is destructive, the list will be cleared before writing to it.
    /// </remarks>    
    /// <param name="results">The list of results to write to.</param>
    /// <param name="branches">the branches to query.</param>
    /// <param name="leaves">the leaf data associated with each branch.</param>
    /// <param name="raycastStartX">the x-component of the raycast starting position.</param>
    /// <param name="raycastStartY">the y-component of the raycast starting position.</param>
    /// <param name="raycastEndX">the x-component of the raycast ending position.</param>
    /// <param name="raycastEndY">the y-component of the raycast ending position.</param>
    public static void RaycastQuery(Soa_QueryResult results, Soa_Branch branches, Soa_Leaf leaves, float raycastStartX, float raycastStartY, float raycastEndX, float raycastEndY)
    {
        Soa_QueryResult.Clear(results);

        int i = 0;

        while (i < branches.AppendCount)
        {
            if (Aabb.LineIntersect(branches.Aabbs.MinX[i], branches.Aabbs.MinY[i], branches.Aabbs.MaxX[i], branches.Aabbs.MaxY[i], raycastStartX, raycastStartY, raycastEndX, raycastEndY) == false)
            {
                // skip entire subtree
                i += branches.SubtreeSizes[i];
                continue;
            }

            int leafCount = branches.LeafCounts[i];

            if (leafCount > 0)
            {
                if(leafCount > 0)
                {
                    RaycastLeaves(results, leaves, [branches.LeftLeafIndices[i], branches.RightLeafIndices[i]], raycastStartX, raycastStartY, raycastEndX, raycastEndY);                    
                }
                else
                {                    
                    RaycastLeaves(results, leaves, [branches.LeftLeafIndices[i]], raycastStartX, raycastStartY, raycastEndX, raycastEndY);
                }
                i++;
            }
            else
            {
                // descend to left child (always i + 1)
                i++;
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
        
        Soa_QueryResult.Dispose(bvh.SpatialPairQueryBuffer);
        bvh.SpatialPairQueryBuffer = null;
        
        bvh.MortonLeafIds = null;
        
        bvh.MortonCentroids = null;

        GC.SuppressFinalize(bvh);
    }

    ~Soa_BoundingVolumeHierarchy()
    {
        Dispose(this);
    }
}