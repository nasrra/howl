namespace Howl.Physics;

public unsafe struct CollisionCallback<T> where T : allows ref struct
{
    public delegate* <T, CollisionInfo, void> Pointer;

    public CollisionCallback(delegate* <T, CollisionInfo, void> pointer)
    {
        Pointer = pointer;
    }
}