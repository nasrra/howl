using Howl.ECS;

namespace Howl.Test.ECS;

public class ComponentTypeTest
{
    private struct Position
    {
        public int X;
        public int Y;
        public int Z;

        public Position(int x, int y, int z)
        {
            X = x;
            Y = y;
            Z = z;
        }
    } 

    private struct Scale
    {
        public int X;
        public int Y;
        public int Z;

        public Scale(int x, int y, int z)
        {
            X = x;
            Y = y;
            Z = z;
        }
    }

    [Fact]
    public void Initialise_Test()
    {
        ComponentType<Position>.Initialise();

        // should fail because its already been initialised.
        Assert.Throws<InvalidOperationException>(ComponentType<Position>.Initialise);

        // should fail because the component has not been initialised yet.
        Assert.Throws<InvalidOperationException>(() =>  ComponentType<Scale>.GetId());

        ComponentType<Scale>.Initialise();
            
        Assert.Equal(0, ComponentType<Position>.GetId());
        Assert.Equal(1, ComponentType<Scale>.GetId());
    }
}