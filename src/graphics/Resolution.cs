namespace Howl.Graphics;

public readonly struct Resolution
{
    public readonly int BackBufferWidth; 
    public readonly int BackBufferHeight; 
    public readonly int MainRenderTargetWidth; 
    public readonly int MainRenderTargetHeight;

    public Resolution(int backBufferWidth, int backBufferHeight, int mainRenderTargetWidth, int mainRenderTargetHeight)
    {
        BackBufferWidth = backBufferWidth;
        BackBufferHeight = backBufferHeight;
        MainRenderTargetWidth = mainRenderTargetWidth;
        MainRenderTargetHeight = mainRenderTargetHeight;
    }
}