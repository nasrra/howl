using Howl.ECS;
using Howl.Math.Shapes;

namespace Howl.Physics;

public static class SOAPhysicsSystem
{
    public static void AllocateCircleCollider(SOAPhysicsSystemState state, in Circle shape, bool isKinematic, bool isTrigger, out GenIndex genIndex)
    {

        // handle body type.

        PhysicsBodyType bodyType = PhysicsBodyType.CircleShape;
        if (isKinematic)
            bodyType |= PhysicsBodyType.KinematicCollider;

        if (isTrigger)
            bodyType |= PhysicsBodyType.TriggerCollider;
        else
            bodyType |= PhysicsBodyType.SolidCollider;
    
        // apply data.

        int index = state.Free.Pop();
        state.Radius[index]     = shape.Radius;
        state.VerticeX[index]   = shape.X;
        state.VerticeY[index]   = shape.Y;
        state.BodyType[index]   = bodyType;

        // return gen index.

        genIndex = new(index, state.Generation[index]);
    }
}