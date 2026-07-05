using Howl.Text;
using N_Howl.N_Math;
using N_Howl.N_Rendering;

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
    public readonly static Colour LightGreen = new(){R = 0.5f, G = 1, B = 0.5f, A = 1};

    public static Colour operator-(
        Colour colour
    ){
        colour.R *= -1;
        colour.G *= -1;
        colour.B *= -1;
        colour.A *= -1;
        return colour;
    }

    public static Colour operator*(Colour colour, float value){
        colour.R *= value;
        colour.G *= value;
        colour.B *= value;
        colour.A *= value;
        return colour;
    }
}

public enum TargetFrameRate
{
    D30,
    D60,
    D90,
    D120,
    D144,
    D165,
    D240,
    D360
}