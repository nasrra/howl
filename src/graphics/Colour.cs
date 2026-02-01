namespace Howl.Graphics;

public struct Colour
{

    public static Colour White => new Colour(255,255,255,255);

    public static Colour Black => new Colour(0,0,0,255);

    public static Colour Red => new Colour(255,0,0,255);
    
    public static Colour Green => new Colour(0,255,0,255);

    public static Colour Blue => new Colour(0,0,255,255);

    public static Colour Orange => new Colour(255,150,0,255);

    public static Colour LightBlue => new Colour(0,150,255,255);

    public static Colour Transparent => new Colour(0,0,0,0);

    public static Colour Pink => new Colour(255,0,150,255);


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
    /// Creates a new Colour.
    /// </summary>
    /// <param name="r">The red channel value.</param>
    /// <param name="g">The green channel value.</param>
    /// <param name="b">The blue channel value.</param>
    /// <param name="a">The alpha channel value.</param>
    public Colour(byte r, byte g, byte b, byte a)
    {
        R = r;
        G = g;
        B = b;
        A = a;
    }
}
