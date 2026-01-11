
using System;
using Microsoft.Xna.Framework.Graphics;

namespace Howl.Graphics;

public class RenderTarget : IDisposable
{
    RenderTarget2D target;
    private bool isDisposed = false;
    private bool isSet = false;
    public bool IsSet => isSet;

    public RenderTarget(int width, int height)
    {
        width = Math.Clamp(width, 1, int.MaxValue);
        height = Math.Clamp(height, 1, int.MaxValue);

        target = new RenderTarget2D(HowlApp.GraphicsDevice, width, height);
    }

    public void Dispose()
    {
        if (isDisposed)
        {
            return;
        }
        target?.Dispose();
        isDisposed = true;
    }

    /// <summary>
    /// Sets the render target of the HowlApp to this instance.
    /// </summary>
    /// <returns>true, if the render target was successfully set; otherwise false if this instance is already set.</returns>

    public bool Set()
    {
        if(isSet == true)
        {
            return false;
        }

        HowlApp.GraphicsDevice.SetRenderTarget(target);
        isSet = true;
        return true;
    }

    /// <summary>
    /// Unsets the render target of the HowlApp from this instance.
    /// </summary>
    /// <returns>true, if the render target was successfully unset; otherwise false it this isnt already set.</returns>
    
    public bool Unset()
    {
        if(isSet == false)
        {
            return false;    
        }

        // Note: setting the render target to null makes the HowlApp render directly to the back buffer.
        HowlApp.GraphicsDevice.SetRenderTarget(null);        
        isSet = false;

        return true;
    }
}