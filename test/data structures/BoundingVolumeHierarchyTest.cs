using Howl.DataStructures;
using Howl.ECS;
using Howl.Math;
using Howl.Math.Shapes;

namespace Howl.Test.DataStructures;

public class BoundingVolumeHierarchyTest
{
    [Fact]
    public void Instantiate_Test()
    {
        BoundingVolumeHierarchy bvh = new();
        Assert.Equal(0, bvh.GetLeaves().Length);
        Assert.Equal(0, bvh.GetBranches().Length);
        Assert.False(bvh.IsDiposed);
        
        bvh.Dispose();
        
        Assert.Throws<ObjectDisposedException>(() => {bvh.GetLeaves();});
        Assert.Throws<ObjectDisposedException>(() => {bvh.GetBranches();});
        Assert.True(bvh.IsDiposed);
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
        
        bvh.InsertLeaf(leaf);

        ReadOnlySpan<Leaf> leaves = bvh.GetLeaves();
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
                bvh.InsertLeaf(leaf);
            }
        }
 
        bvh.Construct();

        // ensure that there is atleast as many branches as there are leaves
        Assert.True(bvh.GetBranches().Length >= 20*20);
    }

    [Fact]
    public void QueryTree_Test()
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
        bvh.InsertLeaf(leaf1);

        // leaf 2
        leafAABB = new AABB(10,10,20,20);
        genIndex = new GenIndex(1,0);
        flag = 0;
        Leaf leaf2 = new Leaf(leafAABB, genIndex, flag);
        bvh.InsertLeaf(leaf2);

        bvh.Construct();
    
        // fail to intersect.
        ReadOnlySpan<QueryResult> zeroResult = bvh.Query(new AABB(100,100,333,333));
        Assert.Equal(0,zeroResult.Length);

        // find single intersect.
        ReadOnlySpan<QueryResult> singleResult = bvh.Query(new AABB(0,0,9,9));
        Assert.Equal(1,singleResult.Length);
        Assert.Equal(leaf1.GenIndex,    singleResult[0].GenIndex);
        Assert.Equal(leaf1.Flag,        singleResult[0].Flag);

        // find dual intersect.
        ReadOnlySpan<QueryResult> doubleResult = bvh.Query(new AABB(5,5,20,20));
        Assert.Equal(2,doubleResult.Length);
        Assert.Equal(leaf1.GenIndex,    doubleResult[0].GenIndex);
        Assert.Equal(leaf1.Flag,        doubleResult[0].Flag);
        Assert.Equal(leaf2.GenIndex,    doubleResult[1].GenIndex);
        Assert.Equal(leaf2.Flag,        doubleResult[1].Flag);
    }
}