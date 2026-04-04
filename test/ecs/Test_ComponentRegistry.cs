namespace Howl.Test.ECS;

public class Test_ComponentRegistry
{
    public struct Foo()
    {
        public float x;
        public float y;
    }

    public struct Loo()
    {
        public float x;
        public float y;        
    }

    [Fact]
    public void Constructor_Test()
    {
        for(int length = ComponentRegistryNew.MinComponentArrayLength; length < ComponentRegistryNew.MinComponentArrayLength + 9; length++)
        {
            ComponentRegistryNew registry = new(length);
            Assert_ComponentRegistry.LengthEqual(length, registry);
        }
    }   

    [Fact]
    public void RegisterAndGet_Test()
    {
        ComponentRegistryNew registry = new ComponentRegistryNew(ComponentRegistryNew.MinComponentArrayLength + 13);

        // check that registering a component returns the correct component Id.
        Assert.Equal(0, ComponentRegistryNew.RegisterComponent<Foo>(registry));
        Assert.Equal(1, ComponentRegistryNew.RegisterComponent<Loo>(registry));

        // get the component arrays.
        Assert.IsType<ComponentArray<Loo>>(ComponentRegistryNew.GetComponents<Loo>(registry));
        Assert.IsType<ComponentArray<Foo>>(ComponentRegistryNew.GetComponents<Foo>(registry));
    } 

    [Fact]
    public void Disposal_Test()
    {
        for(int length = ComponentRegistryNew.MinComponentArrayLength; length < ComponentRegistryNew.MinComponentArrayLength + 9; length++)
        {
            ComponentRegistryNew registry = new(length);
            ComponentRegistryNew.Dispose(registry);
            Assert_ComponentRegistry.Disposed(registry);
        }
    }
}