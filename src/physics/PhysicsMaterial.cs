using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Howl.Physics;

public struct PhysicsMaterial
{
    public const float MinFriction = 0;
    public const float MaxFriction = 1;

    public const float MinDensity = float.Epsilon;
    public const float MaxDensity = 22.6f; // osmium density.

    public const float MinRestitution = 0f;
    public const float MaxRestitution = 1f;

    // Friction has two values: kinetic and static.
    // this is because - in the real world - objects require much more
    // initial force to start moving compared to when they would already be
    // in motion. This is simulated as two friction values.

    /// <summary>
    /// Gets and sets the static friction value.
    /// </summary>
    /// <remarks>
    /// Static friction is the resistance of motion before an object is sliding / is already in motion.
    /// </remarks>
    public float StaticFriction;

    /// <summary>
    /// Gets and sets the kinetic friction value.
    /// </summary>
    /// <remarks>
    /// Kinetic friction is the resistance of motion when an object is sliding / within motion and in contact with another object.
    /// </remarks>
    public float KineticFriction;

    /// <summary>
    /// Gets and sets the density value.
    /// </summary>
    public float Density;

    /// <summary>
    /// Gets and sets the restitution.
    /// </summary>
    /// <remarks>
    /// Note: restitution is how 'bouncy' a body is.
    /// </remarks>
    public float Restitution;

    /// <summary>
    /// Constructs a Physics Material.
    /// </summary>
    public PhysicsMaterial(){}

    /// <summary>
    /// Constructs a Physics Material.
    /// </summary>
    /// <param name="kineticFriction">The amount of kinetic friction - between 0 and 1.</param>
    /// <param name="staticFriction">The amount of static friction - between kinetic friction and 1.</param>
    public PhysicsMaterial(float staticFriction, float kineticFriction, float density, float restitution)
    {
        SetKineticFriction(ref KineticFriction, kineticFriction);
        SetStaticFriction(ref StaticFriction, ref KineticFriction, staticFriction);
        SetDensity(ref Density, density);
        Restitution = restitution;
    }

    /// <summary>
    ///     Asserts that a kinetic friction value is betwen <c><see cref="MinFriction"/></c> and <c><see cref="MaxFriction"/></c>
    /// </summary>
    /// <remarks>
    ///     Calls to this function are compiled out entirely when not in <c>DEBUG</c> builds.
    /// </remarks>
    /// <param name="value">the kinetic friction value.</param>
    [Conditional("DEBUG")]
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void AssertKineticFrictionInRange(float value)
    {
        System.Diagnostics.Debug.Assert(value >= MinFriction && value <= MaxDensity, 
            $"Kinetic Friction '{value}' is not within the range of '{MinFriction}' and '{MaxFriction}'"
        );
    }

    /// <summary>
    ///     Sets a kinetic friction value.
    /// </summary>
    /// <remarks>
    ///     Note: this function clamps kinetic friction within physics material's min and max friction values.
    /// </remarks>
    /// <param name="kineticFriction">the kinetic friction value to mutate.</param>
    /// <param name="value">the new kinetic friction value.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void SetKineticFriction(ref float kineticFriction, float value)
    {
        AssertKineticFrictionInRange(value);
        kineticFriction = Math.Math.Clamp(value, MinFriction, MaxFriction);
    }

    /// <summary>
    /// Sets the kinetic friction value of a physics material.
    /// </summary>
    /// <remarks>
    ///     Note: this function clamps kinetic friction within physics material's min and max friction values.
    /// </remarks>
    /// <param name="material">the physics material to mutate.</param>
    /// <param name="value">the new kinetic friction value.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void SetKineticFriction(ref PhysicsMaterial material, float value)
    {
        SetKineticFriction(ref material.KineticFriction, value);
    }

    /// <summary>
    ///     Asserts that a static friction value is between <c><paramref name="kineticFriction"/></c> and <c><see cref="MaxDensity"/></c>.
    /// </summary>
    /// <remarks>
    ///     Calls to this function are compiled out entirely when not in <c>DEBUG</c> builds.
    /// </remarks>
    /// <param name="value">the static friction value.</param>
    /// <param name="kineticFriction">the kinetic friction value.</param>
    [Conditional("DEBUG")]
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void AssertStaticFrictionInRange(float value, float kineticFriction)
    {
        System.Diagnostics.Debug.Assert(value >=  kineticFriction && value <= MaxFriction, 
            $"Static Friction '{value}' is not within the range of '{kineticFriction}' and '{MaxFriction}'"
        );
    }

    /// <summary>
    ///     Sets a static friction value.
    /// </summary>
    /// <remarks>
    ///     Note: this function clamps static friction within the kinetic friction value and physic material's max friction value.
    /// </remarks>
    /// <param name="staticFriction">the static friction value to mutate.</param>
    /// <param name="kineticFriction">the kinetic friction value used as the minimum static friction value.</param>
    /// <param name="value">the new static friction value.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void SetStaticFriction(ref float staticFriction, ref float kineticFriction, float value)
    {
        AssertStaticFrictionInRange(value, kineticFriction);
        staticFriction = Math.Math.Clamp(value, kineticFriction, MaxFriction);
    }

    /// <summary>
    ///     Sets the static friction value of a physics material.
    /// </summary>
    /// <remarks>
    ///     Note: this function clamps static friction within the kinetic friction value and physic material's max friction value.
    /// </remarks>
    /// <param name="material">the physics material to mutate.</param>
    /// <param name="value">the new static friction value.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void SetStaticFriction(ref PhysicsMaterial material, float value)
    {
        SetStaticFriction(ref material.StaticFriction, ref material.KineticFriction, value);
    }

    /// <summary>
    ///     Asserts that a density value is between <c><see cref="MinDensity"/></c> and <c><see cref="MaxDensity"/></c>.
    /// </summary>
    /// <remarks>
    ///     Calls to this function are compiled out entirely when not in <c>DEBUG</c> builds.
    /// </remarks>
    /// <param name="value">the density value.</param>
    [Conditional("DEBUG")]
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void AssertDensityInRange(float value)
    {
        System.Diagnostics.Debug.Assert(value >= MinDensity && value <= MaxDensity, 
            $"Density '{value}' is not within the range of '{MinDensity}' and '{MaxDensity}'"
        );
    }

    /// <summary>
    ///     Sets a density value.
    /// </summary>
    /// <remarks>
    ///     Note: this function clamps density within physics material's min and max density values.
    /// </remarks>
    /// <param name="density">the density value to mutate.</param>
    /// <param name="value">the new density value.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void SetDensity(ref float density, float value)
    {
        AssertDensityInRange(value);
        density = Math.Math.Clamp(value, MinDensity, MaxDensity);
    }

    /// <summary>
    ///     Sets a physics material's density value.
    /// </summary>
    /// <param name="material">the physics material to mutate.</param>
    /// <param name="value">the new density value.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void SetDensity(ref PhysicsMaterial material, float value)
    {
        SetDensity(ref material.Density, value);
    }

    /// <summary>
    ///     Asserts that a restitution value is between <c><see cref="MinRestitution"/></c> and <c><see cref="MaxDensity"/></c>.
    /// </summary>
    /// <remarks>
    ///     Calls to this function are compiled out entirely when not in <c>DEBUG</c> builds.
    /// </remarks>
    /// <param name="value">the restitution value.</param>
    [Conditional("DEBUG")]
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void AssertRestitutionInRange(float value)
    {        
        System.Diagnostics.Debug.Assert(value >= MinRestitution && value <= MaxRestitution, 
            $"Restitution '{value}' is not within the range of '{MinRestitution}' and '{MaxRestitution}'"
        );
    }

    /// <summary>
    ///     Sets a restitution value.
    /// </summary>
    /// <remarks>
    ///     Note: this function clamps restitution within physics material's min and max restitution values.
    /// </remarks>
    /// <param name="restitution">the restitution value to mutate.</param>
    /// <param name="value">the new restitution value.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void SetRestitution(ref float restitution, float value)
    {
        AssertRestitutionInRange(value);
        restitution = Math.Math.Clamp(value, MinRestitution, MaxRestitution);
    }

    /// <summary>
    ///     Sets a physics material's restitution value.
    /// </summary>
    /// <remarks>
    ///     Note: this function clamps restitution within physics material's min and max restitution values.
    /// </remarks>
    /// <param name="material">the physics material to mutate.</param>
    /// <param name="value">the new restitution value.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void SetRestitution(ref PhysicsMaterial material, float value)
    {
        SetRestitution(ref material.Restitution, value);
    }
}