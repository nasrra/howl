using System;
using Howl.ECS;
using Howl.Generic;
using Howl.Graphics;
using Howl.Math;

namespace Howl.Graphics.Systems;

public static class SpriteSystems
{
    public static DrawSystem DrawSystem(IRenderer renderer, ComponentRegistry componentRegistry)
    {
        return dt =>
        {
            GenIndexList<Sprite> spritesComponents = componentRegistry.Get<Sprite>();
            Span<DenseEntry<Sprite>> denseEntries = spritesComponents.GetDenseAsSpan();

            GenIndexList<Transform> transformComponents = componentRegistry.Get<Transform>();

            for(int i = 0; i < denseEntries.Length; i++)
            {
                ref DenseEntry<Sprite> denseEntry = ref denseEntries[i];
                ref Sprite sprite = ref denseEntry.Value;
                spritesComponents.GetGenIndex(denseEntry.sparseIndex, out GenIndex genIndex);

                if(transformComponents.GetDenseReadOnlyRef(genIndex, out ReadOnlyRef<Transform> transformRef).Fail())
                    continue;
                
                renderer.DrawSprite(transformRef.Value, sprite);
            }
        };
    }
}