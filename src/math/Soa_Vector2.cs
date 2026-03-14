
namespace Howl.Math;

public class Soa_Vector2
{
    /// <summary>
    /// Gets and sets the x-coordinate values.
    /// </summary>
    public float[] X;

    /// <summary>
    /// Gets and sets the y-coordinate values.
    /// </summary>
    public float[] Y;

    /// <summary>
    /// Creates a new SoaVector2 instance.
    /// </summary>
    /// <param name="length">the length of x and y-components to store.</param>
    public Soa_Vector2(int length)
    {
        X = new float[length];
        Y = new float[length];
    }
}