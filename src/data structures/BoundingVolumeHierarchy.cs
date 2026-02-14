using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Howl.Math;
using Howl.Math.Shapes;

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
    private List<QueryResult> queryResult;

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
        queryResult = new();
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
        queryResult.Clear();
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

        ReadOnlySpan<Leaf> leafSpan = CollectionsMarshal.AsSpan(leaves);
        Span<SortNode> nodeSpan = stackalloc SortNode[leafSpan.Length];

        CreateSortNodes(nodeSpan, leafSpan);

        // the amount of branches that will be created in a give step.
        Span<Branch> branchSpan = stackalloc Branch[leafSpan.Length * 2];

        int writeIndex = 0; // start at the root node
        ConstructBranches(leafSpan, branchSpan, nodeSpan, ref writeIndex, out AABB aabb);
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
        out AABB aabb
    )
    {
        // Reserve space
        int branchIndex = writeIndex++;
        ref Branch branch = ref branchSpan[branchIndex];

        // == Leaf ==
        if(nodeSpan.Length <= 2)
        {
            // build leaf AABB
            aabb = leafSpan[nodeSpan[0].LeafIndex].AABB;

            if(nodeSpan.Length == 2)
            {
                // union the sibling leaf if there is one.
                aabb = new AABB(aabb, leafSpan[nodeSpan[1].LeafIndex].AABB);
                
                // and set both leaf indices into the branch.
                branch.SetLeafIndices(
                    [
                        nodeSpan[0].LeafIndex,
                        nodeSpan[1].LeafIndex
                    ]
                );
            }
            else
            {

                // set the only entry indice into the branch.
                branch.SetLeafIndices(
                    [
                        nodeSpan[0].LeafIndex,
                    ]
                );                
            }

            branch.AABB = aabb;
            branch.SubtreeSize = 1;
            return;
        }

        // == Internal Node ==

        // get whether or not to vertically or horizontally split the branch.
        Vector2 min = Vector2.MaxValue;
        Vector2 max = Vector2.MinValue;
        for(int i = 0; i < nodeSpan.Length; i++)
        {
            Vector2 centroid = nodeSpan[i].Centroid;
            min = min.Min(centroid);
            max = max.Max(centroid);
        }
        float width = max.X - min.X;
        float height = max.Y - min.Y;
        bool verticalSplit = width >= height;

        // sort by the longest axis.
        if (verticalSplit)
        {
            nodeSpan.Sort((a,b) => a.Centroid.X.CompareTo(b.Centroid.X));
        }
        else
        {
            nodeSpan.Sort((a,b) => a.Centroid.Y.CompareTo(b.Centroid.Y));            
        }

        // split at the mid point.
        int mid = nodeSpan.Length/2;
        Span<SortNode> left = nodeSpan.Slice(0, mid);
        Span<SortNode> right = nodeSpan.Slice(mid, nodeSpan.Length - mid);

        // recurse (children are written contiguously after parent).
        ConstructBranches(leafSpan, branchSpan, left, ref writeIndex, out AABB leftAABB);
        ConstructBranches(leafSpan, branchSpan, right, ref writeIndex, out AABB rightAABB);
        

        // assign aabb.
        aabb = new AABB(leftAABB, rightAABB);
        branch.AABB = aabb; 

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
    private Span<SortNode> CreateSortNodes(Span<SortNode> nodeSpan, ReadOnlySpan<Leaf> leafSpan)
    {
        for(int i = 0; i < leafSpan.Length; i++)
        {
            nodeSpan[i] = new SortNode(leafSpan[i].AABB.GetCentroid(), i);
        }
        return nodeSpan;
    }

    /// <summary>
    /// Queries the tree for any leaves that may overlap within a given area.
    /// </summary>
    /// <param name="aabb">The area to query for intersects.</param>
    /// <returns>A span of all of the found intersects.</returns>
    public ReadOnlySpan<QueryResult> Query(in AABB aabb)
    {
        queryResult.Clear();

        Span<Branch> branchSpan = CollectionsMarshal.AsSpan(branches);

        int i = 0;

        while (i < branchSpan.Length)
        {
            ref Branch branch = ref branchSpan[i];

            if (!AABB.Intersect(aabb, branch.AABB))
            {
                // skip entire subtree
                i += branch.SubtreeSize;
                continue;
            }

            if (branch.LeafCount > 0)
            {
                QueryLeaf(branch.GetLeafIndices(), aabb);
                i++;
            }
            else
            {
                // descend to left child (always i + 1)
                i++;
            }
        }

        return CollectionsMarshal.AsSpan(queryResult);
    }

    /// <summary>
    /// Queries the tree for any given leaves that may overlap a given raycast.
    /// </summary>
    /// <param name="raycastStart">The starting position of the raycast.</param>
    /// <param name="raycastEnd">The end position of the raycast.</param>
    /// <returns></returns>
    public ReadOnlySpan<QueryResult> Query(Vector2 raycastStart, Vector2 raycastEnd)
    {
        queryResult.Clear();

        Span<Branch> branchSpan = CollectionsMarshal.AsSpan(branches);

        int i = 0;

        while (i < branchSpan.Length)
        {
            ref Branch branch = ref branchSpan[i];

            if (!AABB.Intersect(branch.AABB, raycastStart, raycastEnd))
            {
                // skip entire subtree
                i += branch.SubtreeSize;
                continue;
            }

            if (branch.LeafCount > 0)
            {
                QueryLeaf(branch.GetLeafIndices(), raycastStart, raycastEnd);
                i++;
            }
            else
            {
                // descend to left child (always i + 1)
                i++;
            }
        }

        return CollectionsMarshal.AsSpan(queryResult);
    }

    /// <summary>
    /// Queries a leaf for any data the may overlap within a given area.
    /// </summary>
    /// <param name="leafIndices">The leaves to query.</param>
    /// <param name="aabb">The area to check intersects against.</param>
    private void QueryLeaf(ReadOnlySpan<int> leafIndices, in AABB aabb)
    {
        Span<Leaf> leafSpan = CollectionsMarshal.AsSpan(leaves);
        for(int i = 0; i < leafIndices.Length; i++)
        {
            ref Leaf leaf = ref leafSpan[leafIndices[i]];
            if(AABB.Intersect(leaf.AABB, aabb))
            {
                queryResult.Add(new QueryResult(leaf.GenIndex, leaf.Flag));
            }
        }
    }

    /// <summary>
    /// Queries a leaf for any data the may overlap a given raycast.
    /// </summary>
    /// <param name="leafIndices">The leaves to query.</param>
    /// <param name="raycastStart">The starting position of the raycast.</param>
    /// <param name="raycastEnd">The end position of the raycast.</param>
    private void QueryLeaf(ReadOnlySpan<int> leafIndices, Vector2 raycastStart, Vector2 raycastEnd)
    {
        Span<Leaf> leafSpan = CollectionsMarshal.AsSpan(leaves);
        for(int i = 0; i < leafIndices.Length; i++)
        {
            ref Leaf leaf = ref leafSpan[leafIndices[i]];
            if(AABB.Intersect(leaf.AABB, raycastStart, raycastEnd))
            {
                queryResult.Add(new QueryResult(leaf.GenIndex, leaf.Flag));
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
            QueryResult owner = new QueryResult(ownerLeaf.GenIndex, ownerLeaf.Flag);

            // get all near collider to the leaf AABB.
            ReadOnlySpan<QueryResult> nearSpan = Query(leafSpan[i].AABB);

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
            queryResult = null;
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
