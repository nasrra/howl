using System;
using System.Runtime.CompilerServices;

namespace Howl.Math;

public struct Vector2
{
    public static Vector2 NaN       = new(float.NaN, float.NaN);
    public static Vector2 Zero      = new(0,0);
    public static Vector2 One       = new(1,1);
    public static Vector2 Up        = new(0,1);
    public static Vector2 Down      = new(0,-1);
    public static Vector2 Left      = new(-1,0);
    public static Vector2 Right     = new(1,0);

    /// <summary>
    /// Gets and sets the x-coordinate value.
    /// </summary>
    public float X;

    /// <summary>
    /// Gets and sets the y-coordinate value.
    /// </summary>
    public float Y;

    /// <summary>
    /// Construcsts a vector.
    /// </summary>
    /// <param name="x">The x-coordinate value.</param>
    /// <param name="y">The y-coordinate value.</param>
    public Vector2(float x, float y)
    {
        X = x;
        Y = y;
    }

    /// <summary>
    /// Adds the left-hand side vector to the right-hand side.
    /// </summary>
    /// <param name="lhs">The left-hand side vector.</param>
    /// <param name="rhs">The right-hand side vector.</param>
    /// <returns>The resultant Vector2.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static Vector2 operator +(Vector2 lhs, Vector2 rhs)
    {
        return new Vector2(lhs.X + rhs.X, lhs.Y + rhs.Y);
    }

    /// <summary>
    /// Subtracts the left-hand side vector from the right-hand side.
    /// </summary>
    /// <param name="lhs">The left-hand side vector.</param>
    /// <param name="rhs">The right-hand side vector.</param>
    /// <returns>The resultant vector.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static Vector2 operator -(Vector2 lhs, Vector2 rhs)
    {
        return new Vector2(lhs.X - rhs.X, lhs.Y - rhs.Y);
    }

    /// <summary>
    /// Divides the left-hand side vector by the right-hand side.
    /// </summary>
    /// <param name="lhs">The left-hand side vector.</param>
    /// <param name="rhs">The right-hand side vector.</param>
    /// <returns>The resultant vector.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static Vector2 operator /(Vector2 lhs, Vector2 rhs)
    {
        return new Vector2(lhs.X/rhs.X, lhs.Y/rhs.Y);
    }

    /// <summary>
    /// Divides the left-hand side vector by the right-hand side.
    /// </summary>
    /// <param name="lhs">The left-hand side vector.</param>
    /// <param name="rhs">The right-hand side value.</param>
    /// <returns>The resultant vector.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static Vector2 operator /(Vector2 lhs, float rhs)
    {
        return new Vector2(lhs.X/rhs, lhs.Y/rhs);
    }

    /// <summary>
    /// Multiplies the left-hand side vector by the right-hand side.
    /// </summary>
    /// <param name="lhs">The left-hand side vector.</param>
    /// <param name="rhs">The right-hand side vector.</param>
    /// <returns>The resultant vector.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static Vector2 operator *(Vector2 lhs, Vector2 rhs)
    {
        return new Vector2(lhs.X*rhs.X, lhs.Y*rhs.Y);
    }

    /// <summary>
    /// Multiples the left-hand side vector by the right-hand side.
    /// </summary>
    /// <param name="lhs">The left-hand side vector.</param>
    /// <param name="rhs">The right-hand side value.</param>
    /// <returns>The resultant vector.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static Vector2 operator *(Vector2 lhs, float rhs)
    {
        return new Vector2(lhs.X*rhs, lhs.Y*rhs);
    }

    /// <summary>
    /// Checks if the left-hand side vector is equal to the right-hand side.
    /// </summary>
    /// <param name="lhs">The left-hand side vector.</param>
    /// <param name="rhs">The right-hand side vector.</param>
    /// <returns>true, if both are equal; otherwise false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static bool operator ==(Vector2 lhs, Vector2 rhs)
    {
        return lhs.X == rhs.X && lhs.Y == rhs.Y;
    }

    /// <summary>
    /// Checks if the left-hand side vector is not equal to the right-hand side.
    /// </summary>
    /// <param name="lhs">The left-hand side vector.</param>
    /// <param name="rhs">The right-hand side vector.</param>
    /// <returns>true, if both are not equal; otherwise false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static bool operator !=(Vector2 lhs, Vector2 rhs)
    {
        return lhs.X != rhs.X || lhs.Y != rhs.Y;        
    }

    /// <summary>
    /// Checks if the object is equal to this vector.
    /// </summary>
    /// <param name="obj">The object to check against.</param>
    /// <returns>true, if both are equal; otherwise false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public override bool Equals(object obj)
    {
        return obj is Vector2 other && other == this;
    }

    /// <summary>
    /// Gets the dot product between this vector and another.
    /// </summary>
    /// <param name="vector">The vector to dot product with.</param>
    /// <returns>The dot profduct value</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public readonly float Dot(Vector2 vector)
    {
        return Dot(this, vector);
    }

    /// <summary>
    /// Gets the dot product of two vector.
    /// </summary>
    /// <param name="lhs">The left-hand side vector.</param>
    /// <param name="rhs">The right-hand side vector.</param>
    /// <returns>The dot product.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static float Dot(Vector2 lhs, Vector2 rhs)
    {
        return (lhs.X * rhs.X) + (lhs.Y * rhs.Y);
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
    /// Gets the squared length of a vector.
    /// </summary>
    /// <param name="vector">The vector data.</param>
    /// <returns>The sqaured lenght of the vector.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static float LengthSquared(Vector2 vector)
    {
        return Dot(vector, vector);        
    }

    /// <summary>
    /// Gets the length of this vector.
    /// </summary>
    /// <returns>The length of this vector.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public readonly float Length()
    {
        return Length(this);
    }

    /// <summary>
    /// Gets the length of a vector.
    /// </summary>
    /// <param name="vector">The vector data.</param>
    /// <returns>The length of the vector.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static float Length(Vector2 vector)
    {        
        float lengthSquared = LengthSquared(vector);
        return MathF.Sqrt(lengthSquared);
    }

    /// <summary>
    /// Gets the inverse length of this vector.
    /// </summary>
    /// <returns>The inverse length of this vector.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public readonly float InverseLength()
    {
        return InverseLength(this);
    }

    /// <summary>
    /// Gets the inverse length of this vector.
    /// </summary>
    /// <returns>The inverse length of this vector.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static float InverseLength(Vector2 vector)
    {        
        return 1f / Length(vector);
    }

    /// <summary>
    /// Gets a vector with the same dirrection as the specified vector, but with a length of one.
    /// </summary>
    /// <returns>The normalised vector.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public Vector2 Normalise()
    {
        return Normalise(this);
    }

    /// <summary>
    /// Gets a vector with the same direction as the specified vector, but with a length of one.
    /// </summary>
    /// <param name="value">The vector to normalize.</param>
    /// <returns>The normalised vector.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static Vector2 Normalise(Vector2 value)
    {
        return value * value.InverseLength();
    }

    /// <summary>
    /// Gets a vector with the signs of each value being flipped.
    /// </summary>
    /// <param name="vector">The vector to perform this unary operator to.</param>
    /// <returns>The resultant vector.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static Vector2 operator -(Vector2 vector)
    {
        return new Vector2(-vector.X, -vector.Y);
    }

    /// <summary>
    /// Transforms this vector by the supplied transform.
    /// </summary>
    /// <param name="transform">The transform data to transform by.</param>
    /// <returns>The resultant vector.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public Vector2 Transform(Transform transform)
    {
        return Transform(this, transform); 
    }

    /// <summary>
    /// Transforms a vector by the supplied transform.
    /// </summary>
    /// <param name="vector">The vector to transform.</param>
    /// <param name="transform">The transform data to transform by.</param>
    /// <returns>The resultant vector.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static Vector2 Transform(Vector2 vector, Transform transform)
    {
        return Transform(vector.X, vector.Y, transform);
    }

    /// <summary>
    /// Transforms a vector by the supplied transform.
    /// </summary>
    /// <param name="x">the x-value of the vector.</param>
    /// <param name="y">the y-value of the vector.</param>
    /// <param name="transform">The transform data to transform by.</param>
    /// <returns>The resultant vector.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static Vector2 Transform(float x, float  y, Transform transform)
    {
        // NOTE:
        // This ordering: Scale -> Rotation -> Translation
        // should remain the same. It is pretty much Matrix math.

        // Scale:
        float sx = x * transform.Scale.X;
        float sy = y * transform.Scale.Y; 

        // Rotation:
        float rx = sx * transform.Cos - sy * transform.Sin;
        float ry = sx * transform.Sin + sy * transform.Cos;

        // Translation:
        float tx = rx + transform.Position.X;
        float ty = ry + transform.Position.Y;

        return new(tx, ty);
    }

    /// <summary>
    /// Gets the distance between this vector and another.
    /// </summary>
    /// <param name="vector">The vector to find distance against.</param>
    /// <returns>The distance to the specified vector.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public float Distance(Vector2 vector)
    {
        return Distance(this, vector);
    }

    /// <summary>
    /// Gets the distance between two vectors
    /// </summary>
    /// <param name="from">The vector to start at.</param>
    /// <param name="to">The vector to end at.</param>
    /// <returns>The distance.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static float Distance(Vector2 from, Vector2 to)
    {
        float dx = from.X - to.X;
        float dy = from.Y - to.Y;
        return MathF.Sqrt(dx * dx + dy * dy);
    }

    /// <summary>
    /// Gets the distance squared between this vector and another.
    /// </summary>
    /// <param name="vector">The vector to find distance sqaured against.</param>
    /// <returns>The distance squared to the specified vector.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public float DistanceSquared(Vector2 vector)
    {
        return DistanceSquared(this, vector);
    }

    /// <summary>
    /// Gets the distance squared between two vectors.
    /// </summary>
    /// <param name="from">The vector to start at.</param>
    /// <param name="to">Teh vector to end at.</param>
    /// <returns>The distance squared.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static float DistanceSquared(Vector2 from, Vector2 to)
    {
        float dx = from.X - to.X;
        float dy = from.Y - to.Y;
        return dx * dx + dy * dy;
    }

    /// <summary>
    /// Gets the cross product between to vectors.
    /// </summary>
    /// <param name="lhs">The left-hand side vector.</param>
    /// <param name="rhs">The right-hand side vector.</param>
    /// <returns>The cross product.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public float Cross(Vector2 lhs, Vector2 rhs)
    {
        return lhs.X * rhs.Y - lhs.Y * rhs.X;    
    }   

    /// <summary>
    /// Inverts the y-value of this vector.
    /// </summary>
    /// <returns>The resultant vector.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public Vector2 InvertY()
    {
        return InvertY(this);
    }

    /// <summary>
    /// Inverts the y-value of a vector.
    /// </summary>
    /// <param name="vector">The vector to invert.</param>
    /// <returns>The resultant vector.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static Vector2 InvertY(Vector2 vector)
    {
        return new Vector2(vector.X, -vector.Y);
    }

    /// <summary>
    /// Inverts the x-value of this vector.
    /// </summary>
    /// <returns>The resultant vector.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public Vector2 InvertX()
    {
        return InvertX(this);
    }

    /// <summary>
    /// Inverts the x-value of a vector.
    /// </summary>
    /// <param name="vector">The vector to invert.</param>
    /// <returns>The resultant vector.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static Vector2 InvertX(Vector2 vector)
    {
        return new Vector2(-vector.X, vector.Y);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public override int GetHashCode()
    {
        return X.GetHashCode() ^ Y.GetHashCode(); // XOR the two hash codes together.
    }
}