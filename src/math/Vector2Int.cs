using System;
using System.Runtime.CompilerServices;

namespace Howl.Math;

public struct Vector2Int
{
    public static Vector2Int Zero   = new(0,0);
    public static Vector2Int One    = new(1,1);
    public static Vector2Int Up     = new(0,1);
    public static Vector2Int Down   = new(0,-1);
    public static Vector2Int Left   = new(-1,0);
    public static Vector2Int Right  = new(1,0);

    public int X;
    public int Y;

    public Vector2Int(int x, int y)
    {
        this.X = x;
        this.Y = y;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static Vector2Int operator +(Vector2Int lhs, Vector2Int rhs)
    {
        return new Vector2Int(lhs.X + rhs.X, lhs.Y + rhs.Y);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static Vector2Int operator -(Vector2Int lhs, Vector2Int rhs)
    {
        return new Vector2Int(lhs.X - rhs.X, lhs.Y - rhs.Y);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static Vector2Int operator /(Vector2Int lhs, Vector2Int rhs)
    {
        return new Vector2Int(lhs.X/rhs.X, lhs.Y/rhs.Y);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static Vector2Int operator *(Vector2Int lhs, Vector2Int rhs)
    {
        return new Vector2Int(lhs.X*rhs.X, lhs.Y*rhs.Y);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static bool operator ==(Vector2Int lhs, Vector2Int rhs)
    {
        return lhs.X == rhs.X && lhs.Y == rhs.Y;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static bool operator !=(Vector2Int lhs, Vector2Int rhs)
    {
        return lhs.X != rhs.X || lhs.Y != rhs.Y;        
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public override bool Equals(object obj)
    {
        return obj is Vector2Int other && other == this;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public override int GetHashCode()
    {
        return HashCode.Combine(X,Y);
    }
}