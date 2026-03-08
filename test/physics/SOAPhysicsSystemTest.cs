using Howl.ECS;
using Howl.Math.Shapes;
using Howl.Physics;
using static Howl.Physics.SOAPhysicsSystem;

namespace Howl.Test.Physics;

public class SOAPhysicsSystemTest
{
    int maxBodies = 10;
    int maxVertices = 100;
    SOAPhysicsSystemState state;

    public SOAPhysicsSystemTest(){
        state = new SOAPhysicsSystemState(maxBodies,maxVertices, new(), new());
    }

    [Fact]
    public void AllocateCircleCollider_Test()
    {
        float posX = 12;
        float posY = 13;
        float radius = 3;
        Circle circle = new(posX, posY, radius);

        AllocateCircleCollider(state, circle, true, false, out GenIndex genIndex);
        
        Assert.Equal(0, genIndex.Index);
        Assert.Equal(0, genIndex.Generation);
        Assert.Equal(state.VerticeX[genIndex.Index] , posX);    
        Assert.Equal(state.VerticeY[genIndex.Index] , posY);    
        Assert.Equal(state.Radius[genIndex.Index]   , radius);    
    }
}