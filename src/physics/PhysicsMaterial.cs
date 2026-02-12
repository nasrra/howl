namespace Howl.Physics;

public struct PhysicsMaterial
{

    public const float MinFriction = 0;
    public const float MaxFriction = 1;

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
    private float staticFriction;
    
    /// <summary>
    /// Gets the static friction value.
    /// </summary>
    /// <remarks>
    /// Note: static friction resists motion before an object starts sliding.
    /// </remarks>
    public readonly float StaticFriction => staticFriction;

    /// <summary>
    /// Gets and sets the kinetic friction value.
    /// </summary>
    /// <remarks>
    /// Note: kinetic friction is applied when an object is sliding/currently in motion.
    /// </remarks>
    private float kineticFriction;

    /// <summary>
    /// Gets the kinetic friction value.
    /// </summary>
    /// <remarks>
    /// Note: kinetic friction is applied when an object is sliding/currently in motion.
    /// </remarks>
    public readonly float KineticFriction => kineticFriction;

    /// <summary>
    /// Get and sets whether or not friction is used.
    /// </summary>
    public bool UseFriction;

    /// <summary>
    /// Constructs a Physics Material.
    /// </summary>
    public PhysicsMaterial()
    {
        kineticFriction = 0;
        staticFriction = 0;
        UseFriction = false;
    }

    /// <summary>
    /// Constructs a Physics Material.
    /// </summary>
    /// <param name="kineticFriction">The amount of kinetic friction - between 0 and 1.</param>
    /// <param name="staticFriction">The amount of static friction - between kinetic friction and 1.</param>
    public PhysicsMaterial(float staticFriction, float kineticFriction)
    {
        SetKineticFriction(kineticFriction);
        SetStaticFriction(staticFriction);
        UseFriction = true;
    }

    /// <summary>
    /// Sets the kinetic friction.
    /// </summary>
    /// <remarks>
    /// Note: this function will clamp the passed argument to be between the min and max friction values.
    /// </remars>
    /// <param name="value">the value to set kinetic friction to.</param>
    public void SetKineticFriction(float value)
    {
        kineticFriction = Math.Math.Clamp(value, MinFriction, MaxFriction);
#if DEBUG
        if(kineticFriction != value)
            System.Diagnostics.Debug.Assert(false, $"Kinetic Friction '{value}' is not within the range of '{MinFriction}' and '{MaxFriction}'");
#endif
    }

    /// <summary>
    /// Sets the static friction.
    /// </summary>
    /// <remarks>
    /// Note: this frunction will clamp the passed argument to be between the kinetic friction value and the max friction value.
    /// </remarks>
    /// <param name="value">the value to set static friction to.</param>
    public void SetStaticFriction(float value)
    {
        staticFriction = Math.Math.Clamp(value, KineticFriction, MaxFriction);
#if DEBUG
        if(staticFriction != value)
            System.Diagnostics.Debug.Assert(false, $"Static Friction '{value}' is not within the range of '{KineticFriction}' and '{MaxFriction}'");
#endif
    }
}