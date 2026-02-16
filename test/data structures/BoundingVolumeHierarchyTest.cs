using Howl.DataStructures;
using Howl.ECS;
using Howl.Math;
using Howl.Math.Shapes;
using static Howl.DataStructures.BoundingVolumeHierarchy;
using static System.Runtime.InteropServices.CollectionsMarshal;

namespace Howl.Test.DataStructures;

public class BoundingVolumeHierarchyTest
{
    [Fact]
    public void Instantiate_Test()
    {
        BoundingVolumeHierarchy bvh = new();
        Assert.Empty(bvh.Leaves);
        Assert.Empty(bvh.Branches);
        Assert.False(bvh.isDisposed);
        
        bvh.Dispose();
        
        Assert.Null(bvh.Leaves);
        Assert.Null(bvh.Branches);
        Assert.True(bvh.isDisposed);
    }

    [Fact]
    public void RegisterLeaf_Test()
    {
        BoundingVolumeHierarchy bvh = new();
        
        AABB aabb = new AABB(0,0,12,12);
        GenIndex genIndex = new GenIndex(1,2);

        byte flag = 1;

        Leaf leaf = new Leaf(
            aabb,
            genIndex,
            flag
        );
        
        InsertLeaf(bvh, leaf);

        ReadOnlySpan<Leaf> leaves = AsSpan(bvh.Leaves);
        Assert.Equal(1, leaves.Length);
        Assert.Equal(leaf, leaves[0]);
    }

    [Fact]
    public void ConstructTree_Test()
    {
        BoundingVolumeHierarchy bvh = new();

        AABB aabb;
        GenIndex genIndex;
        byte flag;
        Leaf leaf;

        aabb = new AABB(0,0,12,12);
        genIndex = new GenIndex(2,4);
        flag = 3;

        for(int x = 0; x < 20; x++)
        {
            for(int y = 0; y < 20; y++)
            {
                aabb = new AABB(x-1,y-1,x,y);
                genIndex = new GenIndex(x,0);
                flag = 0;
                leaf = new Leaf(aabb,genIndex,flag);
                InsertLeaf(bvh, leaf);
            }
        }
 
        ConstructTree(bvh);

        // ensure that there is atleast as many branches as there are leaves
        Assert.True(bvh.Branches.Count >= 20*20);
    }

    [Fact]
    public void AreaQuery_Test()
    {
        BoundingVolumeHierarchy bvh = new();

        AABB leafAABB;
        GenIndex genIndex;
        byte flag;

        // leaf 1 
        leafAABB = new AABB(0,0,10,10);
        genIndex = new GenIndex(0,0);
        flag = 0;
        Leaf leaf1 = new Leaf(leafAABB, genIndex, flag);
        InsertLeaf(bvh, leaf1);

        // leaf 2
        leafAABB = new AABB(10,10,20,20);
        genIndex = new GenIndex(1,0);
        flag = 0;
        Leaf leaf2 = new Leaf(leafAABB, genIndex, flag);
        InsertLeaf(bvh, leaf2);

        ConstructTree(bvh);
    
        // fail to intersect.
        ReadOnlySpan<QueryResult> zeroResult = AreaQuery(bvh, new AABB(100,100,333,333));
        Assert.Equal(0,zeroResult.Length);

        // find single intersect.
        ReadOnlySpan<QueryResult> singleResult = AreaQuery(bvh, new AABB(0,0,9,9));
        Assert.Equal(1,singleResult.Length);
        Assert.Equal(leaf1.Index,       singleResult[0].GenIndex.Index);
        Assert.Equal(leaf1.Generation,  singleResult[0].GenIndex.Generation);
        Assert.Equal(leaf1.Flag,        singleResult[0].Flag);

        // find dual intersect.
        ReadOnlySpan<QueryResult> doubleResult = AreaQuery(bvh, new AABB(5,5,20,20));
        Assert.Equal(2,doubleResult.Length);
        Assert.Equal(leaf1.Index,       doubleResult[0].GenIndex.Index);
        Assert.Equal(leaf1.Generation,  doubleResult[0].GenIndex.Generation);
        Assert.Equal(leaf1.Flag,        doubleResult[0].Flag);
        Assert.Equal(leaf2.Index,       doubleResult[1].GenIndex.Index);
        Assert.Equal(leaf2.Generation,  doubleResult[1].GenIndex.Generation);
        Assert.Equal(leaf2.Flag,        doubleResult[1].Flag);
    }

    [Fact]
    public void RaycastQuery_Test()
    {
        BoundingVolumeHierarchy bvh = new();

        AABB leafAABB;
        GenIndex genIndex;
        byte flag;

        // leaf 1 
        leafAABB = new AABB(0,0,10,10);
        genIndex = new GenIndex(0,0);
        flag = 0;
        Leaf leaf1 = new Leaf(leafAABB, genIndex, flag);
        InsertLeaf(bvh, leaf1);

        // leaf 2
        leafAABB = new AABB(10,10,20,20);
        genIndex = new GenIndex(1,0);
        flag = 0;
        Leaf leaf2 = new Leaf(leafAABB, genIndex, flag);
        InsertLeaf(bvh, leaf2);

        ConstructTree(bvh);

        // fail to interset.
        Span<QueryResult> zeroResult = RaycastQuery(bvh, new Vector2(-1,-1), new Vector2(-10,-10));
        Assert.Equal(0, zeroResult.Length);

        // find single intersect.
        ReadOnlySpan<QueryResult> singleResult = RaycastQuery(bvh, new Vector2(5,0), new Vector2(5,30));
        Assert.Equal(1,singleResult.Length);
        Assert.Equal(leaf1.Index,       singleResult[0].GenIndex.Index);
        Assert.Equal(leaf1.Generation,  singleResult[0].GenIndex.Generation);
        Assert.Equal(leaf1.Flag,        singleResult[0].Flag);

        // find double intersect.
        ReadOnlySpan<QueryResult> doubleResult = RaycastQuery(bvh, new Vector2(0,0), new Vector2(40,40));
        Assert.Equal(2,doubleResult.Length);
        Assert.Equal(leaf1.Index,       doubleResult[0].GenIndex.Index);
        Assert.Equal(leaf1.Generation,  doubleResult[0].GenIndex.Generation);
        Assert.Equal(leaf1.Flag,        doubleResult[0].Flag);
        Assert.Equal(leaf2.Index,       doubleResult[1].GenIndex.Index);
        Assert.Equal(leaf2.Generation,  doubleResult[1].GenIndex.Generation);
        Assert.Equal(leaf2.Flag,        doubleResult[1].Flag);
    }
}