namespace N_Howl.N_Graphics;

public struct Colour{
    public float R;
    public float G;
    public float B;
    public float A;

    public readonly static Colour White = new(){R = 1, G = 1, B = 1, A = 1};
    public readonly static Colour Black = new(){R = 0, G = 0, B = 0, A = 1};
    public readonly static Colour Red = new(){R = 1, G = 0, B = 0, A = 1};
    public readonly static Colour Green = new(){R = 0, G = 1, B = 0, A = 1};
    public readonly static Colour Blue = new(){R = 0, G = 0, B = 1, A = 1};
    public readonly static Colour Orange = new(){R = 1, G = 0.5f, B = 0, A = 1};
    public readonly static Colour Yellow = new(){R = 1, G = 1f, B = 0, A = 1};
    public readonly static Colour LightBlue = new(){R = 0.5f, G = 0.5f, B = 1, A = 1};
    public readonly static Colour Purple = new(){R = 1, G = 0, B = 1, A = 1};
    public readonly static Colour Pink = new(){R = 1, G = 0.5f, B = 0.5f, A = 1};
}