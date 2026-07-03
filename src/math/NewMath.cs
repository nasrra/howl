using System.Runtime.CompilerServices;
using N_Howl.N_Math;
using Howl;
using N_Howl.N_Collections;

namespace N_Howl.N_Math; 
public unsafe static class Math{

public const float Pi = 3.1415926535897932384626433f;
public const float Tau = 6.283185307179586f;
public const float OneSixth = 1.0f / 6.0f;
public const float OneTwentyFourth = 1.0f / 24.0f;

public static readonly System.Numerics.Vector<float> Vector_Pi = new System.Numerics.Vector<float>(Math.Pi);

/**##########################################################################################################################################
    div: Scalar Math.
##########################################################################################################################################**/

/// <summary>
/// A rotation update using complex number multiplication (rotors)
/// and a 4th order Taylor Series expansion for delta trigonometry.
/// </summary>
/// <remarks>
/// This is significantly faster then Vector.SinCos as it avoids
/// heavy transcendental instructions.
/// 
/// Accuracy: High for theta < 90 degrees (1.57 radian) per step.
/// Stability: Includes a renormalization pass to prevent floating-point drift
/// (scaling/shrinking) over time.
/// </remarks>
/// <param name="sin">the current sine values.</param>
/// <param name="cos">the current cosing values.</param>
/// <param name="theta">the angular change in radians: E.g. (angularVelocity * deltaTime).</param>
/// <param name="newSin">output for updated sine values.</param>
/// <param name="newCos">putput for updated cosine values.</param>
[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
public static void RotorMultiply(
    float sin, float cos, float theta, ref float newSin, ref float newCos
){
    float thetaSq = theta * theta;

    // Get Sin/Cos of theta (Small Angle Approximation)
    float sinDelta = theta * (1 - (thetaSq * OneSixth));
    float cosDelta = 1 - (thetaSq * 0.5f) + (thetaSq * thetaSq * OneTwentyFourth);

    // Complex Multiplication (identity math)
    // next sin = sin(a)cos(b) + cos(a)sin(b)
    float nextSin = (sin * cosDelta) + (cos * sinDelta);
    // next cos = cos(a)cos(b) - sin(a)sin(b)
    float nextCos = (cos * cosDelta) - (sin * sinDelta);

    // renormalise.
    // Note: floating-point numbers are imprecise, which accumulates the more they
    // are operated on. Renormalizing (the inv leng part) force the length back
    // to 1.0, so it doesnt drift and squish or enlargen undeterministically.
    float dot = (nextSin * nextSin) + (nextCos * nextCos);
    float invLen = 1 / Sqrt(dot);

    // --- NAN PROTECTION ---
    // Define a tiny epsilon to avoid division by zero.        
    if (float.IsNaN(invLen) || 1e-10f > invLen)
    {
        return;
    }

    newSin = nextSin * invLen;
    newCos = nextCos * invLen;
}

/// <summary>
///     Calculates the sum of all integers from <c><paramref name="n"/></c> to 1.
/// </summary>
/// <remarks>
///     Remarks: <c><paramref name="n"/></c> should not be larger than 46430.
/// </remarks>
/// <param name="n">the n'th number.</param>
/// <returns>the triangular sum.</returns>
public static int CalculateTriangularSum(
    int n
){
    Howl.Debug.Assert(n<46430, "");
    return n * (n+1) / 2;
}

[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
public static void Normalise(float x, float y, out float nX, out float nY){
    float invLength = 1.0f / Sqrt(x * x + y * y);
    nX = x * invLength;
    nY = y * invLength;
}

[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]    
public static float LengthSquared(
    float pointX, float pointY
){
    return Dot(pointX,pointY,pointX,pointY);
}

[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
public static T Dot<T>(
    T lhsX, T lhsY, T rhsX, T rhsY
) where T : System.Numerics.INumber<T>{
    return (lhsX * rhsX) + (lhsY * rhsY);
}

[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
public static float DistanceSquared(
    float fromX, float fromY, float toX, float toY
){
    float dx = fromX - toX;
    float dy = fromY - toY;
    return dx * dx + dy * dy;
}

[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
public static bool NearlyEqual(float a, float b, float epsilon)
{
    // // Note: norm-based comparison enurses
    // // the epsilon comparison doesnt return false negatives
    // // at large floating point values.        
    float diff = Abs(a-b);
    // float norm = Max(Abs(a),Abs(b));
    // return diff <= epsilon * Max(1f, norm);
    return diff <= epsilon;
}

[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
public static float ToRadians(float degrees){
    // PI / 180 = 0.0174532925199f
    return degrees * 0.0174532925199f;
}

[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
public static void RotateRadians(
    float increment, ref float radians, ref float sin, ref float cos
){
    radians += increment;
    sin = Sin(radians);
    cos = Cos(radians);
}

[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
public static void RotateRadians(
    float increment, System.Span<float> radians, System.Span<float> sin, System.Span<float> cos, int elementIndex
){
    ref float r = ref radians[elementIndex];
    r += increment;
    sin[elementIndex] = Sin(r);
    cos[elementIndex] = Cos(r);
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
public static float Abs(
    float value
){
    // const uint mask = 0x7FFFFFFF;
    // uint raw = System.BitConverter.SingleToUInt32Bits(value);
    // return System.BitConverter.UInt32BitsToSingle(raw & mask);
    return value<0? -value : value;
}

[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
public static float Atan2(
    float y, float x
){
    return System.MathF.Atan2(y, x);
}


[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
public static int LengthSquared(Vector2I vector){
    return (vector.X * vector.X) + (vector.Y * vector.Y);
}

[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
public static float Cross(
    float lhsX, float lhsY, float rhsX, float rhsY
){
    return lhsX * rhsY - lhsY * rhsX;    
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
public static T Min<T>(
    T a, T b
) where T : System.Numerics.INumber<T> {
    return a < b? a : b;
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

/**##########################################################################################################################################
    div: Vector2
##########################################################################################################################################**/

[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
public static Vector2 Abs(
    Vector2 v
){
    v.X = Abs(v.X);
    v.Y = Abs(v.Y);
    return v;
}

/// <summary>
/// Gets the distance between two vectors
/// </summary>
[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
public static float Distance(
    Vector2 from, Vector2 to
){
    float dx = from.X - to.X;
    float dy = from.Y - to.Y;
    return System.MathF.Sqrt(dx * dx + dy * dy);
}

[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
public static float DistanceSquared(
    Vector2 from, Vector2 to
)
{
    return Math.DistanceSquared(from.X, from.Y, to.X, to.Y);
}

/// <summary>
///     Transforms a vector by the supplied transform.
/// </summary>
[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
public static Vector2 TransformVector(
    Vector2 v, Transform2D t
){
    return TransformVector(t, v.X, v.Y);
}

/// <summary>
///     Transforms a vector by the supplied transform.
/// </summary>
[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
public static Vector2 TransformVector(
    in Transform2D transform, float vectorX, float  vectorY
){
    Math.TransformVector(
        vectorX, vectorY, transform.Scale.X, transform.Scale.Y, transform.Cosine, transform.Sine,
        transform.Position.X, transform.Position.Y, out float tx, out float ty
    );

    return new(){X = tx, Y = ty};
}

/// <summary>
///     Transforms a vector.
/// </summary>
[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
public static void TransformVector(
    float vectorX, float vectorY, float transformScaleX, float transformScaleY, float transformCos, float transformSin, 
    float transformPositionX, float transformPositionY, out float xOutput, out float yOutput
){
    // NOTE:
    // This ordering: Scale -> Rotation -> Translation
    // should remain the same. It is pretty much Matrix math.

    // Scale:
    float sx = vectorX * transformScaleX;
    float sy = vectorY * transformScaleY; 

    // Rotation:
    float rx = sx * transformCos - sy * transformSin;
    float ry = sx * transformSin + sy * transformCos;

    // Translation:
    xOutput = rx + transformPositionX;
    yOutput = ry + transformPositionY;
}

/**##########################################################################################################################################
    div: FsSoa_Vector2
##########################################################################################################################################**/

public static void Init(ref FsSoa_Vector2 soa, ref Memory.Arena arena, int entryStride, int maxEntries)
{
    Debug.Assert(soa.IsIntialised==false, "Already Initialised.");
    soa.IsIntialised = true;
    int dataLength = entryStride*maxEntries;
    Collections.Init(ref soa.X, ref arena, dataLength);
    Collections.Init(ref soa.Y, ref arena, dataLength);
    Collections.Init(ref soa.AppendCounts, ref arena, dataLength);
    soa.EntryStride = entryStride;
    soa.MaxEntries = maxEntries;
}

/// <summary>
///     Appends a vector to a fixed stride soa instance.
/// </summary>
/// <returns>
///     true, if successfully appended; otherwise false.
/// </returns>
public static bool Append(ref FsSoa_Vector2 soa, int entryIndex, float x, float y)
{
    // ensure that the entry slot isnt full.
    int appendCount = soa.AppendCounts[entryIndex];
    if(appendCount >= soa.EntryStride)
    {
        Debug.Assert(true, "Index Out of Range.");
        return false;
    }
    int appendIndex = entryIndex * soa.EntryStride + appendCount;

    // set the value.
    soa.X[appendIndex] = x;
    soa.Y[appendIndex] = y;

    // increment append index.
    soa.AppendCounts[entryIndex]++;
    return true;
}

/// <summary>
///     Sets the append count of an entry to zero in a fixed stride soa instance.
/// </summary>
[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
public static void ClearEntryAppendCount(ref FsSoa_Vector2 soa, int entryIndex)
{
    soa.AppendCounts[entryIndex] = 0;
}

/// <summary>
///     Sets the append count to zero of all entries in a fixed stride soa instance.
/// </summary>
/// <param name="soa">the soa instance to clear </param>
[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
public static void ClearAppendCounts(ref FsSoa_Vector2 soa)
{
    for(int i = 0; i < soa.MaxEntries; i++)
    {
        soa.AppendCounts[i] = 0;
    }
}

/**##########################################################################################################################################
    div: Soa_Vector2
##########################################################################################################################################**/

public static bool Init(ref Soa_Vector2 soa, ref Memory.Arena arena, int length)
{
    if (soa.IsIntialised)
    {
        Debug.Assert(false, "Already initialised.");
        return false;
    }
    soa.IsIntialised = true;
    soa.Length = length;
    Collections.Init(ref soa.X, ref arena, length);
    Collections.Init(ref soa.Y, ref arena, length);
    return true;
}

/// <summary>
///     Inserts elements into a soa instance.
/// </summary>
[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
public static void Insert(ref Soa_Vector2 soa, int insertIndex, float x, float y)
{
    soa.X[insertIndex] = x;
    soa.Y[insertIndex] = y;
}

/// <summary>
///     Appends an entry into a soa at the soa instance's <c>AppendCount</c> index.
/// </summary>
public static void Append(ref Soa_Vector2 soa, float x, float y)
{
    Insert(ref soa, soa.AppendCount, x, y);
    soa.AppendCount++;
}

/// <summary>
///     Sets a soa instance's <c>AppendCount</c> to zero.
/// </summary>
/// <param name="soa">the soa instance to reset.</param>
public static void ResetCount(ref Soa_Vector2 soa)
{
    soa.AppendCount = 0;
}

/**##########################################################################################################################################
    div: Soa_Transform2D
##########################################################################################################################################**/

public static bool Init(ref Soa_Transform2D soa, ref Memory.Arena arena, int length)
{
    if (soa.IsInitialised)
    {
        Debug.Assert(false,"Already Intialised.");
        return false;
    }
    soa.IsInitialised = true;
    Init(ref soa.Positions, ref arena, length);
    Init(ref soa.Scales, ref arena, length);
    Collections.Init(ref soa.Sines, ref arena, length);
    Collections.Init(ref soa.Cosines, ref arena, length);
    Collections.Init(ref soa.RotationRadians, ref arena, length);
    return true;
}

/// <summary>
/// Copies an soa transform entry into a transform struct.
/// </summary>
/// <param name="index">the index in the soa collection to copy.</param>
[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
public static void CopyFromSoa(
    ref Soa_Transform2D source, ref Transform destination, int index
){
    Transform2D t2D = default;
    t2D.Position.X = source.Positions.X[index];
    t2D.Position.Y = source.Positions.Y[index];
    t2D.Scale.X = source.Scales.X[index];
    t2D.Scale.Y = source.Scales.Y[index];
    t2D.Sine = source.Sines[index];
    t2D.Cosine = source.Cosines[index];
    t2D.RotationRadians = source.RotationRadians[index];

    destination = ToTransform(t2D);
}

[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
public static void CopyToSoa(
    ref Soa_Transform2D dst, Transform src, int elementIndex
){
    Transform2D t2D = ToTransform2D(src);
}

[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
public static void CopyToSoa(
    ref Soa_Transform2D dst, Transform2D src, int elementIndex
){

    CopyToSoa(
        ref dst, elementIndex, src.Position.X, src.Position.Y, src.Scale.X, src.Scale.Y, src.Sine, src.Cosine, src.RotationRadians
    );
}

[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
public static void CopyToSoa(
    ref Soa_Transform2D soa, int elementIndex, float posX, float posY, float scaleX, float scaleY, float sin, float cos, float rotationRadians
){
    soa.Positions.X[elementIndex] = posX;
    soa.Positions.Y[elementIndex] = posY;
    soa.Scales.X[elementIndex] = scaleX;
    soa.Scales.Y[elementIndex] = scaleY;
    soa.Sines[elementIndex] = sin;
    soa.Cosines[elementIndex] = cos;
    soa.RotationRadians[elementIndex] = rotationRadians;
}

public static void TransformRelative(Soa_Transform2D src, Soa_Transform2D dst, int srcReadIndex, int dstWriteIndex, 
    float worldPosX, float worldPosY, float worldScaleX, float worldScaleY, float worldSine, float worldCosine, float worldRotationRadians
)
{
    TransformRelative(src.Positions.X[srcReadIndex], src.Positions.Y[srcReadIndex], src.Scales.X[srcReadIndex], 
        src.Scales.Y[srcReadIndex], src.Sines[srcReadIndex], src.Cosines[srcReadIndex], src.RotationRadians[srcReadIndex], 
        worldPosX, worldPosY, worldScaleX, worldScaleY, worldSine, worldCosine, worldRotationRadians, 
        ref dst.Positions.X[dstWriteIndex], ref dst.Positions.Y[dstWriteIndex], ref dst.Scales.X[dstWriteIndex], 
        ref dst.Scales.Y[dstWriteIndex], ref dst.Sines[dstWriteIndex], ref dst.Cosines[dstWriteIndex], 
        ref dst.RotationRadians[dstWriteIndex] 
    );
}

/**##########################################################################################################################################
    div: Transform2D
##########################################################################################################################################**/

public static Transform2D ToTransform2D(
    Transform transform3D
){
    // Extract Roll (rotation around Z-axis) from Quaternion
    float num1 = (transform3D.Rotation.W * transform3D.Rotation.Z) + (transform3D.Rotation.X * transform3D.Rotation.Y);
    float num2 = (transform3D.Rotation.Y * transform3D.Rotation.Y) + (transform3D.Rotation.Z * transform3D.Rotation.Z);
    float rotationRadian = Atan2(2f * num1, 1f - (2f * num2));

    return new()
    {
        Position = new(){X = transform3D.Position.X, Y = transform3D.Position.Y},
        Scale = new(){X = transform3D.Scale.X, Y = transform3D.Scale.Y},
        RotationRadians = rotationRadian,
        Sine = Sin(rotationRadian),
        Cosine = Cos(rotationRadian)
    };
}

/// <summary>
///     Rotates a transform.
/// </summary>
public static void Rotate(
    ref Transform2D transform, float radians
)
{
    transform.RotationRadians += radians;
    transform.Sine = System.MathF.Sin(transform.RotationRadians);
    transform.Cosine = System.MathF.Cos(transform.RotationRadians);
}

/// <summary>
///     Offsets a transform by another.
/// </summary>
[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
public static Transform2D TransformRelative(
    Transform2D local, Transform2D world
)
{
    return TransformRelative(local, world.Position.X, world.Position.Y, world.Scale.X, world.Scale.Y, world.Sine, world.Cosine, world.RotationRadians);
}

/// <summary>
///     Offsets a transform by another.
/// </summary>
[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
public static Transform2D TransformRelative(
    Transform2D local, float worldPosX, float worldPosY, float worldScaleX, 
    float worldScaleY, float worldSine, float worldCosine, float worldRotationRadians
)
{
    Transform2D transform = default;
    
    TransformRelative(local.Position.X, local.Position.Y, local.Scale.X, local.Scale.Y, local.Sine, local.Cosine, local.RotationRadians, 
        worldPosX, worldPosY, worldScaleX, worldScaleY, worldSine, worldCosine, worldRotationRadians, ref transform.Position.X, 
        ref transform.Position.Y, ref transform.Scale.X, ref transform.Scale.Y, ref transform.Sine, ref transform.Cosine, 
        ref transform.RotationRadians 
    );

    return transform;
}

/// <summary>
///     Offsets a transform by another.
/// </summary>
[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
public static void TransformRelative(float localPosX, float localPosY, float localScaleX, float localScaleY, float localSine, float localCosine,
    float localRotationRadians, float worldPosX, float worldPosY, float worldScaleX, float worldScaleY, float worldSine, float worldCosine, 
    float worldRotationRadians, ref float posXOutput, ref float posYOutput, ref float scaleXOutput, ref float scaleYOutput, ref float sineOutput,
    ref float cosineOutput, ref float rotationRadiansOutput 
)
{
    // scale the local offset relative to the world.
    float scaledX = localPosX * worldScaleX;
    float scaledY = localPosY * worldScaleY;
    
    // rotate the local scaled offset around the parents origin.
    //      Standard 2D rotation matrix formula:
    //          x' = x * cos - y * sin
    //          y' = x * sin + y * cos
    float rotatedX = (scaledX * worldCosine) - (scaledY * worldSine);
    float rotatedY = (scaledX * worldSine) + (scaledY * worldCosine);

    // translate the rotated offset to the world position.
    posXOutput = rotatedX + worldPosX;
    posYOutput = rotatedY + worldPosY;

    // combine the scale properties
    scaleXOutput = localScaleX * worldScaleX;
    scaleYOutput = localScaleY * worldScaleY;

    // combine the rotation properties.
    //      Use the trigonometrix identity formulas for combining angles:
    //          sin(a + b) = sin(a)cos(b) + cos(a)sin(b)
    //          cos(a + b) = cos(a)cos(b) - sin(a)sin(b)
    sineOutput = (localSine * worldCosine) + (localCosine * worldSine);
    cosineOutput = (localCosine * worldCosine) + (localSine * worldSine);
    rotationRadiansOutput = localRotationRadians + worldRotationRadians;
}

/**##########################################################################################################################################
    div: Transform
##########################################################################################################################################**/

public static Transform ToTransform(
    Transform2D transform2D
){
    // Create a 3D Quaternion rotating only around the Z axis
    Quaternion rotation3D = CreateFromAxisAngle(new(){Z = 1}, transform2D.RotationRadians);

    return new Transform
    {
        Position = new(){X = transform2D.Position.X, Y = transform2D.Position.Y},
        Scale = new(){X = transform2D.Scale.X, Y = transform2D.Scale.Y},
        Rotation = rotation3D
    };
}

/// <summary>
///     Transforms the left-hand side by the righ-hand side.
/// </summary>
public static Transform TransformRelative(
    Transform lhs, Transform rhs
){
    Transform result = default;
    // combine scales.
    result.Scale = lhs.Scale * rhs.Scale;
    // combine rotations (order matters: rhs*lhs means rhs rotates lhs)
    result.Rotation = lhs.Rotation * rhs.Rotation;
    // combine positions (order matters: scale->rotate->translate).
    result.Position = RotateVector(lhs.Position * rhs.Scale, rhs.Rotation) + rhs.Position;
    return result;
}

/**##########################################################################################################################################
    div: Soa_Aabb
##########################################################################################################################################**/

public static bool Init(
    ref Soa_Aabb soa, ref Memory.Arena arena, int length
)
{
    if (soa.IsIntialised)
    {
        Debug.Panic("Already Intialised.");
        return false;
    }

    Collections.Init(ref soa.MinX, ref arena, length);
    Collections.Init(ref soa.MinY, ref arena, length);
    Collections.Init(ref soa.MaxX, ref arena, length);
    Collections.Init(ref soa.MaxY, ref arena, length);
    soa.Length = length;

    soa.IsIntialised = true;
    return true;
}

/// <summary>
/// Inserts an entry into a soa instance.
/// </summary>
/// <param name="soa">the soa aabb to insert into.</param>
/// <param name="insertIndex">the index in the soa arrays to insert into.</param>
/// <param name="minX">the x-component of the minimum vertex.</param>
/// <param name="minY">the y-component of the minimum vertex.</param>
/// <param name="maxX">the x-component of the maximum vertex.</param>
/// <param name="maxY">the y-component of the maximum vertex.</param>
[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
public static void Insert(ref Soa_Aabb soa, int insertIndex, float minX, float minY, float maxX, float maxY)
{
    soa.MinX[insertIndex] = minX;
    soa.MinY[insertIndex] = minY;
    soa.MaxX[insertIndex] = maxX;
    soa.MaxY[insertIndex] = maxY;   
}

/// <summary>
/// Appends an entry into an soa at the soa's <c>AppendCount</c> index.
/// </summary>
/// <param name="soa">the soa aabb to insert into.</param>
/// <param name="minX">the x-component of the minimum vertex.</param>
/// <param name="minY">the y-component of the minimum vertex.</param>
/// <param name="maxX">the x-component of the maximum vertex.</param>
/// <param name="maxY">the y-component of the maximum vertex.</param>
public static void Append(ref Soa_Aabb soa, float minX, float minY, float maxX, float maxY)
{
    Insert(ref soa, soa.AppendCount, minX, minY, maxX, maxY);
    soa.AppendCount++;
}

/// <summary>
/// Sets a soa instance's <c>AppendCount</c> to zero.
/// </summary>
/// <param name="soa">the soa instance to reset.</param>
[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
public static void ResetCount(ref  Soa_Aabb soa)
{
    soa.AppendCount = 0;
}

/// <summary>
/// Calculates the centroids of aabb's in a soa aabb using SISD.
/// </summary>
/// <remarks>
/// The length of <paramref name="x"/> and <paramref name="y"/> must be equal to the capacity of the soa aabb.
/// </remarks>
/// <param name="soa">the soa aabb with the aabb's to get the centroids of.</param>
/// <param name="x">output span for calculated the x-component of the centroid vectors.</param>
/// <param name="y">output span for calculated the y-component of the centroid vectors.</param>
/// <param name="startIndex">the entry index in the soa aabb to start at.</param>
/// <param name="length">the amount of aabb's to get the centroid of from the starting index.</param>
public static void CalculateCentroids_Sisd(ref Soa_Aabb soa, System.Span<float> x, System.Span<float> y, int startIndex, int length)
{
    System.Span<float> minX = Collections.AsSpan(soa.MinX);
    System.Span<float> minY = Collections.AsSpan(soa.MinY);
    System.Span<float> maxX = Collections.AsSpan(soa.MaxX);
    System.Span<float> maxY = Collections.AsSpan(soa.MaxY);

    for(int i = startIndex; i < length; i++)
    {
        CalculateAabbCentroid(minX[i], minY[i], maxX[i], maxY[i], out x[i], out y[i]);
    }
}

/// <summary>
/// Calculates the centroids of aabb's in a soa aabb using SIMD.
/// </summary>
/// <remarks>
/// The length of <paramref name="x"/> and <paramref name="y"/> must be equal to the capacity of the soa aabb.
/// </remarks>
/// <param name="soa">the soa aabb with the aabb's to get the centroids of.</param>
/// <param name="x">output span for calculated the x-component of the centroid vectors.</param>
/// <param name="y">output span for calculated the y-component of the centroid vectors.</param>
/// <param name="startIndex">the entry index in the soa aabb to start at.</param>
/// <param name="length">the amount of aabb's to get the centroid of from the starting index.</param>
/// <param name="tailindex">output for the index the simd operation stopped at.</param>
public static void CalculateCentroids_Simd(ref Soa_Aabb soa, System.Span<float> x, System.Span<float> y, int startIndex, int length, 
    ref int tailindex
)
{
    System.Span<float> minX = Collections.AsSpan(soa.MinX);
    System.Span<float> minY = Collections.AsSpan(soa.MinY);
    System.Span<float> maxX = Collections.AsSpan(soa.MaxX);
    System.Span<float> maxY = Collections.AsSpan(soa.MaxY);

    int simdSize = System.Numerics.Vector<float>.Count;
    int i = startIndex; 
    for(; i <= length - simdSize; i+= simdSize)
    {
        System.Numerics.Vector<float> vMinX = System.Numerics.Vector.LoadUnsafe(ref minX[i]);
        System.Numerics.Vector<float> vMinY = System.Numerics.Vector.LoadUnsafe(ref minY[i]);
        System.Numerics.Vector<float> vMaxX = System.Numerics.Vector.LoadUnsafe(ref maxX[i]);
        System.Numerics.Vector<float> vMaxY = System.Numerics.Vector.LoadUnsafe(ref maxY[i]);
        System.Numerics.Vector<float> vCentroidX = (vMaxX + vMinX) * 0.5f;
        System.Numerics.Vector<float> vCentroidY = (vMaxY + vMinY) * 0.5f;
        System.Numerics.Vector.StoreUnsafe(vCentroidX, ref x[i]);
        System.Numerics.Vector.StoreUnsafe(vCentroidY, ref y[i]);
    }
    tailindex = i;
}

/// <summary>
/// Calculates the centroids of aabb's in a soa aabb.
/// </summary>
/// <remarks>
/// The length of <paramref name="x"/> and <paramref name="y"/> must be equal to the capacity of the soa aabb.
/// </remarks>
/// <param name="soa">the soa aabb with the aabb's to get the centroids of.</param>
/// <param name="x">output span for calculated the x-component of the centroid vectors.</param>
/// <param name="y">output span for calculated the y-component of the centroid vectors.</param>
/// <param name="startIndex">the entry index in the soa aabb to start at.</param>
/// <param name="length">the amount of aabb's to get the centroid of from the starting index.</param>
public static void CalculateCentroids(ref Soa_Aabb soa, System.Span<float> x, System.Span<float> y, int startIndex, int length)
{
    int simdTailIndex = 0;

    // perform simd.
    CalculateCentroids_Simd(ref soa, x, y, startIndex, length, ref simdTailIndex);
    
    // fallback to sisd.
    CalculateCentroids_Sisd(ref soa, x, y, simdTailIndex, length);
}

/**##########################################################################################################################################
    div: Aabb
##########################################################################################################################################**/

/// <summary>
/// Gets the height of an AABB.
/// </summary>
/// <param name="aabb">the aabb.</param>
/// <returns>the height of the aabb.</returns>
[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
public static float GetHeight(
    in Aabb aabb
){
    return aabb.MaxY - aabb.MinY;
}

/// <summary>
/// Gets the width of an AABB.
/// </summary>
/// <param name="aabb">the aabb.</param>
/// <returns>the width of the aabb.</returns>
[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
public static float GetWidth(
    in Aabb aabb
){
    return aabb.MaxX - aabb.MinX;
}

/// <summary>
/// Calculates the center point of a AABB.
/// </summary>
/// <returns>The resultant vector.</returns>
[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
public static Vector2 CalculateCentroid(
    in Aabb aabb
){
    CalculateAabbCentroid(aabb.MinX, aabb.MinY, aabb.MaxX, aabb.MaxY, out float centerX, out float centerY);
    return new(){X = centerX, Y = centerY}; 
}

/// <summary>
/// Calcuates the center point of an AABB.
/// </summary>
/// <param name="minX">the x-component of the minimum vertex.</param>
/// <param name="minY">the y-component of the minimum vertex.</param>
/// <param name="maxX">the x-component of the maxiumum vertex.</param>
/// <param name="maxY">the y-component of the maxiumum vertex.</param>
/// <param name="centerX">the x-component of the center point.</param>
/// <param name="centerY">the y-component of the center point.</param>
[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveInlining)]
public static void CalculateAabbCentroid(
    float minX, float minY, float maxX, float maxY, out float centerX, out float centerY
){
    centerX = (maxX + minX) * 0.5f;
    centerY = (maxY + minY) * 0.5f;
}

/// <summary>
/// Constructs the minimum vector of an AABB.
/// </summary>
/// <returns>the minimum vector.</returns>
[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
public static Vector2 GetAabbMinVector(
    in Aabb aabb
){
    return new(){ X = aabb.MinX, Y = aabb.MinY};
}

/// <summary>
///     Constructs the maximum vector of an AABB.
/// </summary>
[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
public static Vector2 GetAabbMaxVector(
    in Aabb aabb
){
    return new(){ X = aabb.MaxX, Y = aabb.MaxY };
}

/// <summary>
///     Checks whether two Axis-Aligned-Bounding-Boxes are intersecting.
/// </summary>
/// <returns>true, if there is an intersection; otherwise false.</returns>
[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
public static bool AabbsIntersect(
    in Aabb a, in Aabb b
){
    return AabbsIntersect(a.MinX, b.MinX, a.MinY, b.MinY, a.MaxX, b.MaxX, a.MaxY, b.MaxY);
}

/// <summary>
///     Checks whether two Axis-Aligned-Bounding-Boxes are intersecting.
/// </summary>
/// <returns></returns>
[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
public static bool AabbsIntersect(
    float minXA, float minXB, float minYA, float minYB, float maxXA, float maxXB, float maxYA, float maxYB
){        
    if(maxXA <= minXB || maxXB <= minXA)
    {
        return false;
    }
    if (maxYA <= minYB || maxYB <= minYA)
    {
        return false;
    }

    return true;
}

/// <summary>
///     Checks whether an Axis-Aligned-Bounding-Box intersects with a point.
/// </summary>
/// <returns>true, if there is an intersection; otherwise false.</returns>
[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
public static bool AabbIntersectsPoint(
    in Aabb aabb, Vector2 point
){
    return AabbIntersectsPoint(
        aabb.MinX, aabb.MinY, aabb.MaxX, aabb.MaxY,
        point.X, point.Y
    );
}

/// <summary>
///     Checks whether an Axis-Aligned-Bounding-Box intersects with a point.
/// </summary>
/// <returns>true, if there is an intersection; otherwise false.</returns>
[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
public static bool AabbIntersectsPoint(
    float aabbMinX, float aabbMinY, float aabbMaxX, float aabbMaxY, float pointX, float pointY
){
    return 
    aabbMinX <= pointX &&
    aabbMinY <= pointY && 
    aabbMaxX >= pointX &&
    aabbMaxY >= pointY;        
}

/// <summary>
///     Checks whether an Axis-Aligned-Bounding-Box intersects with a line segment.
/// </summary>
/// <returns>true, if there is an intersection; otherwise false.</returns>
public static bool AabbIntersectsLine(
    in Aabb aabb, Vector2 lineStart, Vector2 lineEnd
){
    return AabbIntersectsLine(
        aabb.MinX, aabb.MinY, aabb.MaxX, aabb.MaxY,
        lineStart.X, lineStart.Y, lineEnd.X, lineEnd.Y
    );
}

/// <summary>
///     Checks whether an Axis-Aligned-Bounding-Box intersects with a line segment.
/// </summary>
public static bool AabbIntersectsLine(
    float aabbMinX, float aabbMinY, float aabbMaxX, float aabbMaxY,
    float lineStartX, float lineStartY, float lineEndX, float lineEndY
){       
    float closestPointX;
    float closestPointY;

    ClosestPoint(lineStartX, lineStartY, lineEndX, lineEndY, aabbMinX, aabbMinY, out closestPointX, out closestPointY);
    if(AabbIntersectsPoint(aabbMinX, aabbMinY, aabbMaxX, aabbMaxY, closestPointX, closestPointY))
    {
        ClosestPoint(lineStartX, lineStartY, lineEndX, lineEndY, aabbMaxX, aabbMaxY, out closestPointX, out closestPointY);
        if(AabbIntersectsPoint(aabbMinX, aabbMinY, aabbMaxX, aabbMaxY, closestPointX, closestPointY))
        {
            return true;
        }
    }
    return false;
}

/// <summary>
///     Constructs a Axis-Aligned-Bounding-Box from the union of two AABB's
/// </summary>
[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
public static Aabb UnionAabbs(
    Aabb a, Aabb b
){
    UnionAabbs(
        a.MinX, a.MinY, a.MaxX, a.MaxY,
        b.MinX, b.MinY, b.MaxX, b.MaxY,
        out float unionMinX, out float unionMinY,
        out float unionMaxX, out float unionMaxY
    );

    Aabb result = default;
    result.MinX = unionMinX;
    result.MinY = unionMinY;
    result.MaxX = unionMaxX;
    result.MaxY = unionMaxY;
    return result;
}

/// <summary>
///     Gets the min and max components for the union of an AABB.
/// </summary>
[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
public static void UnionAabbs(
    float minXA, float minYA, float maxXA, float maxYA,
    float minXB, float minYB, float maxXB, float maxYB,
    out float unionMinX, out float unionMinY, out float unionMaxX, out float unionMaxY
){
    unionMinX = Math.Min(minXA, minXB);
    unionMinY = Math.Min(minYA, minYB);
    unionMaxX = Math.Max(maxXA, maxXB);
    unionMaxY = Math.Max(maxYA, maxYB);
}

/**##########################################################################################################################################
    div: Shape Utils.
##########################################################################################################################################**/

/// <summary>
/// Finds the closest vertex on a polygon to a given position.
/// </summary>
/// <param name="queryPosition">position to find the closest vertex to.</param>
/// <param name="verticesX">The x-componentes of a polygons vertices.</param>
/// <param name="verticesY">The y-componentes of a polygons vertices.</param>
/// <returns>The index of the vertex in the vertices span that is the closest point.</returns>
/// <exception cref="ArgumentException">Throws when the passed in vertex-spans do not match in length.</exception>
public static int FindClosestVertexOnPolygon(
    Vector2 queryPosition, System.Span<float> verticesX, System.Span<float> verticesY
){
    return FindClosestVertexOnPolygon(queryPosition.X, queryPosition.Y, verticesX, verticesY);
}

/// <summary>
/// Finds the closest vertex on a polygon to a given position.
/// </summary>
/// <param name="queryPositionX">the x-component of the position to find the closest vertex to.</param>
/// <param name="queryPositionY">the x-component of the position to find the closest vertex to.</param>
/// <param name="verticesX">The x-componentes of a polygons vertices.</param>
/// <param name="verticesY">The y-componentes of a polygons vertices.</param>
/// <returns>The index of the vertex in the vertices span that is the closest point.</returns>
public static int FindClosestVertexOnPolygon(
    float queryPositionX, float queryPositionY, System.Span<float> verticesX, System.Span<float> verticesY
){
    int result = -1;
    float minDistance = float.MaxValue;

    if(verticesX.Length != verticesY.Length)
    {
        throw new System.ArgumentException($"verticesX length '{verticesX.Length}' is not equal to verticesY length '{verticesY.Length}'");
    }
    
    for(int i = 0; i < verticesX.Length; i++)
    {
        float distance = DistanceSquared(verticesX[i], verticesY[i], queryPositionX, queryPositionY);

        if(distance < minDistance)
        {
            minDistance = distance;
            result = i;
        }
    }

    return result;        
}

/// <summary>
/// Calculates the centroid-vector of of a polygon.
/// </summary>
/// <param name="polygonVerticesX">The x-values of a polygon's vertices.</param>
/// <param name="polygonVerticesY">The y-values of a polygon's vertices.</param>
/// <returns>The centroid-vector.</returns>
/// <exception cref="ArgumentException">Throws when the passed in vertex-spans do not match in length.</exception>
[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
public static Vector2 CalculateCentroid(System.Span<float> polygonVerticesX, System.Span<float> polygonVerticesY)
{
    float cX = 0;
    float cY = 0;
    CalculateCentroid(polygonVerticesX, polygonVerticesY, ref cX, ref cY);
    return new(){X = cX, Y = cY};
}

/// <summary>
/// Calculates the centroid for a convex or concave polygon (that do not self intersect) using the shoelace formula.
/// </summary>
/// <param name="x">the x-values of the shape's vertices.</param>
/// <param name="y">the y-values of the shape's vertices.</param>
/// <param name="cX">output for the x-component of the centroid vertice.</param>
/// <param name="cY">output for the y-component of the centroid vertice.</param>
public static void CalculateCentroid(System.Span<float> x, System.Span<float> y, ref float cX, ref float cY)
{
    float area = 0;
    float invArea = 0;
    float tempX = 0;
    float tempY = 0;
    int length = x.Length;

    float x0 = 0;
    float y0 = 0;
    float x1 = 0;
    float y1 = 0;

    float crossProduct;

    int nextIndex;
    bool nextInRange;

    for(int i = 0; i < length; i++)
    {
        nextIndex = i + 1;
        nextInRange = nextIndex < length;

        // get curernt vertex and the next one.
        x0 = x[i];
        y0 = y[i];
        if (nextInRange)
        {
            x1 = x[nextIndex];
            y1 = y[nextIndex];    
        }
        else
        {
            x1 = x[0];
            y1 = y[0];    
        }

        // calculate the corss product (signed area of the triangle)
        // this is the "Shoelace" part.
        crossProduct = Math.Cross(x0, y0, x1, y1);

        area += crossProduct;
        tempX += (x0 + x1) * crossProduct;
        tempY += (y0 + y1) * crossProduct;
    }

    area *= 0.5f; // final signed area.

    if(Math.Abs(area) > float.Epsilon)
    {
        invArea = 1f/ (area * 6.0f); 
        cX = tempX * invArea;
        cY = tempY * invArea;
    }
    else
    {
        // if the area is 0, the polygon is degenerate (a line or point)
        cX = x[0];
        cY = y[0];
    }

}

/**##########################################################################################################################################
    div: Spatial.
##########################################################################################################################################**/

/// <summary>
/// Gets the closest point along a line segment towards a given point. 
/// </summary>
[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
public static void ClosestPoint(
    Vector2 lineSegmentStart, Vector2 lineSegmentEnd, Vector2 queryPoint, out Vector2 closestPoint, out float distanceSquared
){
    closestPoint = ClosestPoint(lineSegmentStart, lineSegmentEnd, queryPoint);
    distanceSquared = DistanceSquared(queryPoint, closestPoint);
}

/// <summary>
/// Gets the closest point along a line segment towards a given point. 
/// </summary>
/// <returns>The closest point along the line segment towards the query point.</returns>
[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
public static Vector2 ClosestPoint(
    Vector2 lineStart, Vector2 lineEnd, Vector2 queryPoint
){
    ClosestPoint(
        lineStart.X,
        lineStart.Y,
        lineEnd.X,
        lineEnd.Y,
        queryPoint.X,
        queryPoint.Y,
        out float closestPointX,
        out float closestPointY
    );
    return new(){X = closestPointX, Y = closestPointY};
}

/// <summary>
/// Gets the closest point along a line segment towards a given point.
/// </summary>
[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
public static void ClosestPoint(
    float lineStartX, float lineStartY, float lineEndX, float lineEndY, float queryPointX, float queryPointY, 
    out float closestPointX, out float closestPointY, out float distanceSquared
){
    ClosestPoint(lineStartX, lineStartY, lineEndX, lineEndY, queryPointX,queryPointY, out closestPointX,out closestPointY);
    distanceSquared = DistanceSquared(queryPointX, queryPointY, closestPointX, closestPointY);
}

/// <summary>
/// Gets the closest point along a line segment towards a given point.
/// </summary>
[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
public static void ClosestPoint(
    float lineStartX, float lineStartY, float lineEndX, float lineEndY, float queryPointX,
    float queryPointY, out float closestPointX, out float closestPointY
){
    float lineDistanceX = lineEndX - lineStartX;
    float lineDistanceY = lineEndY - lineStartY;
    float pointDistanceX = queryPointX - lineStartX;
    float pointDistanceY = queryPointY - lineStartY;

    // float projection = Vector2.Dot(pointDistance, lineDistance);
    float projection = Dot(pointDistanceX, pointDistanceY, lineDistanceX, lineDistanceY);

    // move the point distance along the line segment.
    float delta = projection / LengthSquared(lineDistanceX, lineDistanceY);

    if(delta <= 0){
        closestPointX = lineStartX;
        closestPointY = lineStartY;
    }
    else if(delta >= 1){
        closestPointX = lineEndX;
        closestPointY = lineEndY;
    }
    else{
        closestPointX = lineStartX + lineDistanceX * delta;
        closestPointY = lineStartY + lineDistanceY * delta;
    }
}

/**##########################################################################################################################################
    div: Circle
##########################################################################################################################################**/

[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
public static void GetMinMaxVertices(
    float x, float y, float radius, out float minX, out float minY, out float maxX, out float maxY
){
    minX = x - radius;
    minY = y - radius;
    maxX = x + radius;
    maxY = y + radius;
}

/// <remarks>
///     The radius is scaled by the largest component in the scaling vector.
/// </remarks>
[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
public static float ScaleRadius(
    float radius, Vector2 scale
){
    return ScaleCircleRadius(radius, scale.X, scale.Y);
}

/// <remarks>
///     The radius is scaled by the largest component in the scaling vector.
/// </remarks>
[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
public static float ScaleCircleRadius(
    float radius, float scaleX, float scaleY
){
    return radius *= Max(scaleX, scaleY);
}

/// <summary>
/// Gets the area of a circle.
/// </summary>
/// <param name="radius">the radius of the circle.</param>
/// <returns>the area of the circle.</returns>
[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
public static float GetCircleArea(
    float radius
){
    return radius * radius * Pi;
}

/// <summary>
/// Gets the area of a circle.
/// </summary>
/// <param name="circle">the circle.</param>
/// <returns>the area of the circle.</returns>
[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
public static float GetCircleArea(
    ref Circle circle
){
    return GetCircleArea(circle.Radius);
}

/// <summary>
/// Vectorised calculation of circles radii.
/// </summary>
/// <param name="radius">a vector of circle radii.</param>
/// <returns>the area values of the circles.</returns>
[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
public static System.Numerics.Vector<float> GetArea(
    System.Numerics.Vector<float> radius
){
    return radius * radius * Vector_Pi;
}

/**##########################################################################################################################################
    div: Polygon Rectangle
##########################################################################################################################################**/


public static PolygonRectangle CreatePolygonRectangle(
    Rectangle rect
){
    return CreatePolygonRectangle(rect.X, rect.Y, rect.Width, rect.Height);
}

public static void Init(
    ref PolygonRectangle polyRect, Array<Vector2> verts
){
    Debug.Assert(verts.Length == PolygonRectangle.VerticesLength, $"verts length '{verts.Length}' is not equal to '{PolygonRectangle.VerticesLength}'");

    fixed(float* xDst = polyRect.VerticesX)
    {
        fixed(float* yDst = polyRect.VerticesY)
        {
            for(int i = 0; i < PolygonRectangle.VerticesLength; i++)
            {
                xDst[i] = verts[i].X;
                yDst[i] = verts[i].Y;
            }
        }
    }
}

public static PolygonRectangle CreatePolygonRectangle(
    Vector2 vert0, Vector2 vert1, Vector2 vert2, Vector2 vert3
){
    PolygonRectangle rect = default;
    rect.VerticesX[0] = vert0.X;
    rect.VerticesY[0] = vert0.Y;
    rect.VerticesX[1] = vert1.X;
    rect.VerticesY[1] = vert1.Y;
    rect.VerticesX[2] = vert2.X;
    rect.VerticesY[2] = vert2.Y;
    rect.VerticesX[3] = vert3.X;
    rect.VerticesY[3] = vert3.Y;
    return rect;
}

public static void Init(
    ref PolygonRectangle polyRect, Array<float> vertsX, Array<float> vertsY
){
fixed(float* xDst = polyRect.VerticesX){
fixed(float* yDst = polyRect.VerticesY){
    Debug.Assert(vertsX.Length == PolygonRectangle.VerticesLength, $"vertsX length '{vertsX.Length}' is not equal to '{PolygonRectangle.VerticesLength}'");
    Debug.Assert(vertsY.Length == PolygonRectangle.VerticesLength, $"vertsY length '{vertsY.Length}' is not equal to '{PolygonRectangle.VerticesLength}'");

    for(int i = 0; i < PolygonRectangle.VerticesLength; i++)
    {
        xDst[i] = vertsX[i];
        yDst[i] = vertsY[i];
    }
}}}

[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
public static PolygonRectangle CreatePolygonRectangle(
    float x, float y, float width, float height
){
    PolygonRectangle rect = default;
    
    float left = x;
    float top = y;
    float right = x+width;
    float bottom = y-height;

    // top left.
    rect.VerticesX[0] = left;
    rect.VerticesY[0] = top;

    // top right.
    rect.VerticesX[1] = right;
    rect.VerticesY[1] = top;
    // bottom right.
    rect.VerticesX[2] = right;
    rect.VerticesY[2] = bottom;
    // bottom left.
    rect.VerticesX[3] = left;
    rect.VerticesY[3] = bottom;

    return rect;
}

public static System.Span<float> GetVerticesXAsSpan(
    in PolygonRectangle polygonRectangle
){
fixed(float* ptr = polygonRectangle.VerticesX){
    System.Span<float> span;
    span = new System.Span<float>(ptr, PolygonRectangle.VerticesLength);
    return span;
}}

public static System.Span<float> GetVerticesYAsSpan(
    in PolygonRectangle polygonRectangle
){
fixed(float* ptr = polygonRectangle.VerticesY){
    System.Span<float> span;
    span = new System.Span<float>(ptr, PolygonRectangle.VerticesLength);
    return span;
}}

[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
public static PolygonRectangle Transform(
    in PolygonRectangle polyRect, in Transform2D transform
){
    return CreatePolygonRectangle(
        TransformVector(transform, polyRect.VerticesX[0], polyRect.VerticesY[0]),
        TransformVector(transform, polyRect.VerticesX[1], polyRect.VerticesY[1]),
        TransformVector(transform, polyRect.VerticesX[2], polyRect.VerticesY[2]),
        TransformVector(transform, polyRect.VerticesX[3], polyRect.VerticesY[3])
    );
}

/// <summary>
///     Calculates the centroid-vector of a polygon rectangle.
/// </summary>
/// <param name="polygonRectangle">The polygon rectangle.</param>
/// <returns>The centroid-vector.</returns>
[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
public static Vector2 GetCentroid(
    in PolygonRectangle polygonRectangle
){
    return CalculateCentroid(GetVerticesXAsSpan(polygonRectangle), GetVerticesYAsSpan(polygonRectangle));
}

/// <summary>
///     Calculates the centroid-vector of a polygon rectangle.
/// </summary>
[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
public static void GetCentroid(
    in PolygonRectangle polygonRectangle, ref float centroidXOutput, ref float centroidYOutput
){
    CalculateCentroid(GetVerticesXAsSpan(polygonRectangle), GetVerticesYAsSpan(polygonRectangle), ref centroidXOutput, ref centroidYOutput);
}


/// <summary>
/// Gets the Axis-Aligned-Bounding-Box of a polygon rectangle.
/// </summary>
/// <param name="polygonRectangle">The polygon rectangle.</param>
/// <returns>The calculated AABB.</returns>
[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
public static Aabb GetAABB(
    in PolygonRectangle polygonRectangle
){
    Math.GetMinMaxVectors(GetVerticesXAsSpan(polygonRectangle), GetVerticesYAsSpan(polygonRectangle), out float minX, out float minY, out float maxX, out float maxY);
    return new(){MinX = minX, MinY = minY, MaxX = maxX, MaxY = maxY};
}

/// <summary>
///     Gets the width of a polygon rectangle.
/// </summary>
[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
public static float GetWidth(
    in PolygonRectangle rectangle
){   
    Vector2 vertexA = new(){X = rectangle.VerticesX[0], Y = rectangle.VerticesY[0]}; 
    Vector2 vertexB = new(){X = rectangle.VerticesX[1], Y = rectangle.VerticesY[1]}; 
    return Distance(vertexA, vertexB);
}

/// <summary>
///     Gets the height of a polygon rectangle.
/// </summary>
[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
public static float GetHeight(
    in PolygonRectangle rectangle
){
    Vector2 vertexA = new(){ X = rectangle.VerticesX[0], Y = rectangle.VerticesY[0]}; 
    Vector2 vertexB = new(){ X = rectangle.VerticesX[3], Y = rectangle.VerticesY[3]}; 
    return Distance(vertexA, vertexB);
}

/// <summary>
/// Gets the min and max vectors from a span of vertices.
/// </summary>
/// <remarks>
///    <para><b>Remarks:</b></para>
///    <para>
///     It is assumed that the x and y span are of equal length;
///     if they are not the same length: undefined behaviour will occur.
///    </para>
/// </remarks>
[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
public static void GetMinMaxVectors(
    System.Span<float> verticesX, System.Span<float> verticesY, out float minX, out float minY, out float maxX, out float maxY
){
    minX = float.MaxValue;
    minY = float.MaxValue;
    maxX = float.MinValue;
    maxY = float.MinValue;

    for(int i = 0; i < verticesX.Length; i++){
        float v = verticesX[i];
        if (v < minX){
            minX = v;
        }
        if(v > maxX){
            maxX = v;
        }
    }

    for(int i = 0; i < verticesY.Length; i++){
        float v = verticesY[i];
        if(v < minY){
            minY = v;
        }
        if(v > maxY){
            maxY = v;
        }
    }
}

/**##########################################################################################################################################
    div: SAT
##########################################################################################################################################**/

public const float PolygonContactPointEpsilon = 1e-5f;

// the fallback normal for any SAT intersect will be up.
// meaning that if any shapes perfectly overlap with eachother
// (sharing the same position) one will be pushed up and the other down.
public const float InitialNormalX = 0;
public const float InitialNormalY = 1;

/// <summary>
///     Checks for intersection between two circles.
/// </summary>
/// <returns>true, if there is an intersection; otherwise false.</returns>
[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
public static bool CirclesIntersect(
    in Circle lhs, in Circle rhs, out Vector2 normal, out float depth
){
    bool intersects = CirclesIntersect(lhs, rhs, out float normalX, out float normalY, out depth);
    normal = new(){X = normalX, Y = normalY};
    return intersects;
}

/// <summary>
///     Checks for intersection between two circles.
/// </summary>
/// <returns>true, if there is an intersection; otherwise false.</returns>
[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
public static bool CirclesIntersect(
    in Circle lhs, in Circle rhs, out float normalX, out float normalY, out float depth
){
    return CirclesIntersect(lhs.X, lhs.Y, lhs.Radius, rhs.X, rhs.Y , rhs.Radius, out normalX, out normalY, out depth);
}

/// <summary>
///     Checks for intersection between two circles.
/// </summary>
/// <returns>true, if there is an intersection; otherwise false.</returns>
[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
public static bool CirclesIntersect(
    float lhsX, float lhsY, float lhsRadius, float rhsX, float rhsY, float rhsRadius, 
    out float normalX, out float normalY, out float depth
){
    normalX = InitialNormalX;
    normalY = InitialNormalY;
    depth = 0f;

    float distanceSqrd = DistanceSquared(lhsX, lhsY, rhsX, rhsY);
    float radiusSum = lhsRadius + rhsRadius;
    float radiusSumSq = radiusSum * radiusSum;

    if (distanceSqrd >= radiusSumSq)
        return false;

    // Apply a full up force if the two colliders are in the exact same position.
    // this also stops the whole collision system from exploding.
    if (distanceSqrd < float.Epsilon)
    {
        depth = radiusSum;
        return true;
    }

    float distance = Sqrt(distanceSqrd);
    Normalise(lhsX - rhsX, lhsY - rhsY, out normalX, out normalY);
    depth = radiusSum - distance;
    return true;        
}

/// <summary>
///     Projects the edges of circle onto a given axis.
/// </summary>
[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
public static void ProjectCircle(
    in Circle circle, float axisX, float axisY, out float minCicleEdge, out float maxCircleEdge
){
    ProjectCircle(circle.X, circle.Y, circle.Radius, axisX, axisY, out minCicleEdge, out maxCircleEdge);
}  

/// <summary>
///     Projects the edges of a circle onto a given axis.
/// </summary>
public static void ProjectCircle(
    float circleX, float circleY, float circleRadius, float axisX, float axisY,
    out float minCircleEdge, out float maxCircleEdge
){
    float directionAndRadiusX = axisX * circleRadius;
    float directionAndRadiusY = axisY * circleRadius;

    float vAX = circleX + directionAndRadiusX;
    float vAY = circleY + directionAndRadiusY;
    float vBX = circleX - directionAndRadiusX;
    float vBY = circleY - directionAndRadiusY;
    
    minCircleEdge = Dot(vAX, vAY, axisX, axisY);
    maxCircleEdge = Dot(vBX, vBY, axisX, axisY);

    if(minCircleEdge > maxCircleEdge)
    {
        float temp = minCircleEdge;
        minCircleEdge = maxCircleEdge;
        maxCircleEdge = temp;
    }
}

/// <summary>
///     Finds the contact point between two intersecting circles.
/// </summary>
/// <param name="contactPoint">The calculated contact point relative to circle a.</param>
[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
public static void FindContactPoints(
    in Circle a, in Circle b, out Vector2 contactPoint
){
    FindContactPoints(a, b, out float cX, out float cY);
    contactPoint = new(){X = cX, Y = cY};
}

/// <summary>
///     Finds the contact point between two intersecting circles.
/// </summary>
[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
public static void FindContactPoints(
    in Circle a, in Circle b, out float contactPointX, out float contactPointY
){
    FindContactPoints(a.X, a.Y, a.Radius, b.X, b.Y, out contactPointX, out contactPointY);
}

/// <summary>
///     Finds the contact points between two circles.
/// </summary>
[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
public static void FindContactPoints(
    float circleAX, float circleAY, float cricleARadius, float circleBX, float circleBY, out float contactPointX, out float contactPointY
){
    float distanceX = circleBX - circleAX;
    float distanceY = circleBY - circleAY;
    Normalise(distanceX, distanceY, out float directionX, out float directionY);
    
    // check for Nan in case the two circles are perfectly ontop of one another,
    // as normalising a distance of zero gives a NaN.
    contactPointX = float.IsNaN(directionX)? circleAX : circleAX + (directionX * cricleARadius);
    contactPointY = float.IsNaN(directionX)? circleAY : circleAY + (directionY * cricleARadius); 
}

[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
public static bool PolygonIntersectsPoint(
    System.Span<float> polygonVerticesX, System.Span<float> polygonVerticesY, 
    float pointX, float pointY, ref float normalX, ref float normalY, ref float depth
){
    depth = float.MaxValue;
    normalX = 0;
    normalY = 0;

    float minMagSqrd = float.MaxValue;
    float minDepthSqrd = float.MaxValue;
    float minAxisDepth = float.MaxValue;
    float minAxisX = float.MaxValue;
    float minAxisY = float.MaxValue;

    float minEdgeA = float.MaxValue;
    float maxEdgeA = float.MinValue;

    for(int i = 0; i < polygonVerticesX.Length; i++)
    {
        int vBIndex = (i + 1 == polygonVerticesX.Length) ? 0 : i + 1;

        // calc the perpendicular edge.
        float axisX = -(polygonVerticesY[vBIndex] - polygonVerticesY[i]);
        float axisY = polygonVerticesX[vBIndex] - polygonVerticesX[i];

        // project using axis.
        ProjectPolygon_Sisd(polygonVerticesX, polygonVerticesY, axisX, axisY, polygonVerticesX.Length, ref minEdgeA, ref maxEdgeA);        
        float pointProj = Dot(pointX, pointY, axisX, axisY);

        if(pointProj <= minEdgeA || pointProj >= maxEdgeA)
        {
            return false; // Separation found.
        }

        // Calculate overlap in "scaled space"
        float axisDepth = Min(pointProj - minEdgeA, maxEdgeA - pointProj);

        // to compare depths correctly, the squared length of the axis is needed.
        float magSqrd = axisX * axisX + axisY * axisY;

        float axisDepthSqrd = axisDepth * axisDepth;

        // check if this is the minimum translation distance.
        if(minDepthSqrd * magSqrd > axisDepthSqrd)
        {
            minDepthSqrd = axisDepthSqrd / magSqrd; // store relative squared depth.
            minAxisX = axisX;
            minAxisY = axisY;
            minAxisDepth = axisDepth;
            minMagSqrd = magSqrd;
        }
    }

    float mag = Sqrt(minMagSqrd);
    depth = minAxisDepth / mag; // Only one sqrt if this is the new minimum translation distance.
    normalX = minAxisX / mag;
    normalY = minAxisY / mag;

    return true;
}

/// <summary>
/// Checks for an intersection between two polygons.
/// </summary>
/// <param name="lhsX">the x-components of the left-hand side rectangle vertices.</param>
/// <param name="lhsY">the y-components of the left-hand side rectangle vertices.</param>
/// <param name="rhsX">the x-components of the right-hand side rectangle vertices.</param>
/// <param name="rhsY">the y-components of the right-hand side rectangle vertices.</param>
/// <param name="lhsCentroidX">the x-component of the left-hand side rectangle's centroid.</param>
/// <param name="lhsCentroidY">the y-component of the left-hand side rectangle's centroid.</param>
/// <param name="rhsCentroidX">the x-component of the right-hand side rectangle's centroid.</param>
/// <param name="rhsCentroidY">the y-component of the right-hand side rectangle's centroid.</param>
/// <param name="normalX">the x-component of the intersection normal in relation to the right-hand side rectangle.</param>
/// <param name="normalY">the y-component of the intersection normal in relation to the right-hand side rectangle.</param>
/// <param name="depth">The depth of the intersection in relation to the right-hand side rectangle.</param>
/// <returns>true, if there is an intersection; otherwise false.</returns>
/// <exception cref="ArgumentException"></exception>
public static bool PolygonsIntersect(
    System.Span<float> lhsX, System.Span<float> lhsY, System.Span<float> rhsX, System.Span<float> rhsY,
    float lhsCentroidX, float lhsCentroidY, float rhsCentroidX, float rhsCentroidY, 
    out float normalX, out float normalY, out float depth
){

    if(lhsX.Length != lhsY.Length)
    {
        throw new System.ArgumentException($"lhs vertices length do not match: '{lhsX.Length}' != '{lhsY.Length}'");
    }
    if(rhsX.Length != rhsY.Length)
    {
        throw new System.ArgumentException($"rhs vertices length do not match: '{rhsX.Length}' != '{rhsY.Length}'");
    }

    normalX = InitialNormalX;
    normalY = InitialNormalY;
    float foundNormalX;
    float foundNormalY;
    float foundDepth;
    depth = float.MaxValue;


    if (PolygonOneWayIntersect(lhsX, lhsY, rhsX, rhsY, out foundNormalX, out foundNormalY, out foundDepth))
    {            
        if(depth > foundDepth)
        {
            depth = foundDepth;
            normalX = foundNormalX; 
            normalY = foundNormalY;
        }
    }
    else
    {
        return false;
    }

    if (PolygonOneWayIntersect(rhsX, rhsY, lhsX, lhsY, out foundNormalX, out foundNormalY, out foundDepth))
    {            
        if(depth > foundDepth)
        {
            depth = foundDepth;
            normalX = foundNormalX; 
            normalY = foundNormalY;
        }
    }
    else
    {
        return false;
    }

    // when a new smaller   
    // depth is found but in relation to rect B, not A.
    // this is so that the resolution code will always push A out of B
    // and not push the two into each other when a smaller depth is found when 
    // looping through rect B.
    if(Dot(rhsCentroidX - lhsCentroidX, rhsCentroidY - lhsCentroidY, normalX, normalY) >= 0)
    {
        normalX = -normalX;
        normalY = -normalY;
    }
    
    return true;
}

[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
public static bool PolygonOneWayIntersect(
    System.Span<float> polygonVerticesXA, System.Span<float> polygonVerticesYA, 
    System.Span<float> polygonVerticesXB, System.Span<float> polygonVerticesYB, 
    out float normalX, out float normalY, out float depth
){
    depth = float.MaxValue;
    normalX = 0;
    normalY = 0;

    float minMagSqrd = float.MaxValue;
    float minDepthSqrd = float.MaxValue;
    float minAxisDepth = float.MaxValue;
    float minAxisX = float.MaxValue;
    float minAxisY = float.MaxValue;

    float minEdgeA = float.MaxValue;
    float minEdgeB = float.MaxValue;
    float maxEdgeA = float.MinValue;
    float maxEdgeB = float.MinValue;

    for(int i = 0; i < polygonVerticesXA.Length; i++)
    {
        int vBIndex = (i + 1 == polygonVerticesXA.Length) ? 0 : i + 1;

        // edge.
        float axisX = -(polygonVerticesYA[vBIndex] - polygonVerticesYA[i]);
        float axisY = polygonVerticesXA[vBIndex] - polygonVerticesXA[i];

        // project using axis.
        ProjectPolygon_Sisd(polygonVerticesXA, polygonVerticesYA, axisX, axisY, polygonVerticesXA.Length, ref minEdgeA, ref maxEdgeA);        
        ProjectPolygon_Sisd(polygonVerticesXB, polygonVerticesYB, axisX, axisY, polygonVerticesXB.Length, ref minEdgeB, ref maxEdgeB);        


        if(minEdgeA >= maxEdgeB || minEdgeB >= maxEdgeA)
        {
            return false; // Separation found.
        }

        // Calculate overlap in "scaled space"
        float axisDepth = Min(maxEdgeB - minEdgeA, maxEdgeA - minEdgeB);

        // to compare depths correctly, the squared length of the axis is needed.
        float magSqrd = axisX * axisX + axisY * axisY;

        float axisDepthSqrd = axisDepth * axisDepth;

        // check if this is the minimum translation distance.
        if(minDepthSqrd * magSqrd > axisDepthSqrd)
        {
            minDepthSqrd = axisDepthSqrd / magSqrd; // store relative squared depth.
            minAxisX = axisX;
            minAxisY = axisY;
            minAxisDepth = axisDepth;
            minMagSqrd = magSqrd;
        }
    }

    float mag = Sqrt(minMagSqrd);
    depth = minAxisDepth / mag; // Only one sqrt if this is the new minimum translation distance.
    normalX = minAxisX / mag;
    normalY = minAxisY / mag;

    return true;
}

/// <summary>
///     projects a set of vertices onto a normalised axis.
/// </summary>
/// <remarks>
///     Remarks: the 'edge' of a polygon is defined as the outer most vertices that are projected onto the axis.
/// </remarks>
[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
public static void ProjectPolygon_Sisd(
    System.Span<float> verticesX, System.Span<float> verticesY, float axisX, float axisY, int vertexCount, 
    ref float minEdgeOutput, ref float maxEdgeOutput
){
    minEdgeOutput = float.MaxValue;
    maxEdgeOutput = float.MinValue;

    for(int i = 0; i < vertexCount; i++)
    {
        float projection = Dot(verticesX[i], verticesY[i], axisX, axisY);

        if(projection < minEdgeOutput)
        {
            minEdgeOutput = projection;
        }
        if(projection > maxEdgeOutput)
        {
            maxEdgeOutput = projection;
        }
    }
}

/// <summary>
/// Finds the contact points between two intersecting polygons.
/// </summary>
/// <remarks>
/// Note: ensure to check contact points amount before using contactPoint2.
/// </remarks>
[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
public static void FindContactPoints(
    System.Span<float> polygonAVerticesX, System.Span<float> polygonAVerticesY, System.Span<float> polygonBVerticesX, 
    System.Span<float> polygonBVerticesY, float epsilon, out float contactPoint1X, out float contactPoint1Y, 
    out float contactPoint2X, out float contactPoint2Y, out int contactPointsAmount
){
    if(polygonAVerticesX.Length != polygonAVerticesY.Length)
    {
        throw new System.ArgumentException($"polygonAVerticesX length '{polygonAVerticesX.Length}' is not equal to polygonAVerticesY length '{polygonAVerticesY.Length}'");
    }

    if(polygonBVerticesX.Length != polygonBVerticesY.Length)
    {
        throw new System.ArgumentException($"polygonBVerticesX length '{polygonBVerticesX.Length}' is not equal to polygonBVerticesY length '{polygonBVerticesY.Length}'");
    }

    contactPoint1X = 0;
    contactPoint1Y = 0;
    contactPoint2X = 0;
    contactPoint2Y = 0;
    contactPointsAmount = 0;
    float minDistSqrd = float.MaxValue;

    // polygon a to b.
    FindContactPointsOneWay(
        polygonAVerticesX, polygonAVerticesY, polygonBVerticesX, polygonBVerticesY, epsilon, ref minDistSqrd, 
        ref contactPoint1X, ref contactPoint1Y, ref contactPoint2X, ref contactPoint2Y, ref contactPointsAmount
    );

    // polygon b to a.
    FindContactPointsOneWay(
        polygonBVerticesX, polygonBVerticesY, polygonAVerticesX, polygonAVerticesY, epsilon, ref minDistSqrd, 
        ref contactPoint1X, ref contactPoint1Y, ref contactPoint2X, ref contactPoint2Y, ref contactPointsAmount
    );
}

/// <summary>
/// Finds the contact points between two intersecting polygons.
/// </summary>
/// <remarks>
/// Note: this function assumes polygon A and B vertices X and Y spans have matching lengths.
/// </remarks>
public static void FindContactPointsOneWay(
    System.Span<float> polygonAVerticesX, System.Span<float> polygonAVerticesY, System.Span<float> polygonBVerticesX, 
    System.Span<float> polygonBVerticesY, float epsilon, ref float minDistSqrd, ref float contactPoint1XOutput, 
    ref float contactPoint1YOutput, ref float contactPoint2XOutput, ref float contactPoint2YOutput, ref int contactPointsAmountOutput
){
    int polygonAVerticesLength = polygonAVerticesX.Length;
    int polygonBVerticesLength = polygonBVerticesX.Length;

    for(int i = 0; i < polygonAVerticesLength; i++)
    {
        float pointX = polygonAVerticesX[i];
        float pointY = polygonAVerticesY[i];

        for(int startIndex = 0; startIndex < polygonBVerticesLength; startIndex++)
        {
            // find the closest point on polygon b to the vertice on polygon a.
            
            float edgeStartX = polygonBVerticesX[startIndex];
            float edgeStartY = polygonBVerticesY[startIndex];

            int endIndex = startIndex + 1;

            // this is faster than modulo.
            if(endIndex >= polygonBVerticesLength)
                endIndex = 0;
            
            float edgeEndX = polygonBVerticesX[endIndex];
            float edgeEndY = polygonBVerticesY[endIndex];

            ClosestPoint(
                edgeStartX, edgeStartY, edgeEndX, edgeEndY, pointX, pointY, 
                out float closestPointX, out float closestPointY, out float distSqrd
            );

            if(NearlyEqual(distSqrd, minDistSqrd, epsilon))
            {
                // note: there is a chance that two contact points can be in the same place.
                // this is caused by when two vertices - one from each polygon - are in contact.
                // without this 'if check', all the contact information will be wiped out 
                // when those two corners hit eachother.

                if(NearlyEqual(closestPointX, contactPoint1XOutput, epsilon) == false
                || NearlyEqual(closestPointY, contactPoint1YOutput, epsilon) == false)
                {
                    // there are two contact points.
                    contactPointsAmountOutput = 2;
                    contactPoint2XOutput = closestPointX;             
                    contactPoint2YOutput = closestPointY;
                }
            }
            else if(distSqrd < minDistSqrd)
            {
                // a new absolute minimum contact point has been found.
                // meaning that there is only one contact point.

                minDistSqrd = distSqrd;
                contactPointsAmountOutput = 1;
                contactPoint1XOutput = closestPointX;
                contactPoint1YOutput = closestPointY;
            }
        } 
    } 
}

/// <summary>
///     Checks whether a polygon and a circle intersect.
/// </summary>
/// <returns>true, if there is an intersection; otherwise false.</returns>
[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
public static bool PolygonAndCircleIntersect(
    System.Span<float> polygonVerticesX, System.Span<float> polygonVerticesY, in Circle circle,
    float polygonCentroidX, float polygonCentroidY, float circleCenterX, float circleCenterY,
    out float normalX, out float normalY, out float depth
){
    return PolygonAndCircleIntersect(
        polygonVerticesX,
        polygonVerticesY,
        polygonCentroidX,
        polygonCentroidY,
        circle.X,
        circle.Y,
        circle.Radius,
        circleCenterX,
        circleCenterY,
        out normalX,
        out normalY,
        out depth
    );       
}

/// <summary>
/// Checks whether a polygon and a circle intersect with eachother.
/// </summary>
/// <returns>true, if the two shapes are colliding; otherwise false.</returns>
public static bool PolygonAndCircleIntersect(        
    System.Span<float> polygonVerticesX, System.Span<float> polygonVerticesY,
    float polygonCentroidX, float polygonCentroidY, float circleX, float circleY,
    float circleRadius, float circleCenterX, float circleCenterY,
    out float normalX, out float normalY, out float depth
){
    depth = float.MaxValue;

    // store normals as floats and operate on them as
    // floats before allocating a Vector as numerical
    // arithematic is faster.
    normalX = InitialNormalX;
    normalY = InitialNormalY;

    float axisX;
    float axisY;
    float axisDepth;
    float minA = float.MaxValue;
    float maxA = float.MaxValue;
    float minB;
    float maxB;

    if(polygonVerticesX.Length != polygonVerticesY.Length)
    {
        throw new System.ArgumentException($"polygonVerticesX length '{polygonVerticesX.Length}' does not equal polygonVerticesY length '{polygonVerticesY.Length}'");
    }

    for(int i = 0; i < polygonVerticesX.Length; i++)
    {
        int vAIndex = i;
        int vBIndex = i+1;

        // this is faster than modulo.
        if(vBIndex >= polygonVerticesX.Length)
            vBIndex = 0;

        float xA = polygonVerticesX[vAIndex];
        float xB = polygonVerticesX[vBIndex];
        float yA = polygonVerticesY[vAIndex];
        float yB = polygonVerticesY[vBIndex];

        float edgeX = xB - xA; 
        float edgeY = yB - yA; 

        // the normal of the edge.
        // note: this only works as vertices are assumed to be in clockwise winding order.
        // change to new Vector2(edge.Y, -edge.X); if anti-clockwise.
        axisX = -edgeY;
        axisY = edgeX;
    
        // normalize (important for correct depth).
        Normalise(axisX, axisY, out axisX, out axisY);

    
        // project all vertices onto the current edge to find the min and max values
        // of the two rectangles along the edge.
        ProjectPolygon_Sisd(polygonVerticesX, polygonVerticesY, axisX, axisY, polygonVerticesX.Length, ref minA, ref maxA);        
        ProjectCircle(circleX, circleY, circleRadius, axisX, axisY, out minB, out maxB);

        if(minA > maxB || minB > maxA)
        {
            // there is separation.
            return false;
        }

        axisDepth = Min(maxB - minA, maxA - minB);
        if(depth > axisDepth)
        {
            // only assign if the newly found intersection depth is smaller.
            depth = axisDepth;
            normalX = axisX;
            normalY = axisY;
        }
    }

    int closestPointIndex = FindClosestVertexOnPolygon(circleCenterX, circleCenterY, polygonVerticesX, polygonVerticesY);
    float closestPointX = polygonVerticesX[closestPointIndex];
    float closestPointY = polygonVerticesY[closestPointIndex];

    axisX = closestPointX - circleX;
    axisY = closestPointY - circleY;
    Normalise(axisX, axisY, out axisX, out axisY);

    // project all vertices onto the current edge to find the min and max values
    // of the two rectangles along the edge.
    ProjectPolygon_Sisd(polygonVerticesX, polygonVerticesY, axisX, axisY, polygonVerticesX.Length, ref minA, ref maxA);        
    ProjectCircle(circleX, circleY, circleRadius, axisX, axisY, out minB, out maxB);

    if(minA > maxB || minB > maxA)
    {
        // there is separation.
        return false;
    }

    axisDepth = Min(maxB - minA, maxA - minB);
    if(depth > axisDepth)
    {
        // only assign if the newly found intersection depth is smaller.
        depth = axisDepth;
        normalX = axisX;
        normalY = axisY;
    }

    float distanceX = circleCenterX - polygonCentroidX;
    float distanceY = circleCenterY - polygonCentroidY;

    // when a new smaller   
    // depth is found but in relation to rect B, not A.
    // this is so that the resolution code will always push A out of B
    // and not push the two into each other when a smaller depth is found when 
    // looping through rect B.
    if(Dot(distanceX, distanceY, normalX,  normalY) >= 0)
    {
        normalX = -normalX;
        normalY = -normalY;
    }

    return true;
}

/// <summary>
///     Finds the contact points between a polygon and a point.
/// </summary>
public static void FindContactPoints(
    System.Span<float> polygonVerticesX, System.Span<float> polygonVerticesY, float pointX, float pointY, 
    out float contactPointX, out float contactPointY
){
    if(polygonVerticesX.Length != polygonVerticesY.Length)
        throw new System.ArgumentException($"polygonVerticesX length '{polygonVerticesX.Length}' is not equal to polygonVerticesY length '{polygonVerticesY.Length}'");

    contactPointX = float.MaxValue;    
    contactPointY = float.MaxValue;
    float minDistSqrd = float.MaxValue;
    int length = polygonVerticesX.Length;

    // find the closest point for each edge of the rectangle.
    for(int startIndex = 0; startIndex < length; startIndex++){        
        int endIndex = startIndex + 1;
        
        // this is faster than modulo.
        if(endIndex >= length)
            endIndex = 0;

        ClosestPoint(
            polygonVerticesX[startIndex], polygonVerticesY[startIndex], polygonVerticesX[endIndex], polygonVerticesY[endIndex],
            pointX, pointY, out float closestPointX, out float closestPointY, out float distSqrd
        );

        if(distSqrd < minDistSqrd){
            minDistSqrd = distSqrd;
            contactPointX = closestPointX;
            contactPointY = closestPointY;
        }
    } 
}

/**##########################################################################################################################################
    div: Rectangle.
##########################################################################################################################################**/

[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
public static float GetRectangleArea(
    float width, float height
){
    return width * height;
}

[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
public static float GetRectangleArea(
    ref Rectangle rectangle
){
    return GetRectangleArea(rectangle.Width, rectangle.Height);
}

[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
public static System.Numerics.Vector<float> GetRectangleArea(
    System.Numerics.Vector<float> width, System.Numerics.Vector<float> heigth
){
    return width * heigth;
}

/**##########################################################################################################################################
    div: Vector3
##########################################################################################################################################**/

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
public static Vector3 Cross(Vector3 a, Vector3 b){
    return new(){
        X = (a.Y * b.Z) - (a.Z * b.Y),
        Y = (a.Z * b.X) - (a.X * b.Z),
        Z = (a.X * b.Y) - (a.Y * b.X)
    };
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

[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
public static float Dot(Vector3 lhs, Vector3 rhs){
    return (lhs.X * rhs.X) + (lhs.Y * rhs.Y) + (lhs.Z * rhs.Z);
}

/// <summary>
///     Rotates a Vector around the origin (0,0,0), by a quaternion rotation. 
/// </summary>
public static Vector3 RotateVector(
    Vector3 v, Quaternion q
){
    Vector3 qV = new(){X = q.X, Y = q.Y, Z = q.Z};
    Vector3 cross1 = Cross(qV, v);
    Vector3 cross2 = Cross(qV, cross1);
    return v + (cross1 * (2.0f * q.W)) + (cross2 * 2.0f);
}

}