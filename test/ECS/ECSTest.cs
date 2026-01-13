using Howl.ECS;
using Howl.Generic;
using Xunit;

namespace Howl.Test.ECS;

public class ECSTest
{
    internal struct Component
    {
        public int x;
        public int y;
        public int z;
    }

    GenIndexAllocator allocator;
    GenIndexList<Component> components;
    GenIndex index0;
    GenIndex index1;
    GenIndex index2;

    public ECSTest()
    {
        allocator = new();
        components = new();
        allocator.Allocate(out index0);
        allocator.Allocate(out index1);
        allocator.Allocate(out index2);
    }

    [Fact]
    public void TestGenIndexAllocate()
    {
        allocator.Allocate(out _);
        allocator.Allocate(out _);
        allocator.Allocate(out _);
        for(int i = 0; i < allocator.Entries.Count; i++)
        {
            AllocatorEntry allocatorEntry = allocator.Entries[i];
            Assert.Equal(0, allocatorEntry.generation);
        }
        Assert.Equal(6, allocator.Entries.Count);
    }

    [Fact]
    public void TestSparseResize()
    {
        components.ResizeSparseEntries(allocator.GetEntriesCount());
        Assert.Equal(3, components.Sparse.Count);  
    }

    [Fact]
    public void TestComponentAllocate()
    {
        components.ResizeSparseEntries(allocator.GetEntriesCount());
        components.Allocate(index0, new Component());
        components.Allocate(index1, new Component());

        Assert.Equal(2,components.Dense.Count);

        // ensure that the sparse references correctly link to a dense index.

        ReadonlyRef<SparseEntry> sparse0 = components.GetSparseRef(index0, out _);
        ReadonlyRef<SparseEntry> sparse1 = components.GetSparseRef(index1, out _);

        Assert.True(sparse0.Valid);
        Assert.True(sparse1.Valid);

        Assert.Equal(0, sparse0.Value.DenseIndex);
        Assert.Equal(1, sparse1.Value.DenseIndex);
        Assert.Equal(2, components.Dense.Count);
    }

    [Fact]
    public void TestComponentGetRef()
    {
        components.ResizeSparseEntries(allocator.GetEntriesCount());
        components.Allocate(index0, new Component());
        components.Allocate(index1, new Component());
        Ref<Component> c0 = components.GetDenseRef(index0, out _);
        Ref<Component> c1 = components.GetDenseRef(index1, out _);        
        Ref<Component> c2 = components.GetDenseRef(index2, out _);

        Assert.True(c0.Valid); 
        Assert.True(c1.Valid); 
        Assert.False(c2.Valid); 
    }

    [Fact]
    public void TestComponentModify()
    {
        const int c0Value = 33;
        const int c1Value = 12;

        components.ResizeSparseEntries(allocator.GetEntriesCount());
        components.Allocate(index0, new Component());
        components.Allocate(index1, new Component());
        
        Ref<Component> c0A = components.GetDenseRef(index0, out _);
        Assert.True(c0A.Valid);
        c0A.Value.x = c0Value;
        c0A.Value.y = c0Value;
        c0A.Value.z = c0Value;

        Ref<Component> c1A = components.GetDenseRef(index1, out _);
        Assert.True(c1A.Valid);
        c1A.Value.x = c1Value;
        c1A.Value.y = c1Value;
        c1A.Value.z = c1Value;

        // ensure that the values are properly set within the list.

        Ref<Component> c0B = components.GetDenseRef(index0, out _);
        Assert.Equal(c0Value, c0B.Value.x);
        Assert.Equal(c0Value, c0B.Value.y);
        Assert.Equal(c0Value, c0B.Value.z);

        Ref<Component> c1B = components.GetDenseRef(index1, out _);
        Assert.Equal(c1Value, c1B.Value.x);
        Assert.Equal(c1Value, c1B.Value.y);
        Assert.Equal(c1Value, c1B.Value.z);
    }

    [Fact]
    public void TestDeallocate()
    {
        const int c0Value = 33;
        const int c1Value = 12;

        components.ResizeSparseEntries(allocator.GetEntriesCount());
        components.Allocate(index0, new Component());
        components.Allocate(index1, new Component());

        // allocate component data to the gen index's.

        Ref<Component> c0A = components.GetDenseRef(index0, out _);
        Assert.True(c0A.Valid);
        c0A.Value.x = c0Value;
        c0A.Value.y = c0Value;
        c0A.Value.z = c0Value;

        Ref<Component> c1A = components.GetDenseRef(index1, out _);
        Assert.True(c1A.Valid);
        c1A.Value.x = c1Value;
        c1A.Value.y = c1Value;
        c1A.Value.z = c1Value;

        // Deallocate the first gen index's dense data.

        GenIndexResult successResult = components.Deallocate(index0);
        Assert.Equal(GenIndexResult.Success, successResult);

        GenIndexResult errorResult = components.Deallocate(index0);
        Assert.Equal(GenIndexResult.DenseNotAllocated, errorResult);


        // ensure that the dense indexes are properly handled during deallocation.
        
        ReadonlyRef<SparseEntry> sparse0 = components.GetSparseRef(index0, out _);
        ReadonlyRef<SparseEntry> sparse1 = components.GetSparseRef(index1, out _);

        Assert.True(sparse0.Valid);
        Assert.False(sparse0.Value.LinkedToADenseEntry());

        Assert.True(sparse1.Valid);
        Assert.Equal(0, sparse1.Value.DenseIndex);

        // ensure that the values have not changed.

        Ref<Component> c1B = components.GetDenseRef(index1, out _);
        Assert.Equal(c1Value, c1B.Value.x);
        Assert.Equal(c1Value, c1B.Value.y);
        Assert.Equal(c1Value, c1B.Value.z);
    }
}