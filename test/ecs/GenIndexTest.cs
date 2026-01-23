using Howl.ECS;
using Howl.Generic;
using Xunit;

namespace Howl.Test.ECS;

public class GenIndexTest
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

    public GenIndexTest()
    {
        allocator = new();
        components = new();
        allocator.Allocate(out index0);
        allocator.Allocate(out index1);
        allocator.Allocate(out index2);
    }

    [Fact]
    public void GenIndexAllocate_Test()
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
    public void SparseResize_Test()
    {
        components.ResizeSparseEntries(allocator.Entries.Count);
        Assert.Equal(3, components.Sparse.Count);  
    }

    [Fact]
    public void ComponentAllocate_Test()
    {
        components.ResizeSparseEntries(allocator.Entries.Count);
        components.Allocate(index0, new Component());
        components.Allocate(index1, new Component());

        Assert.Equal(2,components.Dense.Count);

        // ensure that the sparse references correctly link to a dense index.

        Assert.Equal(GenIndexResult.Success, components.GetSparseRef(in index0, out ReadonlyRef<SparseEntry> sparse0));
        Assert.Equal(GenIndexResult.Success, components.GetSparseRef(in index1, out ReadonlyRef<SparseEntry> sparse1));

        Assert.Equal(0, sparse0.Value.DenseIndex);
        Assert.Equal(1, sparse1.Value.DenseIndex);
        Assert.Equal(2, components.Dense.Count);
    }

    [Fact]
    public void ComponentGetRef_Test()
    {
        components.ResizeSparseEntries(allocator.Entries.Count);
        components.Allocate(index0, new Component());
        components.Allocate(index1, new Component());

        Assert.Equal(GenIndexResult.Success, components.GetDenseRef(index0, out Ref<Component> c0)); 
        Assert.Equal(GenIndexResult.Success, components.GetDenseRef(index1, out Ref<Component> c1)); 
        Assert.Equal(GenIndexResult.DenseNotAllocated, components.GetDenseRef(index2, out Ref<Component> c2)); 
    }

    [Fact]
    public void ComponentModify_Test()
    {
        const int c0Value = 33;
        const int c1Value = 12;

        components.ResizeSparseEntries(allocator.Entries.Count);
        components.Allocate(index0, new Component());
        components.Allocate(index1, new Component());
        
        Assert.Equal(GenIndexResult.Success, components.GetDenseRef(index0, out Ref<Component> c0A));
        Assert.True(c0A.Valid);
        c0A.Value.x = c0Value;
        c0A.Value.y = c0Value;
        c0A.Value.z = c0Value;

        Assert.Equal(GenIndexResult.Success, components.GetDenseRef(index1, out Ref<Component> c1A));
        Assert.True(c1A.Valid);
        c1A.Value.x = c1Value;
        c1A.Value.y = c1Value;
        c1A.Value.z = c1Value;

        // ensure that the values are properly set within the list.

        components.GetDenseRef(index0, out Ref<Component> c0B);
        Assert.Equal(c0Value, c0B.Value.x);
        Assert.Equal(c0Value, c0B.Value.y);
        Assert.Equal(c0Value, c0B.Value.z);

        components.GetDenseRef(index1, out Ref<Component> c1B);
        Assert.Equal(c1Value, c1B.Value.x);
        Assert.Equal(c1Value, c1B.Value.y);
        Assert.Equal(c1Value, c1B.Value.z);
    }

    [Fact]
    public void ComponentDeallocate_Test()
    {
        const int c0Value = 33;
        const int c1Value = 12;

        components.ResizeSparseEntries(allocator.Entries.Count);
        components.Allocate(index0, new Component());
        components.Allocate(index1, new Component());

        // allocate component data to the gen index's.

        components.GetDenseRef(index0, out Ref<Component> c0A);
        Assert.True(c0A.Valid);
        c0A.Value.x = c0Value;
        c0A.Value.y = c0Value;
        c0A.Value.z = c0Value;

        components.GetDenseRef(index1, out Ref<Component> c1A);
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
        
        components.GetSparseRef(index0, out ReadonlyRef<SparseEntry> sparse0);
        components.GetSparseRef(index1, out ReadonlyRef<SparseEntry> sparse1);

        Assert.True(sparse0.Valid);
        Assert.False(sparse0.Value.LinkedToADenseEntry());

        Assert.True(sparse1.Valid);
        Assert.Equal(0, sparse1.Value.DenseIndex);

        // ensure that the values have not changed.

        components.GetDenseRef(index1, out Ref<Component> c1B);
        Assert.Equal(c1Value, c1B.Value.x);
        Assert.Equal(c1Value, c1B.Value.y);
        Assert.Equal(c1Value, c1B.Value.z);
    }
}