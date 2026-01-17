using System;
using System.Runtime.CompilerServices;

namespace Howl.Math;

public struct Vector2
{
    public static Vector2 Zero      = new(0,0);
    public static Vector2 One       = new(1,1);
    public static Vector2 Up        = new(0,1);
    public static Vector2 Down      = new(0,-1);
    public static Vector2 Left      = new(-1,0);
    public static Vector2 Right     = new(1,0);

    public float X;
    public float Y;

    public Vector2(float x, float y)
    {
        X = x;
        Y = y;
    }

    /// <summary>
    /// Adds the left-hand side Vector2 to the right-hand side.
    /// </summary>
    /// <param name="lhs">The left-hand side Vector2.</param>
    /// <param name="rhs">The right-hand side Vector2.</param>
    /// <returns>The resultant Vector2.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static Vector2 operator +(Vector2 lhs, Vector2 rhs)
    {
        return new Vector2(lhs.X + rhs.X, lhs.Y + rhs.Y);
    }

    /// <summary>
    /// Subtracts the left-hand side Vector2 from the right-hand side.
    /// </summary>
    /// <param name="lhs">The left-hand side Vector2.</param>
    /// <param name="rhs">The right-hand side Vector2.</param>
    /// <returns>The resultant Vector2.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static Vector2 operator -(Vector2 lhs, Vector2 rhs)
    {
        return new Vector2(lhs.X - rhs.X, lhs.Y - rhs.Y);
    }

    /// <summary>
    /// Divides the left-hand side Vector2 by the right-hand side.
    /// </summary>
    /// <param name="lhs">The left-hand side Vector2.</param>
    /// <param name="rhs">The right-hand side Vector2.</param>
    /// <returns>The resultant Vector2.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static Vector2 operator /(Vector2 lhs, Vector2 rhs)
    {
        return new Vector2(lhs.X/rhs.X, lhs.Y/rhs.Y);
    }

    /// <summary>
    /// Divides the left-hand side Vector2 by the right-hand side.
    /// </summary>
    /// <param name="lhs">The left-hand side Vector2.</param>
    /// <param name="rhs">The right-hand side value.</param>
    /// <returns>The resultant Vector2.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static Vector2 operator /(Vector2 lhs, float rhs)
    {
        return new Vector2(lhs.X/rhs, lhs.Y/rhs);
    }

    /// <summary>
    /// Multiplies the left-hand side Vector2 by the right-hand side.
    /// </summary>
    /// <param name="lhs">The left-hand side Vector2.</param>
    /// <param name="rhs">The right-hand side Vector2.</param>
    /// <returns>The resultant Vector2.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static Vector2 operator *(Vector2 lhs, Vector2 rhs)
    {
        return new Vector2(lhs.X*rhs.X, lhs.Y*rhs.Y);
    }

    /// <summary>
    /// Multiples the left-hand side Vector2 by the right-hand side.
    /// </summary>
    /// <param name="lhs">The left-hand side Vector2.</param>
    /// <param name="rhs">The right-hand side value.</param>
    /// <returns>The resultant Vector2.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static Vector2 operator *(Vector2 lhs, float rhs)
    {
        return new Vector2(lhs.X*rhs, lhs.Y*rhs);
    }

    /// <summary>
    /// Checks if the left-hand side Vector2 is equal to the right-hand side.
    /// </summary>
    /// <param name="lhs">The left-hand side Vector2.</param>
    /// <param name="rhs">The right-hand side Vector2.</param>
    /// <returns>true, if both are equal; otherwise false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static bool operator ==(Vector2 lhs, Vector2 rhs)
    {
        return lhs.X == rhs.X && lhs.Y == rhs.Y;
    }

    /// <summary>
    /// Checks if the left-hand side Vector2 is not equal to the right-hand side.
    /// </summary>
    /// <param name="lhs">The left-hand side Vector2.</param>
    /// <param name="rhs">The right-hand side Vector2.</param>
    /// <returns>true, if both are not equal; otherwise false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static bool operator !=(Vector2 lhs, Vector2 rhs)
    {
        return lhs.X != rhs.X || lhs.Y != rhs.Y;        
    }

    /// <summary>
    /// Checks if the object is equal to this Vector2.
    /// </summary>
    /// <param name="obj">The object to check against.</param>
    /// <returns>true, if both are equal; otherwise false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public override bool Equals(object obj)
    {
        return obj is Vector2 other && other == this;
    }

    /// <summary>
    /// Gets the squared length of this Vector2. 
    /// </summary>
    /// <returns>The squared length of this Vector2.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public readonly float LengthSquared()
    {
        return Dot(this, this);        
    }

    /// <summary>
    /// Gets the length of this Vector2.
    /// </summary>
    /// <returns>The length of this Vector2.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public readonly float Length()
    {
        float lengthSquared = LengthSquared();
        return MathF.Sqrt(lengthSquared);
    }

    /// <summary>
    /// Gets the dot product of two Vector2.
    /// </summary>
    /// <param name="lhs">The left-hand side Vector2.</param>
    /// <param name="rhs">The right-hand side Vector2.</param>
    /// <returns>The dot product.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static float Dot(Vector2 lhs, Vector2 rhs)
    {
        return (lhs.X * rhs.X) + (lhs.Y * rhs.Y);
    }

    /// <summary>
    /// Gets a Vector2 with the same direction as the specified Vector2, but with a length of one.
    /// </summary>
    /// <param name="value">The Vector2 to normalize.</param>
    /// <returns>The normalised Vector2.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static Vector2 Normalise(Vector2 value)
    {
        return value / value.Length();
    }

    /// <summary>
    /// Gets a Vector2 with the same dirrection as the specified Vector2, but with a length of one.
    /// </summary>
    /// <returns>The normalised Vector2.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public Vector2 Normalise()
    {
        return this / Length();
    }

    /// <summary>
    /// Gets a Vector2 with the signs of each value being flipped.
    /// </summary>
    /// <param name="vector2">The vector2 to perform this unary operator to.</param>
    /// <returns>The resultant Vector2.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static Vector2 operator -(Vector2 vector2)
    {
        return new Vector2(-vector2.X, -vector2.Y);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public override int GetHashCode()
    {
        return X.GetHashCode() ^ Y.GetHashCode(); // XOR the two hash codes together.
    }
}