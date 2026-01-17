using System;
using System.Runtime.CompilerServices;

namespace Howl.Math;

public struct Vector3
{
    public static Vector3 Zero      = new(0,0,0);
    public static Vector3 One       = new(1,1,1);
    public static Vector3 Up        = new(0,1,0);
    public static Vector3 Down      = new(0,-1,0);
    public static Vector3 Left      = new(-1,0,0);
    public static Vector3 Right     = new(1,0,0);
    public static Vector3 Forward   = new(0,0,1);
    public static Vector3 Backward  = new(0,0,-1);

    public float X;
    public float Y;
    public float Z;

    public Vector3(float x, float y, float z)
    {
        X = x;
        Y = y;
        Z = z;
    }

    public Vector3(Vector2 vector, float z)
    {
        X=vector.X;
        Y=vector.Y;
        Z=z;
    }

    /// <summary>
    /// Adds the left-hand side vector to the right-hand side.
    /// </summary>
    /// <param name="lhs">The left-hand side vector.</param>
    /// <param name="rhs">The right-hand side vector.</param>
    /// <returns>The resultant vector.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static Vector3 operator +(Vector3 lhs, Vector3 rhs)
    {
        return new Vector3(lhs.X + rhs.X, lhs.Y + rhs.Y, lhs.Z + rhs.Z);
    }

    /// <summary>
    /// Subtracts the left-hand side vector from the right-hand side.
    /// </summary>
    /// <param name="lhs">The left-hand side vector.</param>
    /// <param name="rhs">The right-hand side vector.</param>
    /// <returns>The resultant vector.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static Vector3 operator -(Vector3 lhs, Vector3 rhs)
    {
        return new Vector3(lhs.X - rhs.X, lhs.Y - rhs.Y, lhs.Z - rhs.Z);
    }

    /// <summary>
    /// Divides the left-hand side vector by the right-hand side.
    /// </summary>
    /// <param name="lhs">The left-hand side vector.</param>
    /// <param name="rhs">The right-hand side vector.</param>
    /// <returns>The resultant vector.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static Vector3 operator /(Vector3 lhs, Vector3 rhs)
    {
        return new Vector3(lhs.X/rhs.X, lhs.Y/rhs.Y, lhs.Z/rhs.Z);
    }

    /// <summary>
    /// Divides the left-hand side vector by the right-hand side.
    /// </summary>
    /// <param name="lhs">The left-hand side vector.</param>
    /// <param name="rhs">The right-hand side value.</param>
    /// <returns>The resultant Vector2.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static Vector3 operator /(Vector3 lhs, float rhs)
    {
        return new Vector3(lhs.X/rhs, lhs.Y/rhs, lhs.Z/rhs);
    }

    /// <summary>
    /// Multiplies the left-hand side vector by the right-hand side.
    /// </summary>
    /// <param name="lhs">The left-hand side vector.</param>
    /// <param name="rhs">The right-hand side vector.</param>
    /// <returns>The resultant vector.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static Vector3 operator *(Vector3 lhs, Vector3 rhs)
    {
        return new Vector3(lhs.X*rhs.X, lhs.Y*rhs.Y, lhs.Z*rhs.Z);
    }

    /// <summary>
    /// Multiples the left-hand side vector by the right-hand side.
    /// </summary>
    /// <param name="lhs">The left-hand side vector.</param>
    /// <param name="rhs">The right-hand side value.</param>
    /// <returns>The resultant vector.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static Vector3 operator *(Vector3 lhs, float rhs)
    {
        return new Vector3(lhs.X*rhs, lhs.Y*rhs, lhs.Z*rhs);
    }

    /// <summary>
    /// Checks if the left-hand side Vector2 is equal to the right-hand side.
    /// </summary>
    /// <param name="lhs">The left-hand side vector.</param>
    /// <param name="rhs">The right-hand side vector.</param>
    /// <returns>true, if both are equal; otherwise false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static bool operator ==(Vector3 lhs, Vector3 rhs)
    {
        return lhs.X == rhs.X && lhs.Y == rhs.Y && lhs.Z == rhs.Z;
    }

    /// <summary>
    /// Checks if the left-hand side vector is not equal to the right-hand side.
    /// </summary>
    /// <param name="lhs">The left-hand side vector.</param>
    /// <param name="rhs">The right-hand side vector.</param>
    /// <returns>true, if both are not equal; otherwise false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static bool operator !=(Vector3 lhs, Vector3 rhs)
    {
        return lhs.X != rhs.X || lhs.Y != rhs.Y || lhs.Z != rhs.Z;        
    }

    /// <summary>
    /// Checks if the object is equal to this vector.
    /// </summary>
    /// <param name="obj">The object to check against.</param>
    /// <returns>true, if both are equal; otherwise false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public override bool Equals(object obj)
    {
        return obj is Vector3 other && other == this;
    }

    /// <summary>
    /// Gets the dot product of two vector.
    /// </summary>
    /// <param name="lhs">The left-hand side vector.</param>
    /// <param name="rhs">The right-hand side vector.</param>
    /// <returns>The dot product.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static float Dot(Vector3 lhs, Vector3 rhs)
    {
        return (lhs.X * rhs.X) + (lhs.Y * rhs.Y) + (lhs.Z *  rhs.Z);
    }

    /// <summary>
    /// Gets the squared length of this vector. 
    /// </summary>
    /// <returns>The squared length of this vector.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public readonly float LengthSquared()
    {
        return Dot(this, this);
    }

    /// <summary>
    /// Gets the length of this vector.
    /// </summary>
    /// <returns>The length of this vector.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public readonly float Length()
    {
        float lengthSquared = LengthSquared();
        return MathF.Sqrt(lengthSquared);
    }

    /// <summary>
    /// Gets a vector with the same direction as the specified vector, but with a length of one.
    /// </summary>
    /// <param name="value">The vector to normalize.</param>
    /// <returns>The normalised vector.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static Vector3 Normalise(Vector3 value)
    {
        return value / value.Length();
    }

    /// <summary>
    /// Gets a vector with the same direction as the specified vector, but with a length of one.
    /// </summary>
    /// <returns>The normalised vector.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public Vector3 Normalise()
    {
        return this / Length();
    }

    /// <summary>
    /// Gets a vector with the signs of each value being flipped.
    /// </summary>
    /// <param name="vector">The vector to perform this unary operator to.</param>
    /// <returns>The resultant vector.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static Vector3 operator -(Vector3 vector)
    {
        return new Vector3(-vector.X, -vector.Y, -vector.Z);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public override int GetHashCode()
    {
        return X.GetHashCode() ^ Y.GetHashCode(); // XOR the two hash codes together.
    }
}