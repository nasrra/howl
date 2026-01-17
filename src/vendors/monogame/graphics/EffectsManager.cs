using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Howl.Vendors.MonoGame.Graphics;

public class EffectManager
{
    private WeakReference<MonoGameApp> monogameApp;

    private Effect[] effects;
    
    public BasicEffect PrimitivesEffect {get; private set;}

    public BasicEffect DefaultSpriteEffect {get; private set;}

    public EffectManager(WeakReference<MonoGameApp> monogameApp, int effectsAmount = 0)
    {        
        this.monogameApp = monogameApp;
        effects = new Effect[effectsAmount];
        CreateDefaultSpriteEffect();
        CreatePrimitivesEffect();
    }

    private MonoGameApp GetMonoGameApp()
    {
        if(monogameApp.TryGetTarget(out MonoGameApp app))
        {
            return app;
        }
        else
        {
            throw new NullReferenceException("EffectManager cannot operate on a MonoGameApp that is null.");
        }
    }

    /// <summary>
    /// Creates a default basic shader effect used by MonoGameRenderer when renderering sprites.
    /// </summary>
    private void CreateDefaultSpriteEffect()
    {
        MonoGameApp app = GetMonoGameApp();
        DefaultSpriteEffect = new BasicEffect(app.GraphicsDevice);
        
        DefaultSpriteEffect.FogEnabled = false;
        
        DefaultSpriteEffect.TextureEnabled = true;
        
        DefaultSpriteEffect.LightingEnabled = false;
        
        DefaultSpriteEffect.VertexColorEnabled = true;
        
        DefaultSpriteEffect.World = Matrix.Identity;
        
        DefaultSpriteEffect.Projection = Matrix.Identity;
        
        DefaultSpriteEffect.View = Matrix.Identity;
    }

    /// <summary>
    /// Creates a basic shader effect used by MonoGameRenderer when rendering primitive shapes.
    /// </summary>
    private void CreatePrimitivesEffect()
    {
        MonoGameApp app = GetMonoGameApp();
        PrimitivesEffect = new BasicEffect(app.GraphicsDevice);
        
        PrimitivesEffect.FogEnabled = false;
        
        // dont use textures as this will sample the color data from an image not the vertex colour, colour
        PrimitivesEffect.TextureEnabled = false;
        
        PrimitivesEffect.LightingEnabled = false;
        
        PrimitivesEffect.VertexColorEnabled = true;
        
        PrimitivesEffect.World = Matrix.Identity;
        
        PrimitivesEffect.Projection = Matrix.Identity;
        
        PrimitivesEffect.View = Matrix.Identity;
    }

    /// <summary>
    /// Registers a new MonoGame effect to this Effects Manager.
    /// </summary>
    /// <param name="effect"></param>
    /// <param name="index"></param>
    /// <returns></returns>
    public bool RegisterEffect(Effect effect, int index)
    {
        Span<Effect> span = effects.AsSpan();
        if(span[index] != null)
        {
            return false;
        }
        span[index] = effect;
        return true;
    }

    /// <summary>
    /// Gets a Span of all Effects stored by this EffectManager.
    /// </summary>
    /// <returns></returns>
    // public Span<Effect> EffectsSpan()
    // {
    //     return effects.AsSpan();
    // }

    /// <summary>
    /// Updates the projection matrix and sets it for all effects to ensure that
    /// all effects are within the same coordinate space.
    /// </summary>
    /// <param name="projectionMatrix"></param>
    public void UpdateProjectionMatrix(Matrix projectionMatrix)
    {
        Span<Effect> span = effects.AsSpan();
        for(int i = 0; i < span.Length; i++)
        {
            ref Effect effect = ref span[i];
            if(effect is BasicEffect basicEffect)
            {
                basicEffect.Projection = projectionMatrix;
            }
            else
            {
                effect.Parameters["Projection"].SetValue(projectionMatrix);
            }
        }

        DefaultSpriteEffect.Projection = projectionMatrix;
        PrimitivesEffect.Projection = projectionMatrix;
    }
}