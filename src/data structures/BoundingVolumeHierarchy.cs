using System;
using System.Collections.Generic;
using Howl.ECS;
using Howl.Math;
using Howl.Math.Shapes;
using static Howl.Math.Shapes.AABB;
using static Howl.DataStructures.Branch;
using static System.Runtime.InteropServices.CollectionsMarshal;
using System.Runtime.CompilerServices;

namespace Howl.DataStructures;

public sealed class BoundingVolumeHierarchy : IDisposable
{   
    /// <summary>
    /// Gets and sets the inserted leaves for constructin branches to.
    /// </summary>
    public List<Leaf> Leaves;

    /// <summary>
    /// Gets and sets the results gathered from a query call.
    /// </summary>
    public List<QueryResult> QueryResults;

    /// <summary>
    /// Gets the contructed branches from the inserted leaves.
    /// </summary>
    public List<Branch> Branches;

    public List<SpatialPair> SpatialPairs;

    /// <summary>
    /// Gets and sets whether or not this instance has been disposed.
    /// </summary>
    private bool disposed;

    /// <summary>
    /// Gets whether or not this instance has been disposed.
    /// </summary>
    public bool isDisposed => disposed;

    /// <summary>
    /// Creates a new Bounding Volume Hierarchy instance.
    /// </summary>
    public BoundingVolumeHierarchy()
    {
        Leaves = new();
        QueryResults = new();
        Branches = new();
        SpatialPairs = new();
    }
    
    /// <summary>
    /// Disposes this instance.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
    }

    private void Dispose(bool disposing)
    {
        if (disposed)
        {
            return;
        }
        
        if (disposing)
        {
            Clear(this);
            QueryResults = null;
            Leaves = null;
            Branches = null;
        }

        disposed = true;
        GC.SuppressFinalize(this);
    }

    private void ThrowIfDisposed()
    {
        if (disposed)
        {
            throw new ObjectDisposedException(nameof(BoundingVolumeHierarchy));
        }
    }

    ~BoundingVolumeHierarchy()
    {
        Dispose(false);
    }




    /*******************
    
        Data Handling.
    
    ********************/




    /// <summary>
    /// Inserts a leaf into a bounding-volume hierarchy for branch construction.
    /// </summary>
    /// <param name="bvh">the bounding volume hierarchy.</param>
    /// <param name="leaf">The leaf data.</param>
    public static void InsertLeaf(BoundingVolumeHierarchy bvh, Leaf leaf)
    {
        bvh.Leaves.Add(leaf);
    }

    /// <summary>
    /// Clears all stored data in a bounding-volume hierarchy.
    /// </summary>
    public static void Clear(BoundingVolumeHierarchy bvh)
    {
        bvh.Leaves.Clear();
        bvh.Branches.Clear();        
        bvh.QueryResults.Clear();
        bvh.SpatialPairs.Clear();
    }




    /*******************
    
        Construction.
    
    ********************/




    /// <summary>
    /// Constructs bouncing-volume-hierarchies tree with its stored leaves.
    /// </summary>
    /// <param name="bvh">the bounding-volume-hierarchy.</param>
    public static void ConstructTree(BoundingVolumeHierarchy bvh)
    {
        bvh.Branches.Clear();

        Span<Leaf> leafSpan = AsSpan(bvh.Leaves);
        Span<SortNode> nodeSpan = stackalloc SortNode[leafSpan.Length];

        ConstructSortNodes(nodeSpan, leafSpan);

        // the amount of branches that will be created in a give step.
        Span<Branch> branchSpan = stackalloc Branch[leafSpan.Length * 2];

        int writeIndex = 0; // start at the root node
        ConstructBranches(
            leafSpan, 
            branchSpan, 
            nodeSpan, 
            ref writeIndex, 
            out float boundingBoxMinX,
            out float boundingBoxMinY,
            out float boundingBoxMaxX,
            out float boundingBoxMaxY
        );
        bvh.Branches.AddRange(branchSpan[..writeIndex]);

        // pre-compute spatial pairs so systems dont have to query specfic leaves.
        ConstructSpatialPairs(bvh);
    }

    /// <summary>
    /// Constructs branches from a given span of leaves and sorting nodes.
    /// </summary>
    /// <param name="leafSpan">The span of leaves</param>
    /// <param name="branchSpan">The branch span to write branches to.</param>
    /// <param name="nodeSpan">The sorting nodes for the leaves.</param>
    /// <param name="writeIndex">The index of the most recently written branch in the loop.</param>
    /// <param name="aabb">The aabb of the current iterations constructed branch.</param>
    public static void ConstructBranches(
        ReadOnlySpan<Leaf> leafSpan,
        Span<Branch> branchSpan,
        Span<SortNode> nodeSpan, 
        ref int writeIndex,
        out float boundingBoxMinX,
        out float boundingBoxMinY,
        out float boundingBoxMaxX,
        out float boundingBoxMaxY
    )
    {
        // Reserve space
        int branchIndex = writeIndex++;
        ref Branch branch = ref branchSpan[branchIndex];

        // == Leaf ==
        if(nodeSpan.Length <= 2)
        {

            // build leaf AABB
            int leafIndex1 = nodeSpan[0].LeafIndex;
            ref readonly Leaf leaf1 = ref leafSpan[leafIndex1];
            boundingBoxMinX = leaf1.BoundingBoxMinX;
            boundingBoxMinY = leaf1.BoundingBoxMinY;
            boundingBoxMaxX = leaf1.BoundingBoxMaxX;
            boundingBoxMaxY = leaf1.BoundingBoxMaxY;


            if(nodeSpan.Length == 2)
            {
                // union the sibling leaf if there is one.
                int leafIndex2 = nodeSpan[1].LeafIndex;
                ref readonly Leaf leaf2 = ref leafSpan[leafIndex2];
                Union(
                    boundingBoxMinX,
                    boundingBoxMinY,
                    boundingBoxMaxX,
                    boundingBoxMaxY,
                    leaf2.BoundingBoxMinX,
                    leaf2.BoundingBoxMinY,
                    leaf2.BoundingBoxMaxX,
                    leaf2.BoundingBoxMaxY,
                    out boundingBoxMinX,
                    out boundingBoxMinY,
                    out boundingBoxMaxX,
                    out boundingBoxMaxY
                );
                
                // and set both leaf indices into the branch.
                SetLeafIndices(ref branch,[leafIndex1, leafIndex2]);
            }
            else
            {
                // set the only entry indice into the branch.
                SetLeafIndices(ref branch,[leafIndex1]);                
            }

            branch.BoundingBoxMinX = boundingBoxMinX;
            branch.BoundingBoxMinY = boundingBoxMinY;
            branch.BoundingBoxMaxX = boundingBoxMaxX;
            branch.BoundingBoxMaxY = boundingBoxMaxY;
            branch.SubtreeSize = 1;
            return;
        }

        // == Internal Node ==

        // get whether or not to vertically or horizontally split the branch.
        float minX = float.MaxValue;
        float minY = float.MaxValue;
        float maxX = float.MinValue;
        float maxY = float.MinValue;
        for(int i = 0; i < nodeSpan.Length; i++)
        {
            ref SortNode node = ref nodeSpan[i];
            float positionX = node.PositionX;
            float positionY = node.PositionY;

            if(positionX < minX && positionY < minY)
            {
                minX = positionX;
                minY = positionY;
            }
            if(positionX > maxX && positionY > maxY)
            {
                maxX = positionX;
                maxY = positionY;
            }
        }
        float width = maxX - minX;
        float height = maxY - minY;

        // sort by the longest axis.
        if (width >= height)
        {
            // vertical split.
            nodeSpan.Sort((a,b) => a.PositionX.CompareTo(b.PositionX));
        }
        else
        {
            // horizontal split.
            nodeSpan.Sort((a,b) => a.PositionY.CompareTo(b.PositionY));            
        }

        // split at the mid point.
        int mid = nodeSpan.Length/2;
        Span<SortNode> left = nodeSpan.Slice(0, mid);
        Span<SortNode> right = nodeSpan.Slice(mid, nodeSpan.Length - mid);

        // recurse (children are written contiguously after parent).
        ConstructBranches(
            leafSpan, 
            branchSpan, 
            left, 
            ref writeIndex, 
            out float leftBoundingBoxMinX,
            out float leftBoundingBoxMinY,
            out float leftBoundingBoxMaxX,
            out float leftBoundingBoxMaxY
        );
        
        ConstructBranches(
            leafSpan, 
            branchSpan, 
            right, 
            ref writeIndex, 
            out float rightBoundingBoxMinX,
            out float rightBoundingBoxMinY,
            out float rightBoundingBoxMaxX,
            out float rightBoundingBoxMaxY
        );

        // assign aabb.
        Union(
            leftBoundingBoxMinX,
            leftBoundingBoxMinY,
            leftBoundingBoxMaxX,
            leftBoundingBoxMaxY,
            rightBoundingBoxMinX,
            rightBoundingBoxMinY,
            rightBoundingBoxMaxX,
            rightBoundingBoxMaxY,
            out boundingBoxMinX,
            out boundingBoxMinY,
            out boundingBoxMaxX,
            out boundingBoxMaxY
        );
        branch.BoundingBoxMinX = boundingBoxMinX;
        branch.BoundingBoxMinY = boundingBoxMinY;
        branch.BoundingBoxMaxX = boundingBoxMaxX;
        branch.BoundingBoxMaxY = boundingBoxMaxY;

        // subtree = everything written since this node.
        branch.SubtreeSize = writeIndex - branchIndex;
    }

    /// <summary>
    /// Constructs sorting nodes for branch construction of a given span of leaves.
    /// </summary>
    /// <remarks>
    /// This functions assumes that nodeSpan and leafSpan are of equal length.
    /// </remarks>
    /// <param name="nodeSpan">The span of sort nodes to write to.</param>
    /// <param name="leafSpan">The leaf span to create sorting nodes from.</param>
    /// <returns></returns>
    public static Span<SortNode> ConstructSortNodes(Span<SortNode> nodeSpan, Span<Leaf> leafSpan)
    {
        for(int i = 0; i < leafSpan.Length; i++)
        {
            ref Leaf leaf = ref leafSpan[i];
            Center(
                leaf.BoundingBoxMinX, 
                leaf.BoundingBoxMinY, 
                leaf.BoundingBoxMaxX, 
                leaf.BoundingBoxMaxY,
                out float centerX,
                out float centerY
            );

            ref SortNode node = ref nodeSpan[i];
            node.LeafIndex = i;
            node.PositionX = centerX;
            node.PositionY = centerY;
        }
        return nodeSpan;
    }




    /*******************
    
        Area Querying.
    
    ********************/




    /// <summary>
    /// Queries a built bounding-volume-hierarchy tree for any leaves that may overlap within a given area.
    /// </summary>
    /// <remarks>
    /// Note: this function should not be called in parallel.
    /// </remarks>
    /// <param name="bvh">the bounding volume hierarchy.</param>
    /// <param name="aabb">The area to query for intersects.</param>
    /// <returns>A span of all of the found intersects.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static Span<QueryResult> AreaQuery(BoundingVolumeHierarchy bvh, in AABB aabb)
    {
        return AreaQuery(bvh, aabb.MinX, aabb.MinY, aabb.MaxX, aabb.MaxY);
    }

    /// <summary>
    /// Queries a built bounding-volume-hierarchy tree for any leaves that may overlap within a given area.
    /// </summary>
    /// <remarks>
    /// Note: this function should not be called in parallel.
    /// </remarks>
    /// <param name="bvh">the bounding-volume-hierarchy.</param>
    /// <param name="minX">the minimum x-component of the query area.</param>
    /// <param name="minY">the minimum y-component of the query area.</param>
    /// <param name="maxX">the maximum x-component of the query area.</param>
    /// <param name="maxY">the maximum y-component of the query area.</param>
    /// <returns>the resultant data found stored in the query location.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static Span<QueryResult> AreaQuery(
        BoundingVolumeHierarchy bvh, 
        float minX, 
        float minY, 
        float maxX, 
        float maxY
    )
    {
        AreaQuery(bvh.QueryResults, AsSpan(bvh.Branches), AsSpan(bvh.Leaves), minX, minY, maxX, maxY);
        return AsSpan(bvh.QueryResults);
    }

    /// <summary>
    /// Queries a set of branches for any leaves that may overlap within a given area.
    /// </summary>
    /// <remarks>
    /// Note: result writing is destructive, the list will be cleared before writing to it.
    /// </remarks>
    /// <param name="results">The list of results to write to.</param>
    /// <param name="branches">the branches to query.</param>
    /// <param name="leaves">the leaf data associated with each branch.</param>
    /// <param name="minX">the minimum x-component of the query area.</param>
    /// <param name="minY">the minimum y-component of the query area.</param>
    /// <param name="maxX">the maximum x-component of the query area.</param>
    /// <param name="maxY">the maximum y-component of the query area.</param>
    public static void AreaQuery(List<QueryResult> results, Span<Branch> branches, Span<Leaf> leaves, float minX, float minY, float maxX, float maxY)  
    {
        results.Clear();

        int i = 0;

        while (i < branches.Length)
        {
            ref Branch branch = ref branches[i];

            if (!Intersect(
                minX, 
                minY, 
                maxX, 
                maxY, 
                branch.BoundingBoxMinX, 
                branch.BoundingBoxMinY, 
                branch.BoundingBoxMaxX, 
                branch.BoundingBoxMaxY))
            {
                // skip entire subtree
                i += branch.SubtreeSize;
                continue;
            }

            if (branch.LeafCount > 0)
            {
                // leaf query results are appended to the results list.
                QueryLeaves(results, leaves, GetLeafIndices(branch), minX, minY, maxX, maxY);
                i++;
            }
            else
            {
                // descend to left child (always i + 1)
                i++;
            }
        }
    }

    /// <summary>
    /// Queries a leaf data set for any that may overlap within a given area.
    /// </summary>
    /// <remarks>
    /// Note: result writing is destructive, the list will be cleared before writing to it.
    /// </remarks>
    /// <param name="results">The list of results to write to.</param>
    /// <param name="leaves">the leaf data set.</param>
    /// <param name="leafIndices">the leaves to query.</param>
    /// <param name="aabb">the area to query.</param>
    public static void QueryLeaves(
        List<QueryResult> results,
        Span<Leaf> leaves, 
        Span<int> leafIndices, 
        in AABB aabb
    )
    {
        QueryLeaves(
            results,
            leaves, 
            leafIndices, 
            aabb.MinX, 
            aabb.MinY, 
            aabb.MaxX, 
            aabb.MaxY
        );            
    }

    /// <summary>
    /// Queries a leaf data set for any that may overlap within a given area.
    /// </summary>
    /// <remarks>
    /// Note: result writing is non-destructive, any found data will be appended to the results list.
    /// </remarks>
    /// <param name="results">The list of results to write to.</param>
    /// <param name="leaves">the leaf data set.</param>
    /// <param name="leafIndices">the leaves to query.</param>
    /// <param name="minX">the minimum vector x-component of the query area.</param>
    /// <param name="minY">the minimum vector y-component of the query area.</param>
    /// <param name="maxX">the maximum vector x-component of the query area.</param>
    /// <param name="maxY">the maximum vector y-component of the query area.</param>
    public static void QueryLeaves(
        List<QueryResult> results,
        Span<Leaf> leaves, 
        Span<int> leafIndices, 
        float minX, 
        float minY, 
        float maxX, 
        float maxY
    )
    {
        for(int i = 0; i < leafIndices.Length; i++)
        {
            ref Leaf leaf = ref leaves[leafIndices[i]];
            if(Intersect(leaf.BoundingBoxMinX, leaf.BoundingBoxMinY, leaf.BoundingBoxMaxX, leaf.BoundingBoxMaxY, minX, minY, maxX, maxY))
            {
                results.Add(
                    new QueryResult(
                        new GenIndex(leaf.Index, leaf.Generation), 
                        leaf.Flag
                    )
                );
            }
        }
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
    public static void RaycastLeaves(List<QueryResult> results, Span<Leaf> leaves, Span<int> leafIndices, Vector2 raycastStart, Vector2 raycastEnd)
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
    private static void RaycastLeaves(List<QueryResult> results, Span<Leaf> leaves, Span<int> leafIndices, float raycastStartX, float raycastStartY, float raycastEndX, float raycastEndY)
    {        
        for(int i = 0; i < leafIndices.Length; i++)
        {
            ref Leaf leaf = ref leaves[leafIndices[i]];
            if(LineIntersect(
                leaf.BoundingBoxMinX, leaf.BoundingBoxMinY, leaf.BoundingBoxMaxX, leaf.BoundingBoxMaxY, 
                raycastStartX, raycastStartY, raycastEndX, raycastEndY
            ))
            {
                results.Add(
                    new QueryResult(
                        new GenIndex(leaf.Index, leaf.Generation), 
                        leaf.Flag
                    )
                );
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
    public static Span<QueryResult> RaycastQuery(BoundingVolumeHierarchy bvh, Vector2 raycastStart, Vector2 raycastEnd)
    {
        RaycastQuery(bvh.QueryResults, AsSpan(bvh.Branches), AsSpan(bvh.Leaves), raycastStart.X, raycastStart.Y, raycastEnd.X, raycastEnd.Y);
        return AsSpan(bvh.QueryResults);
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
    public static void RaycastQuery(List<QueryResult> results, Span<Branch> branches, Span<Leaf> leaves, float raycastStartX, float raycastStartY, float raycastEndX, float raycastEndY)
    {
        results.Clear();

        int i = 0;

        while (i < branches.Length)
        {
            ref Branch branch = ref branches[i];

            if (LineIntersect(branch.BoundingBoxMinX, branch.BoundingBoxMinY, branch.BoundingBoxMaxX, branch.BoundingBoxMaxY, raycastStartX, raycastStartY, raycastEndX, raycastEndY) == false)
            {
                // skip entire subtree
                i += branch.SubtreeSize;
                continue;
            }

            if (branch.LeafCount > 0)
            {
                // leaf query results are appended to the results list.
                RaycastLeaves(results, leaves, GetLeafIndices(branch), raycastStartX, raycastStartY, raycastEndX, raycastEndY);
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
    
        Spatial Pairings.
    
    ********************/




    /// <summary>
    /// Rebuilds the spatial pair list for a bounding volume hierarchy.
    /// </summary>
    /// <remarks>
    /// Note: this function should not be called in parallel.
    /// </remarks>
    /// <param name="bvh">the bounding volume hierarchy.</param>
    public static void ConstructSpatialPairs(BoundingVolumeHierarchy bvh)
    {
        ConstructSpatialPairs(bvh.QueryResults, bvh.SpatialPairs, AsSpan(bvh.Branches), AsSpan(bvh.Leaves));
    }

    /// <summary>
    /// Rebuilds the spatial pair list from a given leaf set.
    /// </summary>
    /// <remarks>
    /// Note: 
    /// - nearResults and spatialPair writing is destructive, the lists will be cleared before writing to them.
    /// - This method does not produce duplicate pairings.
    /// </remarks>       
    /// <param name="nearResults">The list of results to write to when querying branches for leaves.</param>
    /// <param name="spatialPairs">The list of spatial pairs to write to when constructing a spatial pair.</param>
    /// <param name="branches">the branches to query.</param>
    /// <param name="leaves">the leaf data associated with each branch.</param>
    public static void ConstructSpatialPairs(List<QueryResult> nearResults, List<SpatialPair> spatialPairs, Span<Branch> branches, Span<Leaf> leaves)
    {

        spatialPairs.Clear();
        
        for(int i = 0; i < leaves.Length; i++)
        {
            ref readonly Leaf ownerLeaf = ref leaves[i]; 
            QueryResult owner = new QueryResult(new GenIndex(ownerLeaf.Index, ownerLeaf.Generation), ownerLeaf.Flag);

            // get all near collider to the leaf AABB.
            AreaQuery(
                nearResults, 
                branches, 
                leaves,
                ownerLeaf.BoundingBoxMinX,
                ownerLeaf.BoundingBoxMinY,
                ownerLeaf.BoundingBoxMaxX,
                ownerLeaf.BoundingBoxMaxY
            );

            Span<QueryResult> nearSpan = AsSpan(nearResults);

            for(int j = 0; j < nearSpan.Length; j++)
            {
                ref readonly QueryResult other = ref nearSpan[j];

                // ensure that spatial pairs are not added twice.
                if(owner.GenIndex.Index <= other.GenIndex.Index)
                {
                    continue;
                }

                spatialPairs.Add(new SpatialPair(owner, other));                
            }
        }

    }
}
