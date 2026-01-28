using System;
using Howl.ECS;
using Howl.Generic;
using Howl.Math;

namespace Howl.Graphics.Systems;

public static class TextSystems
{
    public static void RegisterComponents(ComponentRegistry componentRegistry)
    {
        componentRegistry.RegisterComponent<Text16>();
    }

    public static DrawSystem DrawSystem(IRenderer renderer, ComponentRegistry componentRegistry)
    {
        return deltaTime =>
        {
            GenIndexList<Text16> textComponents = componentRegistry.Get<Text16>();
            Span<DenseEntry<Text16>> denseEntries = textComponents.GetDenseAsSpan();

            GenIndexList<Transform> transformComponents = componentRegistry.Get<Transform>();            

            for(int i = 0; i < denseEntries.Length; i++)
            {
                ref DenseEntry<Text16> denseEntry = ref denseEntries[i];
                ref Text16 text = ref denseEntry.Value;
                textComponents.GetGenIndex(denseEntry.sparseIndex, out GenIndex genIndex);
                
                if(transformComponents.GetDenseReadOnlyRef(genIndex, out ReadOnlyRef<Transform> transformReadOnlyRef) == GenIndexResult.Success)
                {
                    renderer.DrawString(transformReadOnlyRef.Value, text);
                }
            }
        };
    }
}