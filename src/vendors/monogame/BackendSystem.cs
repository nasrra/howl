using Howl.Graphics;

namespace Howl.Vendors.MonoGame;

public static class BackendSystem
{
    public static void Initialise(HowlApp howlApp, Resolution resolution, out MonoGameApp monoGameApp)
    {
        monoGameApp = new(howlApp);
        
    }
}