using System;
using System.Runtime.CompilerServices;

namespace Howl.Math.Shapes;

public struct Rectangle
{

    /// <summary>
    /// Gets and sets the x-coordinate of the origin point.
    /// </summary>
    public float X;

    /// <summary>
    /// Gets and set the y-coordinate of the origin point.
    /// </summary>
    public float Y;

    /// <summary>
    /// The width of this rectangle.
    /// </summary>
    public float Width;

    /// <summary>
    /// The height of this rectangle.
    /// </summary>
    public float Height;

    /// <summary>
    /// Constructs a Rectangle.
    /// </summary>
    /// <param name="x">The x-coordinate of the origin point.</param>
    /// <param name="y">The y-coordinate of the origin point.</param>
    /// <param name="width">The width of this rectangle.</param>
    /// <param name="height">The height of this rectangle.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public Rectangle(float x, float y, float width, float height)
    {
        X = x;
        Y = y;
        Width = width;
        Height = height;
    }

    /// <summary>
    /// Constructs a Rectangle.
    /// </summary>
    /// <param name="min">The minimum vector.</param>
    /// <param name="max">The maximum vector.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public Rectangle(Vector2 min, Vector2 max)
    {
        X = min.X;
        Y = max.Y;
        Width = max.X - min.X;
        Height = max.Y - min.Y;
    }

    /// <summary>
    /// Adds the left-hand side rectangle to the right-hand side.
    /// </summary>
    /// <param name="lhs">The left-hand side rectangle.</param>
    /// <param name="rhs">The right-hand side rectangle.</param>
    /// <returns>The result as a Rectangle struct.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static Rectangle operator +(Rectangle lhs, Rectangle rhs)
    {
        return new Rectangle(
            lhs.X + rhs.X,
            lhs.Y + rhs.Y,
            lhs.Width + rhs.Width,
            lhs.Height + rhs.Height
        );
    }

    /// <summary>
    /// Subtracts the left-hand side rectangle to the right-hand side.
    /// </summary>
    /// <param name="lhs">The left-hand side rectangle.</param>
    /// <param name="rhs">The right-hand side rectangle.</param>
    /// <returns>The result as a Rectangle struct.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static Rectangle operator -(Rectangle lhs, Rectangle rhs)
    {
        return new Rectangle(
            lhs.X - rhs.X,
            lhs.Y - rhs.Y,
            lhs.Width - rhs.Width,
            lhs.Height - rhs.Height
        );
    }

    /// <summary>
    /// Multiplies the left-hand side rectangle to the right-hand side.
    /// </summary>
    /// <param name="lhs">The left-hand side rectangle.</param>
    /// <param name="rhs">The right-hand side rectangle.</param>
    /// <returns>The result as a Rectangle struct.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static Rectangle operator *(Rectangle lhs, Rectangle rhs)
    {
        return new Rectangle(
            lhs.X * rhs.X,
            lhs.Y * rhs.Y,
            lhs.Width * rhs.Width,
            lhs.Height * rhs.Height
        );
    }

    /// <summary>
    /// Divides the left-hand side rectangle to the right-hand side.
    /// </summary>
    /// <param name="lhs">The left-hand side rectangle.</param>
    /// <param name="rhs">The right-hand side rectangle.</param>
    /// <returns>The result as a Rectangle struct.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static Rectangle operator /(Rectangle lhs, Rectangle rhs)
    {
        return new Rectangle(
            lhs.X / rhs.X,
            lhs.Y / rhs.Y,
            lhs.Width / rhs.Width,
            lhs.Height / rhs.Height
        );
    }

    /// <summary>
    /// Checks if the left-hand side rectangle is equal to the right-hand side.
    /// </summary>
    /// <param name="lhs">The left-hand side rectangle.</param>
    /// <param name="rhs">The right-hand side rectangle.</param>
    /// <returns>true, if equal; otherwise false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static bool operator ==(Rectangle lhs, Rectangle rhs)
    {
        return 
            lhs.X == rhs.X &&
            lhs.Y == rhs.Y &&
            lhs.Width == rhs.Width &&
            lhs.Height == rhs.Height; 
    }

    /// <summary>
    /// Checks if the left-hand side rectangle is not equal to the right-hand side.
    /// </summary>
    /// <param name="lhs">The left-hand side rectangle.</param>
    /// <param name="rhs">The right-hand side rectangle.</param>
    /// <returns>true, if not equal; otherwise false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static bool operator !=(Rectangle lhs, Rectangle rhs)
    {
        return 
            lhs.X != rhs.X ||
            lhs.Y != rhs.Y ||
            lhs.Width != rhs.Width ||
            lhs.Height != rhs.Height; 
    }

    /// <summary>
    /// Checks if the object is equal to this Rectangle.
    /// </summary>
    /// <param name="obj">The object to check equality against.</param>
    /// <returns>true, if the two objects are equal otherwise false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public override bool Equals(object obj)
    {
        return obj is Rectangle other && other == this; 
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public override int GetHashCode()
    {   
        return HashCode.Combine(X,Y,Height,Width);
    }

    /// <summary>
    /// Gets the x-coordinate of the left edge of a rectangle.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static float Left(in Rectangle rectangle)
    {
        return rectangle.X;
    }

    /// <summary>
    /// Gets the x-coordinate of the right edge of a rectangle.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static float Right(in Rectangle rectangle)
    {
        return rectangle.X + rectangle.Width;   
    }

    /// <summary>
    /// Gets the y-coordinate of the top edge of a rectangle.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static float Bottom(in Rectangle rectangle)
    {
        return rectangle.Y- rectangle.Height;
    }

    /// <summary>
    /// Gets the y-coordinate of the top edge of a rectangle.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static float Top(in Rectangle rectangle)
    {
        return rectangle.Y;   
    }

    /// <summary>
    /// Gets the top-left corner of a rectangle.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static Vector2 TopLeft(in Rectangle rectangle)
    {
        return new Vector2(Left(rectangle),Top(rectangle));    
    }
    

    /// <summary>
    /// Gets the top-right corner of a rectangle.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static Vector2 TopRight(in Rectangle rectangle)
    {
        return new Vector2(Right(rectangle),Top(rectangle));    
    }
    

    /// <summary>
    /// Gets the bottom-left corner of a rectangle. 
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static Vector2 BottomLeft(in Rectangle rectangle)
    {
        return new Vector2(Left(rectangle), Bottom(rectangle));    
    }
    

    /// <summary>
    /// Gets the bottom-right corner of a rectangle.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static Vector2 BottomRight(in Rectangle rectangle)
    {
        return new(Right(rectangle), Bottom(rectangle));   
    }

    /// <summary>
    /// Gets the center position of a rectangle.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static Vector2 Center(in Rectangle rectangle)
    {
        return new Vector2(Left(rectangle) + (rectangle.Width * 0.5f), Top(rectangle) - (rectangle.Height * 0.5f));   
    }

    /// <summary>
    /// Constructs and gets the AABB of a rectangle.
    /// </summary>
    /// <param name="rectangle">the rectangle.</param>
    /// <returns>the constructed AABB.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static AABB GetAABB(in Rectangle rectangle)
    {
        return new AABB(BottomLeft(rectangle), TopRight(rectangle));
    }

    /// <summary>
    /// Constructs a rectangle that has a rectangle's width and height scaled by a vector.
    /// </summary>
    /// <param name="rectangle">the rectangle to scale.</param>
    /// <param name="scale">the vector to scale by.</param>
    /// <returns>the resultant rectangle.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static Rectangle Scale(in Rectangle rectangle, Vector2 scale)
    {
        return new Rectangle(
            rectangle.X, 
            rectangle.Y, 
            rectangle.Width * scale.X, 
            rectangle.Height * scale.Y
        );
    }

    /// <summary>
    /// Constructs a rectangle that has a rectangle's width and height scaled by a value.
    /// </summary>
    /// <param name="scale">the value to scale by </param>
    /// <returns>the resultant rectangle.</returns>
    public static Rectangle Scale(in Rectangle rectangle, float scale)
    {
        return new Rectangle(
            rectangle.X,
            rectangle.Y, 
            rectangle.Width * scale,
            rectangle.Height * scale
        );
    }
}