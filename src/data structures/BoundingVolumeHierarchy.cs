using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Howl.ECS;
using Howl.Math;
using Howl.Math.Shapes;
using static Howl.Math.Shapes.AABB;
using static Howl.DataStructures.Branch;
using System.Runtime.CompilerServices;

namespace Howl.DataStructures;

public sealed class BoundingVolumeHierarchy : IDisposable
{   
    /// <summary>
    /// Gets and sets the inserted leaves for constructin branches to.
    /// </summary>
    private List<Leaf> leaves;

    /// <summary>
    /// Gets and sets the results gathered from a query call.
    /// </summary>
    private List<QueryResult> queryResults;

    /// <summary>
    /// Gets the contructed branches from the inserted leaves.
    /// </summary>
    private List<Branch> branches;

    private List<SpatialPair> spatialPairs;

    /// <summary>
    /// Gets and sets whether or not this instance has been disposed.
    /// </summary>
    private bool disposed;

    /// <summary>
    /// Gets whether or not this instance has been disposed.
    /// </summary>
    public bool IsDiposed => disposed;

    /// <summary>
    /// Creates a new Bounding Volume Hierarchy instance.
    /// </summary>
    public BoundingVolumeHierarchy()
    {
        leaves = new();
        queryResults = new();
        branches = new();
        spatialPairs = new();
    }

    /// <summary>
    /// Inserts a leaf for constructing branches to.
    /// </summary>
    /// <param name="leaf">The leaf data.</param>
    public void InsertLeaf(Leaf leaf)
    {
        leaves.Add(leaf);
    }

    /// <summary>
    /// Clears all stored data.
    /// </summary>
    public void Clear()
    {
        leaves.Clear();
        branches.Clear();        
        queryResults.Clear();
        spatialPairs.Clear();
    }

    /// <summary>
    /// Gets a span of the constructed branches.
    /// </summary>
    /// <returns>the span.</returns>
    public ReadOnlySpan<Branch> GetBranches()
    {
        ThrowIfDisposed();
        return CollectionsMarshal.AsSpan(branches);
    }

    /// <summary>
    /// Gets a span of the inserted leaves.
    /// </summary>
    /// <returns>the span.</returns>
    public ReadOnlySpan<Leaf> GetLeaves()
    {
        ThrowIfDisposed();
        return CollectionsMarshal.AsSpan(leaves);
    }
    
    public ReadOnlySpan<SpatialPair> GetSpatialPairs()
    {
        ThrowIfDisposed();
        return CollectionsMarshal.AsSpan(spatialPairs);
    }

    /// <summary>
    /// Constructs the tree with the inserted leaves.
    /// </summary>
    public void Construct()
    {
        branches.Clear();

        Span<Leaf> leafSpan = CollectionsMarshal.AsSpan(leaves);
        Span<SortNode> nodeSpan = stackalloc SortNode[leafSpan.Length];

        CreateSortNodes(nodeSpan, leafSpan);

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
        branches.AddRange(branchSpan[..writeIndex]);

        // pre-compute spatial pairs so systems dont have to query specfic leaves.
        ConstructSpatialPairs();
    }

    /// <summary>
    /// Constructs branches from a given span of leaves and sorting nodes.
    /// </summary>
    /// <param name="leafSpan">The span of leaves</param>
    /// <param name="branchSpan">The branch span to write branches to.</param>
    /// <param name="nodeSpan">The sorting nodes for the leaves.</param>
    /// <param name="writeIndex">The index of the most recently written branch in the loop.</param>
    /// <param name="aabb">The aabb of the current iterations constructed branch.</param>
    private void ConstructBranches(
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
            float positionY = node.PositionX;

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
    /// Creates sorting nodes for branch construction of a given span of leaves.
    /// </summary>
    /// <remarks>
    /// This functions assumes that nodeSpan and leafSpan are of equal length.
    /// </remarks>
    /// <param name="nodeSpan">The span of sort nodes to write to.</param>
    /// <param name="leafSpan">The leaf span to create sorting nodes from.</param>
    /// <returns></returns>
    private Span<SortNode> CreateSortNodes(Span<SortNode> nodeSpan, Span<Leaf> leafSpan)
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

    /// <summary>
    /// Queries the tree for any leaves that may overlap within a given area.
    /// </summary>
    /// <param name="aabb">The area to query for intersects.</param>
    /// <returns>A span of all of the found intersects.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public ReadOnlySpan<QueryResult> Query(in AABB aabb)
    {
        return Query(aabb.MinX, aabb.MinY, aabb.MaxX, aabb.MaxY);
    }

    /// <summary>
    /// Queries the tree for any leaves that may overlap within a given area.
    /// </summary>
    /// <param name="minX">the minimum x-component of the query area.</param>
    /// <param name="minY">the minimum y-component of the query area.</param>
    /// <param name="maxX">the maximum x-component of the query area.</param>
    /// <param name="maxY">the maximum y-component of the query area.</param>
    /// <returns>the resultant data found stored in the query location.</returns>
    public ReadOnlySpan<QueryResult> Query(float minX, float minY, float maxX, float maxY)
    {
        queryResults.Clear();

        Span<Branch> branchSpan = CollectionsMarshal.AsSpan(branches);

        int i = 0;

        while (i < branchSpan.Length)
        {
            ref Branch branch = ref branchSpan[i];

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
                QueryLeaves(GetLeafIndices(branch), minX, minY, maxX, maxY);
                i++;
            }
            else
            {
                // descend to left child (always i + 1)
                i++;
            }
        }

        return CollectionsMarshal.AsSpan(queryResults);
    }

    /// <summary>
    /// Queries the tree for any given leaves that may overlap a given raycast.
    /// </summary>
    /// <param name="raycastStart">The starting position of the raycast.</param>
    /// <param name="raycastEnd">The end position of the raycast.</param>
    /// <returns>the resultant data found stored in the query location.</returns>
    public ReadOnlySpan<QueryResult> Query(Vector2 raycastStart, Vector2 raycastEnd)
    {
        queryResults.Clear();

        Span<Branch> branchSpan = CollectionsMarshal.AsSpan(branches);

        int i = 0;

        while (i < branchSpan.Length)
        {
            ref Branch branch = ref branchSpan[i];

            if (LineIntersect(branch.BoundingBoxMinX, branch.BoundingBoxMinY, branch.BoundingBoxMaxX, branch.BoundingBoxMaxY, raycastStart.X, raycastStart.Y, raycastEnd.X, raycastEnd.Y) == false)
            {
                // skip entire subtree
                i += branch.SubtreeSize;
                continue;
            }

            if (branch.LeafCount > 0)
            {
                RaycastLeaves(GetLeafIndices(branch), raycastStart, raycastEnd);
                i++;
            }
            else
            {
                // descend to left child (always i + 1)
                i++;
            }
        }

        return CollectionsMarshal.AsSpan(queryResults);
    }

    /// <summary>
    /// Queries a span leaves for any data the may overlap within a given area.
    /// </summary>
    /// <param name="leafIndices">The leaves to query.</param>
    /// <param name="aabb">The area to check intersects against.</param>
    /// <returns>the resultant data found stored in the query location.</returns>
    private void QueryLeaves(ReadOnlySpan<int> leafIndices, in AABB aabb)
    {
        QueryLeaves(leafIndices, aabb.MinX, aabb.MinY, aabb.MaxX, aabb.MaxY);
    }

    /// <summary>
    /// Queries a span leaves for any data the may overlap within a given area.
    /// </summary>
    /// <param name="leafIndices">the leaves to query.</param>
    /// <param name="minX">the minimum vector x-component of the query area.</param>
    /// <param name="minY">the minimum vector y-component of the query area.</param>
    /// <param name="maxX">the maximum vector x-component of the query area.</param>
    /// <param name="maxY">the maximum vector y-component of the query area.</param>
    private void QueryLeaves(ReadOnlySpan<int> leafIndices, float minX, float minY, float maxX, float maxY)
    {
        Span<Leaf> leafSpan = CollectionsMarshal.AsSpan(leaves);
        for(int i = 0; i < leafIndices.Length; i++)
        {
            ref Leaf leaf = ref leafSpan[leafIndices[i]];
            if(Intersect(leaf.BoundingBoxMinX, leaf.BoundingBoxMinY, leaf.BoundingBoxMaxX, leaf.BoundingBoxMaxY, minX, minY, maxX, maxY))
            {
                queryResults.Add(
                    new QueryResult(
                        new GenIndex(leaf.Index, leaf.Generation), 
                        leaf.Flag
                    )
                );
            }
        }
    }

    /// <summary>
    /// Queries a span of leaves for any data the may overlap a given raycast.
    /// </summary>
    /// <param name="leafIndices">The leaves to query.</param>
    /// <param name="raycastStart">The starting position of the raycast.</param>
    /// <param name="raycastEnd">The end position of the raycast.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private void RaycastLeaves(ReadOnlySpan<int> leafIndices, Vector2 raycastStart, Vector2 raycastEnd)
    {
        RaycastLeaves(leafIndices, raycastStart.X, raycastStart.Y, raycastEnd.X, raycastEnd.Y);
    }

    /// <summary>
    /// Queries a span of leaves for any data the may overlap a given raycast.
    /// </summary>
    /// <param name="leafIndices">The leaves to query.</param>
    /// <param name="raycastStartX">the raycast start position x-component.</param>
    /// <param name="raycastStartY">the raycast start position y-component.</param>
    /// <param name="raycastEndX">the raycast end position x-component.</param>
    /// <param name="raycastEndY">the raycast end position y-component.</param>
    private void RaycastLeaves(ReadOnlySpan<int> leafIndices, float raycastStartX, float raycastStartY, float raycastEndX, float raycastEndY)
    {        
        Span<Leaf> leafSpan = CollectionsMarshal.AsSpan(leaves);
        for(int i = 0; i < leafIndices.Length; i++)
        {
            ref Leaf leaf = ref leafSpan[leafIndices[i]];
            if(LineIntersect(
                leaf.BoundingBoxMinX, leaf.BoundingBoxMinY, leaf.BoundingBoxMaxX, leaf.BoundingBoxMaxY, 
                raycastStartX, raycastStartY, raycastEndX, raycastEndY
            ))
            {
                queryResults.Add(
                    new QueryResult(
                        new GenIndex(leaf.Index, leaf.Generation), 
                        leaf.Flag
                    )
                );
            }
        }
    }

    /// <summary>
    /// Rebuilds the spatial pair list from the current BVH leaf set.
    /// </summary>
    /// <remarks>
    /// This method performs the broad-phase pairing step by querying each leaf's
    /// AABB against the BVH and collecting all overlapping collider pairs. This 
    /// method does not produce duplicate pairings.
    ///
    /// For every leaf:
    /// - Its AABB is queried against the tree.
    /// - All overlapping leaves are retrieved.
    /// - Unique unordered pairs are generated and appended to <c>spatialPairs</c>.
    /// </remarks>
    private void ConstructSpatialPairs()
    {
        spatialPairs.Clear();

        ReadOnlySpan<Leaf> leafSpan = CollectionsMarshal.AsSpan(leaves);
        
        for(int i = 0; i < leafSpan.Length; i++)
        {
            ref readonly Leaf ownerLeaf = ref leafSpan[i]; 
            QueryResult owner = new QueryResult(new GenIndex(ownerLeaf.Index, ownerLeaf.Generation), ownerLeaf.Flag);

            // get all near collider to the leaf AABB.
            ReadOnlySpan<QueryResult> nearSpan = Query(
                ownerLeaf.BoundingBoxMinX,
                ownerLeaf.BoundingBoxMinY,
                ownerLeaf.BoundingBoxMaxX,
                ownerLeaf.BoundingBoxMaxY
            );

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
            Clear();
            queryResults = null;
            leaves = null;
            branches = null;
        }

        disposed = true;
        GC.SuppressFinalize(true);
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
}
