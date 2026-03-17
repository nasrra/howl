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
    /// Note: static friction resists motion before an object starts sliding.
    /// </remarks>
    public float StaticFriction;

    /// <summary>
    /// Gets and sets the kinetic friction value.
    /// </summary>
    /// <remarks>
    /// Note: kinetic friction is applied when an object is sliding/currently in motion.
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
    /// Get and sets whether or not friction is used.
    /// </summary>
    public bool UseFriction;

    /// <summary>
    /// Constructs a Physics Material.
    /// </summary>
    public PhysicsMaterial(){}

    /// <summary>
    /// Constructs a Physics Material.
    /// </summary>
    /// <param name="kineticFriction">The amount of kinetic friction - between 0 and 1.</param>
    /// <param name="staticFriction">The amount of static friction - between kinetic friction and 1.</param>
    public PhysicsMaterial(float staticFriction, float kineticFriction, float density)
    {
        SetKineticFriction(ref KineticFriction, kineticFriction);
        SetStaticFriction(ref StaticFriction, ref KineticFriction, staticFriction);
        SetDensity(ref Density, density);
        UseFriction = true;
    }

    /// <summary>
    /// Sets a kinetic friction value.
    /// </summary>
    /// <remarks>
    /// Note: this function clamps kinetic friction within physics material's min and max friction values.
    /// </remarks>
    /// <param name="kineticFriction">the kinetic friction value to mutate.</param>
    /// <param name="value">the new kinetic friction value.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void SetKineticFriction(ref float kineticFriction, float value)
    {
        kineticFriction = Math.Math.Clamp(value, MinFriction, MaxFriction);
#if DEBUG
        if(kineticFriction != value)
            System.Diagnostics.Debug.Assert(false, $"Kinetic Friction '{value}' is not within the range of '{MinFriction}' and '{MaxFriction}'");
#endif
    }

    /// <summary>
    /// Sets the kinetic friction value of a physics material.
    /// </summary>
    /// <remarks>
    /// Note: this function clamps kinetic friction within physics material's min and max friction values.
    /// </remarks>
    /// <param name="material">the physics material to mutate.</param>
    /// <param name="value">the new kinetic friction value.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void SetKineticFriction(ref PhysicsMaterial material, float value)
    {
        SetKineticFriction(ref material.KineticFriction, value);
    }

    /// <summary>
    /// Sets a static friction value.
    /// </summary>
    /// <remarks>
    /// Note: this function clamps static friction within the kinetic friction value and physic material's max friction value.
    /// </remarks>
    /// <param name="staticFriction">the static friction value to mutate.</param>
    /// <param name="kineticFriction">the kinetic friction value used as the minimum static friction value.</param>
    /// <param name="value">the new static friction value.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void SetStaticFriction(ref float staticFriction, ref float kineticFriction, float value)
    {
        staticFriction = Math.Math.Clamp(value, kineticFriction, MaxFriction);
#if DEBUG
        if(staticFriction != value)
            System.Diagnostics.Debug.Assert(false, $"Static Friction '{value}' is not within the range of '{kineticFriction}' and '{MaxFriction}'");
#endif
    }

    /// <summary>
    /// Sets the static friction value of a physics material.
    /// </summary>
    /// <remarks>
    /// Note: this function clamps static friction within the kinetic friction value and physic material's max friction value.
    /// </remarks>
    /// <param name="material">the physics material to mutate.</param>
    /// <param name="value">the new static friction value.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void SetStaticFriction(ref PhysicsMaterial material, float value)
    {
        SetStaticFriction(ref material.StaticFriction, ref material.KineticFriction, value);
    }

    /// <summary>
    /// Sets a density value.
    /// </summary>
    /// <remarks>
    /// Note: this function clamps density within physics material's min and max density values.
    /// </remarks>
    /// <param name="density">the density value to mutate.</param>
    /// <param name="value">the new density value.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void SetDensity(ref float density, float value)
    {
        density = Math.Math.Clamp(value, MinDensity, MaxDensity);
#if DEBUG
        if(density != value)
            System.Diagnostics.Debug.Assert(false, $"Density '{value}' is not within the range of '{MinDensity}' and '{MaxDensity}'");
#endif
    }

    /// <summary>
    /// Sets a physics material's density value.
    /// </summary>
    /// <param name="material">the physics material to mutate.</param>
    /// <param name="value">the new density value.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void SetDensity(ref PhysicsMaterial material, float value)
    {
        SetDensity(ref material.Density, value);
    }

    /// <summary>
    /// Sets a restitution value.
    /// </summary>
    /// <remarks>
    /// Note: this function clamps restitution within physics material's min and max restitution values.
    /// </remarks>
    /// <param name="restitution">the restitution value to mutate.</param>
    /// <param name="value">the new restitution value.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void SetRestitution(ref float restitution, float value)
    {
        restitution = Math.Math.Clamp(value, MinRestitution, MaxRestitution);
#if DEBUG
        if(restitution != value)
            System.Diagnostics.Debug.Assert(false, $"Restitution '{value}' is not within the range of '{MinRestitution}' and '{MaxRestitution}'");
#endif
    }

    /// <summary>
    /// Sets a physics material's restitution value.
    /// </summary>
    /// <remarks>
    /// Note: this function clamps restitution within physics material's min and max restitution values.
    /// </remarks>
    /// <param name="material">the physics material to mutate.</param>
    /// <param name="value">the new restitution value.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void SetRestitution(ref PhysicsMaterial material, float value)
    {
        SetRestitution(ref material.Restitution, value);
    }
}