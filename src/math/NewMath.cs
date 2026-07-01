using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using N_Howl.N_Math;
using Howl;

namespace N_Howl.N_Math;
public unsafe static class Math{

public const float Pi = 3.1415926535897932384626433f;
public const float Tau = 6.283185307179586f;
public const float OneSixth = 1.0f / 6.0f;
public const float OneTwentyFourth = 1.0f / 24.0f;

[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
public static float ToRadians(float degrees){
    // PI / 180 = 0.0174532925199f
    return degrees * 0.0174532925199f;
}

[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
public static float Sqrt(float value){
    return System.MathF.Sqrt(value);
}

[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
public static float Cos(float radians){
    return System.MathF.Cos(radians);
}

[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
public static float Sin(float radians){
    return System.MathF.Sin(radians);
}

[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
public static float Tan(float value){
    return System.MathF.Tan(value);
}

[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
public static float Dot(Vector3 lhs, Vector3 rhs){
    return (lhs.X * rhs.X) + (lhs.Y * rhs.Y) + (lhs.Z * rhs.Z);
}

[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
public static float Abs(
    float value
){
    return value<0? -value : value;
}

[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
public static float LengthSquared(
    Vector3 vector
){
    return (vector.X * vector.X) + (vector.Y * vector.Y) + (vector.Z * vector.Z);
}

[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
public static float Length(
    Vector3 vector
){
    return Sqrt(LengthSquared(vector));
}

[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
public static int LengthSquared(Vector2I vector){
    return (vector.X * vector.X) + (vector.Y * vector.Y);
}

[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
public static Vector3 Cross(Vector3 a, Vector3 b){
    return new(){
        X = (a.Y * b.Z) - (a.Z * b.Y),
        Y = (a.Z * b.X) - (a.X * b.Z),
        Z = (a.X * b.Y) - (a.Y * b.X)
    };
}

/// <summary>
/// Clamps a value between a min and max
/// </summary>
/// <remarks>
/// Note: Min and max are both invlusive.
/// </remarks>
/// <param name="value">the value to clamp.</param>
/// <param name="min">the min value.</param>
/// <param name="max">the max value.</param>
/// <returns></returns>
[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
public static T Clamp<T>(T value, T min, T max) where T : System.Numerics.INumber<T>
{
    Debug.Assert(min <= max, "min is greater than max!");

    if(value <= min)
    {
        return min;
    }
    else if(value >= max)
    {
        return max;
    }

    return value;
}

[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
public static T Max<T>(
    T a, T b
) where T : System.Numerics.INumber<T> {
    return a > b? a : b;
}

[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
public static Vector3 Normalise(Vector3 vector){
    float sqrd = LengthSquared(vector);
    if(sqrd > 0){
        float invLength = 1.0f / Sqrt(sqrd);
        vector.X *= invLength;
        vector.Y *= invLength;
        vector.Z *= invLength;
        return vector;
    }
    return default;
}

public static Matrix4x4 IdentityMatrix()
{
    Matrix4x4 result = default;
    float* m = result.M;
    m[0]  = 1.0f;
    m[5]  = 1.0f;
    m[10] = 1.0f; 
    m[15] = 1.0f; 
    return result;
}

/// <summary>
/// Computes a Left-Handed View Matrix (Column-Major)
/// </summary>
public static Matrix4x4 CreateLookAt(Vector3 cameraPos, Vector3 lookAtPos, Vector3 worldUpDir)
{
    // Left-Handed forward: Positive Z goes forward into the screen
    Vector3 forward = Normalise(lookAtPos - cameraPos);

    // find the local space right and up directions of the camera.
    Vector3 right = default;
    right.X = (worldUpDir.Y * forward.Z) - (worldUpDir.Z * forward.Y);
    right.Y = (worldUpDir.Z * forward.X) - (worldUpDir.X * forward.Z); 
    right.Z = (worldUpDir.X * forward.Y) - (worldUpDir.Y * forward.X);
    right = Normalise(right);
    Vector3 up = default;
    up.X = (forward.Y * right.Z) - (forward.Z * right.Y);
    up.Y = (forward.Z * right.X) - (forward.X * right.Z);
    up.Z = (forward.X * right.Y) - (forward.Y * right.X);
    up = Normalise(up);

    Matrix4x4 result = default;
    float* m = result.M;

    // Column 0: Right Axis
    m[0] = right.X; 
    m[1] = right.Y; 
    m[2] = right.Z; 

    // Column 1: Up Axis
    m[4] = up.X; 
    m[5] = up.Y; 
    m[6] = up.Z; 

    // Column 2: Forward Axis
    m[8]  = forward.X; 
    m[9]  = forward.Y; 
    m[10] = forward.Z; 

    // Column 3: Calculate the translation; how far along the local-space axis the camera is, and move all objects relative to that.
    // note that is accounts for the camera rotation as well.
    m[12] = -Dot(right, cameraPos);
    m[13] = -Dot(up, cameraPos);
    m[14] = -Dot(forward, cameraPos);
    m[15] = 1.0f;

    return result;
}

/// <summary>
/// Computes a Left-Handed Perspective Matrix mapping depth to Vulkan 0..1 (Column-Major)
/// </summary>
public static Matrix4x4 CreatePerspective(float fovYRadians, float aspectRatio, float zNear, float zFar)
{

    /**
        Calculate half of the vertical line that describes the length from the bottom to the top of out total vertical viewing angle;
        
        Note:
            think of it like the fovYRadians is how much your eye (or camera lens) sees, 
            the tangent describes the length from the top to bot.
    **/
    float tanHalfFovY = Tan(fovYRadians * 0.5f);
    /**
        bring the half-total vertical viewing area into window-space coordinates:

        Window Space:
            X = -1 (left) to 1 (right).
            Y = -1 (bottom) to 1 (top).
            Z = 0 (clost) to 1 (far).
    **/
    float g = 1.0f / tanHalfFovY;

    Matrix4x4 result = default;
    float* m = result.M;

    /**
        shrink the x-scaling of objects relative to the vertical viewing area to ensure that they arent 
        elongated along the x-axis of the window due to window size differences.
    **/
    m[0] = g / aspectRatio;
    
    /**
        set the y-scaling of objects to be relative to the vertical view of the camera; 
        squishing them down along their y-axis to fit them on the window.
    **/
    m[5] = g;

    /**
        unlike orthographic projection where depth mapping is perfectly linear, a perspective matrix mapping must compress
        depth non-linearly. More precision is allocated to objects close to the lens (zNear), while precision is agressively
        compressed as objects move toward the background (zFar) to preven distant objects from flickering (z-fighting).
        note that it is also 0-1 for modern graphics APIS (metal, vulkan, directX12) unlike opengl (-1 to 0).
    **/
    m[10] = zFar / (zFar - zNear);
    // add the world position of an object to its final 'w' value: perspective divsion (final 'w' division). 
    m[11] = 1.0f;

    /**
        This shears the z-values of objects backward, transforming them into window-space coordinates. this "shearing" backwards 
        is required as perspective projection has a plane that rendering is relative to (zNear) which must be above zero; otherwise
        there would be divide by zero issues. So when perspective division happens, we need to offset/shear the plane back to zero to
        ensure that our depth values are correct; offseting m[11]'s added value so its relative to the origin (0,0) and not the plane.  
    **/
    m[14] = -(zFar * zNear) / (zFar - zNear); 

    return result;
}

/// <summary>
/// Computes a Left-Handed Rotation Matrix applied to a source matrix (Column-Major)
/// </summary>
public static Matrix4x4 Rotate(Matrix4x4 src, float radians, Vector3 axis)
{
    // how much the of the objects original orientation is kept along its original axis.
    float c = Cos(radians);
    // how much of the objects orientation is shifted perpendicularly into a new direction.
    float s = Sin(radians);
    
    axis = Normalise(axis);

    float tempX = (1.0f - c) * axis.X;
    float tempY = (1.0f - c) * axis.Y;
    float tempZ = (1.0f - c) * axis.Z;

    // Left-Handed basis rotation vectors
    Matrix3x3 rot = default;
    float* pRot = rot.M;
    pRot[0] = c + tempX * axis.X;
    pRot[1] = tempX * axis.Y + s * axis.Z;
    pRot[2] = tempX * axis.Z - s * axis.Y;
    pRot[3] = tempY * axis.X - s * axis.Z;
    pRot[4] = c + tempY * axis.Y;
    pRot[5] = tempY * axis.Z + s * axis.X;
    pRot[6] = tempZ * axis.X + s * axis.Y;
    pRot[7] = tempZ * axis.Y - s * axis.X;
    pRot[8] = c + tempZ * axis.Z;

    Matrix4x4 dst = default;

    float* pSrc = src.M;
    float* pDst = dst.M;

    // Column 0 concatenation
    pDst[0] = (pSrc[0] * pRot[0]) + (pSrc[4] * pRot[3]) + (pSrc[8]   * pRot[6]);
    pDst[1] = (pSrc[1] * pRot[0]) + (pSrc[5] * pRot[3]) + (pSrc[9]   * pRot[6]);
    pDst[2] = (pSrc[2] * pRot[0]) + (pSrc[6] * pRot[3]) + (pSrc[10]  * pRot[6]);
    pDst[3] = (pSrc[3] * pRot[0]) + (pSrc[7] * pRot[3]) + (pSrc[11]  * pRot[6]);

    // Column 1 concatenation
    pDst[4] = (pSrc[0] * pRot[1]) + (pSrc[4] * pRot[4]) + (pSrc[8]  * pRot[7]);
    pDst[5] = (pSrc[1] * pRot[1]) + (pSrc[5] * pRot[4]) + (pSrc[9]  * pRot[7]);
    pDst[6] = (pSrc[2] * pRot[1]) + (pSrc[6] * pRot[4]) + (pSrc[10] * pRot[7]);
    pDst[7] = (pSrc[3] * pRot[1]) + (pSrc[7] * pRot[4]) + (pSrc[11] * pRot[7]);

    // Column 2 concatenation
    pDst[8]  = (pSrc[0] * pRot[2]) + (pSrc[4] * pRot[5]) + (pSrc[8]  * pRot[8]);
    pDst[9]  = (pSrc[1] * pRot[2]) + (pSrc[5] * pRot[5]) + (pSrc[9]  * pRot[8]);
    pDst[10] = (pSrc[2] * pRot[2]) + (pSrc[6] * pRot[5]) + (pSrc[10] * pRot[8]);
    pDst[11] = (pSrc[3] * pRot[2]) + (pSrc[7] * pRot[5]) + (pSrc[11] * pRot[8]);

    // Column 3 preserves source translations
    pDst[12] = pSrc[12];
    pDst[13] = pSrc[13];
    pDst[14] = pSrc[14];
    pDst[15] = pSrc[15];

    return dst;
}

/// <summary>
///     Computes a Left-Handed Orthographic Matrix mapping depth to Vulkan 0..1 (Column-Major)
/// </summary>
/// <param name="lowerX">the lower-bound x-value of the camera resolution in pixels; e.g, 0.</param>
/// <param name="upperX">the upper-bound x-value of the camera resolution in pixels; e.g, 1920.</param>
/// <param name="lowerY">the lower-bound y-value of the camera resolution in pixels; e.g, 0.</param>
/// <param name="upperY">the upper-bound y-value of the camera resolution in pixels; e.g, 1080.</param>
public static Matrix4x4 CreateOrthographic(
    float lowerX, float upperX, float lowerY, float upperY, float zNear, float zFar
)
{
    Matrix4x4 result = default;

    // calculate the absolute width, height and depth of the viewing frustrum (box) to display to the screen.
    float xRange = upperX - lowerX;
    float yRange = upperY - lowerY;
    float zRange = zFar - zNear; 

    float* m = result.M;

    /**
        scale objects from screen space into the graphics api's window-space.

        Window Space:
            X = -1 (left) to 1 (right).
            Y = -1 (bottom) to 1 (top).
            Z = 0 (close) to 1 (far).
    **/
    m[0] = 2.0f / xRange;
    m[5] = 2.0f / yRange;
    m[10] = 1.0f / zRange; 

    /**
        if left and right are symmetrical (e.g, -400 to 400) then right + left equals 0 (center of window-space)
        however if they are assymetrical (e.g, 0 to 1920) then the viewing frustum doesnt align with the center of window-space.

        these translations below shift the entire coordinate system horizontally and vertically so that whatever those values are
        it will always line up exactly with the center of window-space.
    **/
    m[12] = -(upperX + lowerX) / xRange;
    m[13] = -(upperY + lowerY) / yRange;
    // note that this is 0-1 for modern graphics APIS (metal, vulkan, directX12) unlike opengl (-1 to 0).
    m[14] = -zNear / zRange; 

    // homogenous coordinate value.
    m[15] = 1.0f;

    return result;
}

/// <summary>
///     Creates a col-major matrix from a transform.
/// </summary>
public static Matrix4x4 CreateMatrix(
    Transform transform
){
    Matrix4x4 result = default;
    float* m = result.M;

    // Pre-calculate squared terms for the quaternion rotation
    float x2 = transform.Rotation.X + transform.Rotation.X; 
    float y2 = transform.Rotation.Y + transform.Rotation.Y; 
    float z2 = transform.Rotation.Z + transform.Rotation.Z;
    float xx = transform.Rotation.X * x2; 
    float xy = transform.Rotation.X * y2; 
    float xz = transform.Rotation.X * z2;
    float yy = transform.Rotation.Y * y2; 
    float yz = transform.Rotation.Y * z2; 
    float zz = transform.Rotation.Z * z2;
    float wx = transform.Rotation.W * x2; 
    float wy = transform.Rotation.W * y2; 
    float wz = transform.Rotation.W * z2;

    // --- COLUMN 0 (X-Basis * ScaleX) ---
    m[0] = (1.0f - (yy + zz)) * transform.Scale.X;
    m[1] = (xy + wz) * transform.Scale.X;
    m[2] = (xz - wy) * transform.Scale.X;
    m[3] = 0.0f;

    // --- COLUMN 1 (Y-Basis * ScaleY) ---
    m[4] = (xy - wz) * transform.Scale.Y;
    m[5] = (1.0f - (xx + zz)) * transform.Scale.Y;
    m[6] = (yz + wx) * transform.Scale.Y;
    m[7] = 0.0f;

    // --- COLUMN 2 (Z-Basis * ScaleZ) ---
    m[8] = (xz + wy) * transform.Scale.Z;
    m[9] = (yz - wx) * transform.Scale.Z;
    m[10] = (1.0f - (xx + yy)) * transform.Scale.Z;
    m[11] = 0.0f;

    // --- COLUMN 3 (Translation) ---
    m[12] = transform.Position.X;
    m[13] = transform.Position.Y;
    m[14] = transform.Position.Z;
    m[15] = 1.0f;

    return result;
}

public static Quaternion Rotate(
    Quaternion q, float axisX, float axisY, float axisZ, float angleRadians
){
    // 1. Normalize the axis vector to ensure safe rotation math
    float length = (float)Sqrt(axisX * axisX + axisY * axisY + axisZ * axisZ);
    if (length < 0.0001f) return q; // Avoid division by zero
    
    axisX /= length;
    axisY /= length;
    axisZ /= length;

    // 2. Compute half-angles for the rotation representation
    float halfAngle = angleRadians * 0.5f;
    float sinHalf = (float)Sin(halfAngle);
    float cosHalf = (float)Cos(halfAngle);

    // 3. Construct the 'new' rotation quaternion
    float newX = axisX * sinHalf;
    float newY = axisY * sinHalf;
    float newZ = axisZ * sinHalf;
    float newW = cosHalf;

    // 4. Combine via multiplication (Local space: Original * New)
    float finalX = q.W * newX + q.X * newW + q.Y * newZ - q.Z * newY;
    float finalY = q.W * newY - q.X * newZ + q.Y * newW + q.Z * newX;
    float finalZ = q.W * newZ + q.X * newY - q.Y * newX + q.Z * newW;
    float finalW = q.W * newW - q.X * newX - q.Y * newY - q.Z * newZ;

    // 5. Return the finalized normalized rotation
    return Normalise(new Quaternion(){X = finalX, Y = finalY, Z = finalZ, W = finalW});
}

public static Quaternion Normalise(
    Quaternion q
){
    float len = (float)Sqrt(q.X * q.X + q.Y * q.Y + q.Z * q.Z + q.W * q.W);
    return new(){X = q.X / len, Y = q.Y / len, Z = q.Z / len, W = q.W / len};
}

/// <summary>
///     Creates a rotation quaternion around a normalised axis vector by a given angle in radians.
/// </summary>
public static Quaternion CreateFromAxisAngle(
    Vector3 axis, float angle
){

    // Half angle calculations required by quaternion space
    float halfAngle = angle * 0.5f;
    float sin = Sin(halfAngle);
    float cos = Cos(halfAngle);

    // Scale the normalized directional axis components by the sine projection
    return new(){
        X = axis.X * sin,
        Y = axis.Y * sin,
        Z = axis.Z * sin,
        W = cos
    };
}

public static Quaternion GetRotationBetweenPoints(
    Vector3 pointA, Vector3 pointB
){
    // Get the direction vector target pointing from A to B
    Vector3 delta = new(){
        X = pointB.X - pointA.X, 
        Y = pointB.Y - pointA.Y, 
        Z = pointB.Z - pointA.Z
    };

    Vector3 direction = Normalise(delta);
    
    // Define the default local resting axis (e.g., Vector3(0, 1, 0) if pointing Up)
    Vector3 startingAxis = Vector3.Up; 

    float dot = Dot(startingAxis, direction);

    // Edge Case Handling: Check if target vectors point directly opposite to prevent a divide-by-zero crash (180 deg)
    if (dot < -0.99999f)
    {
        // Pick an arbitrary perpendicular backup axis to rotate around instead
        Vector3 perpendicular = Cross(startingAxis, new(){X = 1});
        if (LengthSquared(perpendicular) < 0.001f)
        {
            perpendicular = Cross(startingAxis, new Vector3(){Z = 1});
        }
        
        return CreateFromAxisAngle(Normalise(perpendicular), Pi);
    }
    
    // Edge Case Handling: Vectors already point in the identical direction
    if (dot > 0.99999f)
    {
        return Quaternion.Identity;
    }

    // Shortest arc computation mapping directly onto the native components
    Vector3 axis = Cross(startingAxis, direction);
    
    Quaternion q = new(){
        X = axis.X,
        Y = axis.Y,
        Z = axis.Z,
        W = 1.0f + dot // W component maps directly to the cosine length offset prior to normalization
    };

    return Normalise(q);
}


}