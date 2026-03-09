namespace Howl.Math;

public class SoaTransform
{
    /// <summary>
    /// Gets and sets the positional values.
    /// </summary>
    public SoaVector2 Position;

    /// <summary>
    /// Gets and sets the scaling values.
    /// </summary>
    public SoaVector2 Scale;

    /// <summary>
    /// Gets and sets the rotational values.
    /// </summary>
    public float[] Rotation;

    /// <summary>
    /// Gets and sets the Sin value of a rotation.
    /// </summary>
    public float[] Sin;

    /// <summary>
    /// Gets and sets the Cos value of a rotation.
    /// </summary>
    public float[] Cos;

    /// <summary>
    /// Creates a new SoaTransform instance.
    /// </summary>
    /// <param name="length"></param>
    public SoaTransform(int length)
    {
        Position    = new(length);
        Scale       = new(length);
        Rotation    = new float[length];
        Sin         = new float[length];
        Cos         = new float[length];
    }
}