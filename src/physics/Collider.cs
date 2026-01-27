
using Howl.Math;

namespace Howl.Physics;

public struct Collider
{
    public bool IsTrigger;
    public bool IsStatic;

    public Collider(bool isTrigger, bool isStatic)
    {
        IsTrigger = isTrigger;
        IsStatic = isStatic;
    }
}