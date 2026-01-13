using System;
using Howl.MonoGame;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Howl.MonoGame.Graphics;

public class EffectManager
{
    private WeakReference<MonoGameApp> monogameApp;

    private const int DefaultEffectIndex = 0;

    private Effect[] effects;

    public EffectManager(WeakReference<MonoGameApp> monogameApp, int effectsAmount = 1)
    {        
        this.monogameApp = monogameApp;
        effects = new Effect[effectsAmount];
        CreateDefaultEffect();
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
    /// Creates a default basic shader effect used by  MonoGameRenderer.
    /// </summary>
    private void CreateDefaultEffect()
    {
        MonoGameApp app = GetMonoGameApp();
        BasicEffect Default = new BasicEffect(app.GraphicsDevice);
        Default.FogEnabled = false;
        Default.TextureEnabled = true;
        Default.LightingEnabled = false;
        Default.VertexColorEnabled = true;
        Default.World = Matrix.Identity;
        Default.Projection = Matrix.Identity;
        Default.View = Matrix.Identity;
        effects[DefaultEffectIndex] = Default;
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
    public Span<Effect> EffectsSpan()
    {
        return effects.AsSpan();
    }

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
    }
}