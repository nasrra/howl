using Howl.Math;

namespace Howl.Physics;

public struct RectangleRigidBody
{
    public RigidBody RigidBody {get; private set;}
    public float Width {get; private set;}
    public float Height {get; private set;}

    /// <summary>
    /// Contructs a RectangleRigdyBody.
    /// </summary>
    /// <param name="position"></param>
    /// <param name="rotation"></param>
    /// <param name="density"></param>
    /// <param name="restitution"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    public RectangleRigidBody(
        Vector2 position,
        float rotation,
        float density,
        float restitution,
        float width,
        float height
    )
    {
        float area = width * height;
        float mass = area * density;

        Width = width;
        Height = height;
        
        RigidBody = new RigidBody(
            position,
            rotation,
            area,
            density,
            mass,
            restitution
        );    
    }
}