

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
    /// The height of this rectanlge.
    /// </summary>
    public float Height;

    /// <summary>
    /// Gets the x-coordinate of the left edge of this rectangle.
    /// </summary>
    public float Left => X;

    /// <summary>
    /// Gets the x-coordinate of the right edge of this rectangle.
    /// </summary>
    public float Right => X + Width;

    /// <summary>
    /// Gets the y-coordinate of the top edge of this rectangle.
    /// </summary>
    public float Bottom => Y;

    /// <summary>
    /// Gets the y-coordinate of the top edge of this rectangle.
    /// </summary>
    public float Top => Y + Height;

    /// <summary>
    /// Gets the top-left corner of this rectangle.
    /// </summary>
    public Vector2 TopLeft => new(X,Y);

    /// <summary>
    /// Gets the top-right corner of this rectangle.
    /// </summary>
    public Vector2 TopRight => new(X+Width,Y);

    /// <summary>
    /// Gets the bottom-left corner of this rectangle. 
    /// </summary>
    public Vector2 BottomLeft => new(X, Y-Height);

    /// <summary>
    /// Gets the bottom-right corner of this rectangle.
    /// </summary>
    public Vector2 BottomRight => new(X+Width, Y-Height);

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
        return 
            X.GetHashCode() ^
            Y.GetHashCode() ^
            Width.GetHashCode() ^ 
            Height.GetHashCode();
    }
}