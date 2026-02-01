
using Howl.Math;

namespace Howl.Physics;

public struct ColliderParameters
{
    public ColliderMode Mode;
    // public readonly bool IsStatic;

    public ColliderParameters(ColliderMode mode)
    {
        Mode = mode;
    }
}