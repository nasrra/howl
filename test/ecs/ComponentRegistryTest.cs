using Howl.Ecs;

namespace Howl.Test.Ecs;

public class ComponentRegistryTest
{
    private struct Foo
    {
        public float X;
        public float Y;
        public float Z;

        public Foo(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }
    }

    private struct Loo
    {
        public byte R;
        public byte G;
        public byte B;
        public byte A;

        public Loo(byte r, byte g, byte b, byte a)
        {
            R = r; 
            G = g;
            B = b;
            A = a;
        }
    }

    [Fact]
    public void Test()
    {
        GenIndexAllocator allocator = new();
        ComponentRegistry worldRegistry = new(allocator);
        ComponentRegistry guiRegistry = new(allocator);

        // check that registering a component returns the correct 
        // component Id.
        Assert.Equal(0, worldRegistry.RegisterComponent<Foo>());
        Assert.Equal(1, worldRegistry.RegisterComponent<Loo>());

        // ensure that duplicate calls to registering a component
        // returns the same 
        Assert.Equal(0, worldRegistry.RegisterComponent<Foo>());
        Assert.Equal(1, worldRegistry.RegisterComponent<Loo>());

        // allocate entity entries.
        allocator.Allocate(out GenIndex index1, out _);
        allocator.Allocate(out GenIndex index2, out _);

        // check that the registry has updated its spars entries alongside the allocator.
        GenIndexList<Loo> loos = worldRegistry.Get<Loo>();
        Assert.NotNull(loos);
        Assert.Equal(2, loos.Sparse.Count);

        // check that the registry has updated its spars entries alongside the allocator.        
        GenIndexList<Foo> foos = worldRegistry.Get<Foo>();
        Assert.NotNull(foos);
        Assert.Equal(2, foos.Sparse.Count);

        // no gen index list should have been allocated as the component was not registered in this component registry.
        Assert.Null(guiRegistry.Get<Foo>());
        Assert.Null(guiRegistry.Get<Loo>());

        // resize to one above the current entry count.
        Assert.True(worldRegistry.ResizeSparseEntries(allocator.Entries.Count+1));
    }
}