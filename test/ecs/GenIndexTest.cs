using Howl.Ecs;
using Howl.Generic;
using Xunit;
using static Howl.Ecs.GenIndexListProc;

namespace Howl.Test.Ecs;

public class GenIndexTest
{
    internal struct Component
    {
        public int X;
        public int Y;
        public int Z;

        public Component(int x, int y, int z)
        {
            X = x;
            Y = y;
            Z = z;
        }
    }


    private void CreateTestBench(
        out GenIndexAllocator allocator, 
        out GenIndexList<Component> components,
        out GenIndex index0,
        out GenIndex index1,
        out GenIndex index2
    )
    {        
        allocator = new();
        components = new();
        allocator.Allocate(out index0, out _);
        allocator.Allocate(out index1, out _);
        allocator.Allocate(out index2, out _);
        ResizeSparseEntries(components, allocator.Entries.Count);
    }

    [Fact]
    public void GenIndexAllocate_Test()
    {
        GenIndexAllocator allocator = new();

        allocator.Allocate(out _, out _);
        allocator.Allocate(out _, out _);
        allocator.Allocate(out _, out _);
        for(int i = 0; i < allocator.Entries.Count; i++)
        {
            AllocatorEntry allocatorEntry = allocator.Entries[i];
            Assert.Equal(0, allocatorEntry.generation);
        }
        Assert.Equal(3, allocator.Entries.Count);
    }

    [Fact]
    public void SparseResize_Test()
    {
        GenIndexAllocator allocator = new();
        GenIndexList<Component> components = new();
        allocator.Allocate(out GenIndex index0, out _);
        allocator.Allocate(out GenIndex index1, out _);
        allocator.Allocate(out GenIndex index2, out _);
        ResizeSparseEntries(components, allocator.Entries.Count);
        Assert.Equal(3, components.Sparse.Count);  
    }

    [Fact]
    public void ComponentAllocate_Test()
    {
        CreateTestBench(out GenIndexAllocator allocator, out GenIndexList<Component> components, out GenIndex index0, out GenIndex index1, out GenIndex index2);

        Allocate(components, index0, new Component());
        Allocate(components, index1, new Component());

        Assert.Equal(2,components.Dense.Count);

        // ensure that the sparse references correctly link to a dense index.

        Assert.Equal(GenIndexResult.Ok, GetSparseReadOnlyRef(components, in index0, out ReadOnlyRef<SparseEntry> sparse0));
        Assert.Equal(GenIndexResult.Ok, GetSparseReadOnlyRef(components, in index1, out ReadOnlyRef<SparseEntry> sparse1));

        Assert.Equal(0, sparse0.Value.DenseIndex);
        Assert.Equal(1, sparse1.Value.DenseIndex);
        Assert.Equal(2, components.Dense.Count);
    }

    [Fact]
    public void ComponentGetRef_Test()
    {
        CreateTestBench(out GenIndexAllocator allocator, out GenIndexList<Component> components, out GenIndex index0, out GenIndex index1, out GenIndex index2);

        Allocate(components, index0, new Component());
        Allocate(components, index1, new Component());

        Assert.Equal(GenIndexResult.Ok, GetDenseRef(components, index0, out Ref<Component> c0)); 
        Assert.Equal(GenIndexResult.Ok, GetDenseRef(components, index1, out Ref<Component> c1)); 
        Assert.Equal(GenIndexResult.DenseNotAllocated, GetDenseRef(components, index2, out Ref<Component> c2)); 
    }

    [Fact]
    public void ComponentModify_Test()
    {
        CreateTestBench(out GenIndexAllocator allocator, out GenIndexList<Component> components, out GenIndex index0, out GenIndex index1, out GenIndex index2);
     
        const int c0Value = 33;
        const int c1Value = 12;

        Allocate(components, index0, new Component());
        Allocate(components, index1, new Component());
        
        Assert.Equal(GenIndexResult.Ok, GetDenseRef(components, index0, out Ref<Component> c0A));
        Assert.True(c0A.Valid);
        c0A.Value.X = c0Value;
        c0A.Value.Y = c0Value;
        c0A.Value.Z = c0Value;

        Assert.Equal(GenIndexResult.Ok, GetDenseRef(components, index1, out Ref<Component> c1A));
        Assert.True(c1A.Valid);
        c1A.Value.X = c1Value;
        c1A.Value.Y = c1Value;
        c1A.Value.Z = c1Value;

        // ensure that the values are properly set within the list.

        GetDenseRef(components, index0, out Ref<Component> c0B);
        Assert.Equal(c0Value, c0B.Value.X);
        Assert.Equal(c0Value, c0B.Value.Y);
        Assert.Equal(c0Value, c0B.Value.Z);

        GetDenseRef(components, index1, out Ref<Component> c1B);
        Assert.Equal(c1Value, c1B.Value.X);
        Assert.Equal(c1Value, c1B.Value.Y);
        Assert.Equal(c1Value, c1B.Value.Z);
    }

    [Fact]
    public void ComponentDeallocate_Test()
    {
        CreateTestBench(out GenIndexAllocator allocator, out GenIndexList<Component> components, out GenIndex index0, out GenIndex index1, out GenIndex index2);

        const int c0Value = 33;
        const int c1Value = 12;

        Allocate(components, index0, new Component());
        Allocate(components, index1, new Component());

        // allocate component data to the gen index's.

        GetDenseRef(components, index0, out Ref<Component> c0A);
        Assert.True(c0A.Valid);
        c0A.Value.X = c0Value;
        c0A.Value.Y = c0Value;
        c0A.Value.Z = c0Value;

        GetDenseRef(components, index1, out Ref<Component> c1A);
        Assert.True(c1A.Valid);
        c1A.Value.X = c1Value;
        c1A.Value.Y = c1Value;
        c1A.Value.Z = c1Value;

        // Deallocate the first gen index's dense data.

        GenIndexResult successResult = Deallocate(components, index0);
        Assert.Equal(GenIndexResult.Ok, successResult);

        Assert.Equal(GenIndexResult.DenseNotAllocated, Deallocate(components, index0));

        // ensure that the dense indexes are properly handled during deallocation.
        
        GetSparseReadOnlyRef(components, index0, out ReadOnlyRef<SparseEntry> sparse0);
        GetSparseReadOnlyRef(components, index1, out ReadOnlyRef<SparseEntry> sparse1);

        Assert.True(sparse0.Valid);
        Assert.False(sparse0.Value.HasDenseEntry());

        Assert.True(sparse1.Valid);
        Assert.Equal(0, sparse1.Value.DenseIndex);

        // ensure that the values have not changed.

        GetDenseRef(components, index1, out Ref<Component> c1B);
        Assert.Equal(c1Value, c1B.Value.X);
        Assert.Equal(c1Value, c1B.Value.Y);
        Assert.Equal(c1Value, c1B.Value.Z);
    }
    
    [Fact]
    public void StaleAllocationResult_Test()
    {
        CreateTestBench(out GenIndexAllocator allocator, out GenIndexList<Component> components, out GenIndex index0, out GenIndex index1, out GenIndex index2);
    
        GenIndexResult result;

        Allocate(components, index0, new (1,2,3));

        // deallocate the component and entity.
        Deallocate(components, index0);
        allocator.Deallocate(index0);

        // reuses the same slot as index 0 but the generation has changed.
        allocator.Allocate(out GenIndex newIndex0, out _);

        Allocate(components, newIndex0, new(2,3,4));

        result = GetDenseRef(components, index0, out Ref<Component> reference);

        Assert.Equal(GenIndexResult.StaleGenIndex, result); 

        result = GetDenseReadOnlyRef(components, index0, out ReadOnlyRef<Component> readOnlyReference);

        Assert.Equal(GenIndexResult.StaleGenIndex, result);
    }

    [Fact]
    public void DenseNotAllocatedResult_Test()
    {        
        CreateTestBench(out GenIndexAllocator allocator, out GenIndexList<Component> components, out GenIndex index0, out GenIndex index1, out GenIndex index2);
 
        GenIndexResult result;

        result = GetDenseReadOnlyRef(components, index0, out ReadOnlyRef<Component> componentReadOnlyRef); 
        
        Assert.Equal(GenIndexResult.DenseNotAllocated, result);
        
        result = GetDenseRef(components, index0, out Ref<Component> componentRef); 

        Assert.Equal(GenIndexResult.DenseNotAllocated, result);
    }
}