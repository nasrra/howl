using System.Runtime.CompilerServices;
using Howl.Math;

namespace Howl.Components;

public struct Position
{
    public Vector2 Value;
    
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static implicit operator Vector2(Position position) => position.Value;

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static implicit operator Position(Vector2 vector) => new(vector);

    public Position(Vector2 vector)
    {
        Value = vector;
    }

    public Position(float x, float y)
    {
        Value = new(x,y);
    }
}