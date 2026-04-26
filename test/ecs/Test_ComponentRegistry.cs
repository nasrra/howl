using Howl.Ecs;

namespace Howl.Test.Ecs;

public class Test_ComponentRegistry
{
    public struct Foo
    {
        public float X;
        public float Y;
        
        public Foo(float x, float y)
        {
            X = x ;
            Y = y;
        }

        public static bool operator ==(Foo a, Foo b)
        {
            return a.X == b.X && a.Y == b.Y;
        }

        public static bool operator !=(Foo a, Foo b)
        {
            return a.X != b.X && a.Y != b.Y;            
        }

        public override bool Equals(object? obj)
        {
            return obj is Foo other && other == this;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    public struct Loo
    {
        public float X;
        public float Y;  

        public Loo(float x, float y)
        {
            X = x ;
            Y = y;
        }

        public static bool operator ==(Loo a, Loo b)
        {
            return a.X == b.X && a.Y == b.Y;
        }

        public static bool operator !=(Loo a, Loo b)
        {
            return a.X != b.X && a.Y != b.Y;            
        }

        public override bool Equals(object? obj)
        {
            return obj is Loo other && other == this;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    [Fact]
    public void Constructor_Test()
    {
        for(int length = ComponentRegistry.MinComponentArrayLength; length < ComponentRegistry.MinComponentArrayLength + 9; length++)
        {
            ComponentRegistry registry = new(length);
            Assert_ComponentRegistry.LengthEqual(length, registry);
        }
    }   

    [Fact]
    public void RegisterAndGet_Test()
    {
        ComponentRegistry registry = new ComponentRegistry(ComponentRegistry.MinComponentArrayLength + 13);

        // check that registering a component returns the correct component Id.
        Assert.Equal(0, ComponentRegistry.RegisterComponent<Foo>(registry));
        Assert.Equal(1, ComponentRegistry.RegisterComponent<Loo>(registry));

        // get the component arrays.
        Assert.IsType<ComponentArray<Loo>>(ComponentRegistry.GetComponents<Loo>(registry));
        Assert.IsType<ComponentArray<Foo>>(ComponentRegistry.GetComponents<Foo>(registry));
    } 

    [Fact]
    public void EnforceNils_Test()
    {
        ComponentRegistry registry = new ComponentRegistry(ComponentRegistry.MinComponentArrayLength + 13);

        ComponentRegistry.RegisterComponent<Foo>(registry);
        ComponentRegistry.RegisterComponent<Loo>(registry);

        ComponentArray<Loo> loos = ComponentRegistry.GetComponents<Loo>(registry);
        ComponentArray<Foo> foos = ComponentRegistry.GetComponents<Foo>(registry);

        loos.Sparse[0] = new(13, 24);
        foos.Sparse[0] = new(98, 78);

        ComponentRegistry.EnforceNil(registry);
    
        Assert.True(loos.Sparse[0] == default);
        Assert.True(foos.Sparse[0] == default);
    }

    [Fact]
    public void Disposal_Test()
    {
        for(int length = ComponentRegistry.MinComponentArrayLength; length < ComponentRegistry.MinComponentArrayLength + 9; length++)
        {
            ComponentRegistry registry = new(length);
            ComponentRegistry.Dispose(registry);
            Assert_ComponentRegistry.Disposed(registry);
        }
    }
}