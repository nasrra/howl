using Howl.Unmanaged.Collections;

namespace N_Howl.N_Math;

public unsafe struct Matrix4x4{

    public fixed float M[16];

    public static Matrix4x4 operator*(
        Matrix4x4 lhs, Matrix4x4 rhs
    ){
        Matrix4x4 dst = default;
        float* d = dst.M;
        float* l = lhs.M;
        float* r = rhs.M;

        d[0]  = (l[0] * r[0]) + (l[4] * r[1]) + (l[8]  * r[2]) + (l[12] * r[3]);
        d[1]  = (l[1] * r[0]) + (l[5] * r[1]) + (l[9]  * r[2]) + (l[13] * r[3]);
        d[2]  = (l[2] * r[0]) + (l[6] * r[1]) + (l[10] * r[2]) + (l[14] * r[3]);
        d[3]  = (l[3] * r[0]) + (l[7] * r[1]) + (l[11] * r[2]) + (l[15] * r[3]);

        d[4]  = (l[0] * r[4]) + (l[4] * r[5]) + (l[8]  * r[6]) + (l[12] * r[7]);
        d[5]  = (l[1] * r[4]) + (l[5] * r[5]) + (l[9]  * r[6]) + (l[13] * r[7]);
        d[6]  = (l[2] * r[4]) + (l[6] * r[5]) + (l[10] * r[6]) + (l[14] * r[7]);
        d[7]  = (l[3] * r[4]) + (l[7] * r[5]) + (l[11] * r[6]) + (l[15] * r[7]);

        d[8]  = (l[0] * r[8]) + (l[4] * r[9]) + (l[8]  * r[10]) + (l[12] * r[11]);
        d[9]  = (l[1] * r[8]) + (l[5] * r[9]) + (l[9]  * r[10]) + (l[13] * r[11]);
        d[10] = (l[2] * r[8]) + (l[6] * r[9]) + (l[10] * r[10]) + (l[14] * r[11]);
        d[11] = (l[3] * r[8]) + (l[7] * r[9]) + (l[11] * r[10]) + (l[15] * r[11]);

        d[12] = (l[0] * r[12]) + (l[4] * r[13]) + (l[8]  * r[14]) + (l[12] * r[15]);
        d[13] = (l[1] * r[12]) + (l[5] * r[13]) + (l[9]  * r[14]) + (l[13] * r[15]);
        d[14] = (l[2] * r[12]) + (l[6] * r[13]) + (l[10] * r[14]) + (l[14] * r[15]);
        d[15] = (l[3] * r[12]) + (l[7] * r[13]) + (l[11] * r[14]) + (l[15] * r[15]);

        return dst;
    }
} 

public unsafe struct Matrix3x3{
    public fixed float M[9];
}

public struct Vector3{
    
    public readonly static Vector3 Right = new(){X = 1, Y = 0, Z = 0};
    public readonly static Vector3 Up = new(){X = 0, Y = 1, Z = 0};
    public readonly static Vector3 Forward = new(){X = 0, Y = 0, Z = 1};

    public float X;
    public float Y;
    public float Z;

    public static Vector3 operator- (
        Vector3 lhs, Vector3 rhs
    ){
        lhs.X -= rhs.X;
        lhs.Y -= rhs.Y;
        lhs.Z -= rhs.Z;
        return lhs;
    }

    public static Vector3 operator+ (
        Vector3 lhs, Vector3 rhs
    ){
        lhs.X += rhs.X;
        lhs.Y += rhs.Y;
        lhs.Z += rhs.Z;
        return lhs;
    }

    public static Vector3 operator* (
        Vector3 lhs, Vector3 rhs
    ){
        lhs.X *= rhs.X;
        lhs.Y *= rhs.Y;
        lhs.Z *= rhs.Z;
        return lhs;
    }

    public static Vector3 operator/ (
        Vector3 lhs, Vector3 rhs
    ){
        lhs.X /= rhs.X;
        lhs.Y /= rhs.Y;
        lhs.Z /= rhs.Z;
        return lhs;
    }

    public static Vector3 operator+ (
        Vector3 lhs, Vector2 rhs
    ){
        lhs.X += rhs.X;
        lhs.Y += rhs.Y;
        return lhs;
    }

    public static Vector3 operator- (
        Vector3 lhs, Vector2 rhs
    ){
        lhs.X -= rhs.X;
        lhs.Y -= rhs.Y;
        return lhs;
    }

    public static Vector3 operator* (
        Vector3 lhs, Vector2 rhs
    ){
        lhs.X *= rhs.X;
        lhs.Y *= rhs.Y;
        return lhs;
    }

    public static Vector3 operator/ (
        Vector3 lhs, Vector2 rhs
    ){
        lhs.X /= rhs.X;
        lhs.Y /= rhs.Y;
        return lhs;
    }

    public static Vector3 operator+ (
        Vector3 lhs, Vector2I rhs
    ){
        lhs.X += rhs.X;
        lhs.Y += rhs.Y;
        return lhs;
    }

    public static Vector3 operator- (
        Vector3 lhs, Vector2I rhs
    ){
        lhs.X -= rhs.X;
        lhs.Y -= rhs.Y;
        return lhs;
    }

    public static Vector3 operator* (
        Vector3 lhs, Vector2I rhs
    ){
        lhs.X *= rhs.X;
        lhs.Y *= rhs.Y;
        return lhs;
    }

    public static Vector3 operator/ (
        Vector3 lhs, Vector2I rhs
    ){
        lhs.X /= rhs.X;
        lhs.Y /= rhs.Y;
        return lhs;
    }

    public static Vector3 operator*(
        Vector3 lhs, float rhs
    ){
        lhs.X *= rhs;
        lhs.Y *= rhs;
        lhs.Z *= rhs;
        return lhs;
    }

    public static Vector3 operator/(
        Vector3 lhs, float rhs
    ){
        lhs.X /= rhs;
        lhs.Y /= rhs;
        lhs.Z /= rhs;
        return lhs;
    }

    public static Vector3 operator-(
        Vector3 v
    ){
        v.X = -v.X;
        v.Y = -v.Y;
        return v;
    }
}

public struct Vector2{

    public float X;
    public float Y;

    public static Vector2 operator+ (
        Vector2 lhs, Vector2 rhs
    ){
        lhs.X += rhs.X;
        lhs.Y += rhs.Y;
        return lhs;
    }

    public static Vector2 operator- (
        Vector2 lhs, Vector2 rhs
    ){
        lhs.X -= rhs.X;
        lhs.Y -= rhs.Y;
        return lhs;
    }

    public static Vector2 operator* (
        Vector2 lhs, Vector2 rhs
    ){
        lhs.X *= rhs.X;
        lhs.Y *= rhs.Y;
        return lhs;
    }

    public static Vector2 operator/ (
        Vector2 lhs, Vector2 rhs
    ){
        lhs.X /= rhs.X;
        lhs.Y /= rhs.Y;
        return lhs;
    }

    public static Vector2 operator+ (
        Vector2 lhs, Vector3 rhs
    ){
        lhs.X += rhs.X;
        lhs.Y += rhs.Y;
        return lhs;
    }

    public static Vector2 operator- (
        Vector2 lhs, Vector3 rhs
    ){
        lhs.X -= rhs.X;
        lhs.Y -= rhs.Y;
        return lhs;
    }

    public static Vector2 operator* (
        Vector2 lhs, Vector3 rhs
    ){
        lhs.X *= rhs.X;
        lhs.Y *= rhs.Y;
        return lhs;
    }

    public static Vector2 operator/ (
        Vector2 lhs, Vector3 rhs
    ){
        lhs.X /= rhs.X;
        lhs.Y /= rhs.Y;
        return lhs;
    }

    public static Vector2 operator*(
        Vector2 lhs, float rhs
    ){
        lhs.X *= rhs;
        lhs.Y *= rhs;
        return lhs;
    }

    public static Vector2 operator/(
        Vector2 lhs, float rhs
    ){
        lhs.X /= rhs;
        lhs.Y /= rhs;
        return lhs;
    }

}

public struct Vector2I{
    public int X;
    public int Y;

    public static Vector2I operator+ (Vector2I lhs, Vector2I rhs){
        return new(){X = lhs.X + rhs.X, Y = lhs.Y + rhs.X};
    }

    public static Vector2I operator- (Vector2I lhs, Vector2I rhs){
        return new(){X = lhs.X - rhs.X, Y = lhs.Y - rhs.Y};
    }

    public static bool operator==(Vector2I lhs, Vector2I rhs){
        return lhs.X == rhs.X && lhs.Y == rhs.Y;
    }

    public static bool operator!=(Vector2I lhs, Vector2I rhs){
        return !(lhs.X == rhs.X);
    }

    public override bool Equals(object o){
        return o is Vector2I v && v == this;
    }

    public override int GetHashCode(){
        return base.GetHashCode();
    }
}

public struct Vector2UI{
    public uint X;
    public uint Y;

    public static Vector2UI operator+ (Vector2UI lhs, Vector2UI rhs){
        return new(){X = lhs.X + rhs.X, Y = lhs.Y + rhs.X};
    }

    public static Vector2UI operator- (Vector2UI lhs, Vector2UI rhs){
        return new(){X = lhs.X - rhs.X, Y = lhs.Y - rhs.Y};
    }
}

public struct Quaternion{
    public float X;
    public float Y;
    public float Z;
    public float W;

    public static readonly Quaternion Identity = new(){W = 1};
}

public struct Transform{
    public Quaternion Rotation;
    public Vector3 Position;
    public Vector3 Scale;
    public static readonly Transform Identity = new(){Scale = new(){X = 1, Y = 1, Z = 1}}; 
}

public struct Transform2D{
    public Vector2 Position;
    public Vector2 Scale;
    public float Sine;
    public float Cosine;
    public float RotationRadians;
    public static readonly Transform2D Identity = new(){Scale = new(){X = 1, Y = 1}, Cosine = 1, Sine = 0}; 
}

public struct Rectangle{
    public float X;
    public float Y;
    public float Width;
    public float Height;
}

public struct Circle{
    public float X;
    public float Y;
    public float Radius;
}

/// <summary>
///     Fixed-Stride Structure-of-Arrays Vector2.
/// </summary>
public struct FsSoa_Vector2{
    /// <remarks>
    ///    <para>Remarks:</para>
    ///    <para>Elements are accessed via <c>entryElementIndex</c>.</para>
    /// </remarks>
    public Array<float> X;
    /// <remarks>
    ///    <para>Remarks:</para>
    ///    <para>Elements are accessed via <c>entryElementIndex</c>.</para>
    /// </remarks>
    public Array<float> Y;
    /// <remarks>
    ///    <para>Remarks:</para>
    ///    <para>Elements are accessed via <c>entryIndex</c>.</para>
    /// </remarks>
    public Array<int> AppendCounts;
    /// <summary>
    ///     The fixed stride of each entry.
    /// </summary>
    public int EntryStride;
    /// <summary>
    ///     The amount of entries this collection can hold.
    /// </summary>
    public int MaxEntries;
    public bool IsIntialised;
}

public struct Soa_Transform2D{
    public Soa_Vector2 Positions;
    public Soa_Vector2 Scales;
    public Array<float> Sines;
    public Array<float> Cosines;
    public Array<float> RotationRadians;
    public bool IsInitialised;
}

public struct Soa_Vector2{
    public Array<float> X;
    public Array<float> Y;
    /// <summary>
    ///     The count of allocated entries from appending.
    /// </summary>
    public int AppendCount;
    /// <summary>
    ///     The length of all the backing arrays of this instance.
    /// </summary>
    public int Length;
    public bool IsIntialised; 
}

public struct Soa_Aabb{
    /// <summary>
    ///     The x-components of the minimum vertex.
    /// </summary>
    public Array<float> MinX;
    /// <summary>
    ///     the y-components of the minimum vertex.
    /// </summary>
    public Array<float> MinY;
    /// <summary>
    /// the x-components of the maximum vertex.
    /// </summary>
    public Array<float> MaxX;
    /// <summary>
    ///     The y-components of the maximum vertex.
    /// </summary>
    public Array<float> MaxY;
    /// <summary>
    /// The count of allocated entries from appending.
    /// </summary>
    public int AppendCount;
    /// <summary>
    /// The length of all the backing arrays of this instance.
    /// </summary>
    public int Length;
    public bool IsIntialised;
}

public struct Aabb{
    public float MinX;
    public float MinY;
    public float MaxX;
    public float MaxY;

    public static Aabb operator +(
        Aabb aabb, Vector2 vector
    ){
        aabb.MinX += vector.X;
        aabb.MinY += vector.Y;
        aabb.MaxX += vector.X;
        aabb.MaxY += vector.Y;
        return aabb;
    }

    public static Aabb operator -(
        Aabb aabb, Vector2 vector
    ){
        aabb.MinX -= vector.X;
        aabb.MinY -= vector.Y;
        aabb.MaxX -= vector.X;
        aabb.MaxY -= vector.Y;
        return aabb;
    }

    public static bool operator ==(
        Aabb a, Aabb b
    ){
        return 
        a.MinX == b.MinX 
        && a.MinY == b.MinY
        && a.MaxX == b.MaxX
        && a.MaxY == b.MaxY;   
    }

    public static bool operator !=(
        Aabb a, Aabb b
    ){
        return 
        a.MinX != b.MinX
        || a.MinY != b.MinY 
        || a.MaxX != b.MaxX
        || a.MaxY != b.MaxY;
    }

    public override bool Equals(
        object obj
    ){
        return obj is Aabb other && this == other;
    }

    public override int GetHashCode(
        
    ){
        return base.GetHashCode();
    }
}

public unsafe struct PolygonRectangle{
    /// <summary>
    /// The maximum amount of vertices a PolygonRectangle can store.
    /// </summary>
    public const int VerticesLength = 4;
    public fixed float VerticesX[VerticesLength];
    public fixed float VerticesY[VerticesLength];
}
