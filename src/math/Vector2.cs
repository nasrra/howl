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

    public static Vector2 operator +(Vector2 lhs, Vector2 rhs)
    {
        return new Vector2(lhs.X + rhs.X, lhs.Y + rhs.Y);
    }

    public static Vector2 operator -(Vector2 lhs, Vector2 rhs)
    {
        return new Vector2(lhs.X - rhs.X, lhs.Y - rhs.Y);
    }

    public static Vector2 operator /(Vector2 lhs, Vector2 rhs)
    {
        return new Vector2(lhs.X/rhs.X, lhs.Y/rhs.Y);
    }

    public static Vector2 operator *(Vector2 lhs, Vector2 rhs)
    {
        return new Vector2(lhs.X*rhs.X, lhs.Y*rhs.Y);
    }

    public static bool operator ==(Vector2 lhs, Vector2 rhs)
    {
        return lhs.X == rhs.X && lhs.Y == rhs.Y;
    }

    public static bool operator !=(Vector2 lhs, Vector2 rhs)
    {
        return lhs.X != rhs.X || lhs.Y != rhs.Y;        
    }

    public override bool Equals(object obj)
    {
        return obj is Vector2 other && other == this;
    }

    public override int GetHashCode()
    {
        return X.GetHashCode() ^ Y.GetHashCode(); // XOR the two hash codes together.
    }
}