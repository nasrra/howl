using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Howl.Graphics;

public class EffectManager
{
    private const int DefaultEffectIndex = 0;

    private Effect[] effects;

    public EffectManager(int effectsAmount = 1)
    {        
        effects = new Effect[effectsAmount];
        CreateDefaultEffect();
    }

    private void CreateDefaultEffect()
    {
        BasicEffect Default = new BasicEffect(HowlApp.GraphicsDevice);
        Default.FogEnabled = false;
        Default.TextureEnabled = true;
        Default.LightingEnabled = false;
        Default.VertexColorEnabled = true;
        Default.World = Matrix.Identity;
        Default.Projection = Matrix.Identity;
        Default.View = Matrix.Identity;
        effects[DefaultEffectIndex] = Default;
    }

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

    public Span<Effect> EffectsSpan()
    {
        return effects.AsSpan();
    }

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

    internal void UpdateProjectionMatrix(object projectionMatrix)
    {
        throw new NotImplementedException();
    }
}