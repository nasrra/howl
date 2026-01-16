namespace Howl.Graphics;

public struct Color
{
    /// <summary>
    /// Gets and sets the red channel component.
    /// </summary>
    public byte R;

    /// <summary>
    /// Gets and sets the green channel component.
    /// </summary>
    public byte G;

    /// <summary>
    /// Gets and sets the blue channel component.
    /// </summary>
    public byte B;

    /// <summary>
    /// Gets and set the alpha channel component.
    /// </summary>
    public byte A;

    /// <summary>
    /// Creates a new Color.
    /// </summary>
    /// <param name="r">The red channel value.</param>
    /// <param name="g">The green channel value.</param>
    /// <param name="b">The blue channel value.</param>
    /// <param name="a">The alpha channel value.</param>
    public Color(byte r, byte g, byte b, byte a)
    {
        R = r;
        G = g;
        B = b;
        A = a;
    }
}
