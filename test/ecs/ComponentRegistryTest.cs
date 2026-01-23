using Howl.ECS;

namespace Howl.Test.ECS;

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
        ComponentRegistry registry = new(allocator);

        Assert.Equal(0, registry.RegisterComponent<Foo>());
        Assert.Equal(1, registry.RegisterComponent<Loo>());

        allocator.Allocate(out GenIndex index1);
        allocator.Allocate(out GenIndex index2);

        GenIndexList<Loo> loos = registry.Get<Loo>();
        Assert.NotNull(loos);
        Assert.Equal(2, loos.Sparse.Count);
        
        GenIndexList<Foo> foos = registry.Get<Foo>();
        Assert.NotNull(foos);
        Assert.Equal(2, foos.Sparse.Count);

        Assert.True(registry.ResizeSparseEntries(allocator.Entries.Count+1));
    }
}