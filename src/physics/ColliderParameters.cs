
using Howl.Math;

namespace Howl.Physics;

public struct ColliderParameters
{
    public bool IsTrigger;
    public bool IsStatic;

    public ColliderParameters(bool isTrigger, bool isStatic)
    {
        IsTrigger = isTrigger;
        IsStatic = isStatic;
    }
}