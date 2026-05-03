using System.Numerics;
using System.Runtime.CompilerServices;
using Vector2 = Howl.Math.Vector2;
namespace Howl.Math;

public static class MathV
{
    public static readonly Vector<float> Pi = new Vector<float>(Math.Pi);
    public static readonly Vector<float> OneSixth = new Vector<float>(1.0f / 6.0f);
    public static readonly Vector<float> OneTwentyFourth = new Vector<float>(1.0f / 24.0f);
    
    /// <summary>
    /// Vectorized rotation update using complex number multiplication (rotors)
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
    public static void RotorMultiply(Vector<float> sin, Vector<float> cos, Vector<float> theta,
        ref Vector<float> newSin, ref Vector<float> newCos
    )
    {
        Vector<float> thetaSq = theta * theta;

        // Get Sin/Cos of theta (Small Angle Approximation)
        Vector<float> sinDelta = theta * (Vector<float>.One - (thetaSq * OneSixth));
        Vector<float> cosDelta = Vector<float>.One - (thetaSq * 0.5f) + (thetaSq * thetaSq * OneTwentyFourth);

        // Complex Multiplication (identity math)
        // next sin = sin(a)cos(b) + cos(a)sin(b)
        Vector<float> nextSin = (sin * cosDelta) + (cos * sinDelta);
        // next cos = cos(a)cos(b) - sin(a)sin(b)
        Vector<float> nextCos = (cos * cosDelta) - (sin * sinDelta);

        // renormalise.
        // Note: floating-point numbers are imprecise, which accumulates the more they
        // are operated on. Renormalizing (the inv leng part) force the length back
        // to 1.0, so it doesnt drift and squish or enlargen undeterministically.
        Vector<float> dot = (nextSin * nextSin) + (nextCos * nextCos);

        // --- NAN PROTECTION ---
        // Define a tiny epsilon to avoid division by zero.
        Vector<float> epsilon = new Vector<float>(1e-10f);
        
        // Check where dot product is healthy
        Vector<int> isNormal = Vector.GreaterThan(dot, epsilon);

        Vector<float> invLen = Vector<float>.One / Vector.SquareRoot(dot);

        // If dot > epsilon, use normalized values. 
        // Otherwise, fallback to the unnormalized values (or a default identity).
        // Vector.ConditionalSelect(mask, trueValue, falseValue)
        newSin = Vector.ConditionalSelect(isNormal, nextSin * invLen, nextSin);
        newCos = Vector.ConditionalSelect(isNormal, nextCos * invLen, nextCos);
    }
}