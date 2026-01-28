using System;
using Howl.ECS;
using Howl.Generic;
using Howl.Math;

namespace Howl.Graphics.Text;

public static class TextSystems
{
    /// <summary>
    /// Registers all necessary components for this system.
    /// </summary>
    /// <param name="componentRegistry">The component registry to register to.</param>
    public static void RegisterComponents(ComponentRegistry componentRegistry)
    {
        componentRegistry.RegisterComponent<Text16>();
        componentRegistry.RegisterComponent<Text4096>();
        componentRegistry.RegisterComponent<GuiText16>();
        componentRegistry.RegisterComponent<GuiText4096>();
    }

    /// <summary>
    /// Creates a new DrawSystem for drawing text.
    /// </summary>
    /// <param name="renderer">The renderer used for drawing.</param>
    /// <param name="componentRegistry">The component registry that contains the necessary data.</param>
    /// <returns></returns>
    public static DrawSystem DrawSystem(IRenderer renderer, ComponentRegistry componentRegistry)
    {
        return deltaTime =>
        {
            DrawText16(renderer, componentRegistry);
            DrawText4096(renderer, componentRegistry);
        };
    }

    /// <summary>
    /// Creates a new DrawGuiSystem for drawing gui text.
    /// </summary>
    /// <param name="renderer">The renderer used for drawing.</param>
    /// <param name="componentRegistry">The component registry that contains the necessary data.</param>
    /// <returns></returns>
    public static DrawGuiSystem DrawGuiSystem(IRenderer renderer, ComponentRegistry componentRegistry)
    {
        return deltaTime =>
        {
            DrawGuiText16(renderer, componentRegistry);  
            DrawGuiText4096(renderer, componentRegistry);  
        };
    }

    private static void DrawText16(IRenderer renderer, ComponentRegistry componentRegistry)
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
                renderer.DrawText(transformReadOnlyRef.Value, text);
            }
        }
    }

    private static void DrawText4096(IRenderer renderer, ComponentRegistry componentRegistry)
    {        
        GenIndexList<Text4096> textComponents = componentRegistry.Get<Text4096>();
        Span<DenseEntry<Text4096>> denseEntries = textComponents.GetDenseAsSpan();
        GenIndexList<Transform> transformComponents = componentRegistry.Get<Transform>();            

        for(int i = 0; i < denseEntries.Length; i++)
        {
            ref DenseEntry<Text4096> denseEntry = ref denseEntries[i];
            ref Text4096 text = ref denseEntry.Value;
            textComponents.GetGenIndex(denseEntry.sparseIndex, out GenIndex genIndex);
            
            if(transformComponents.GetDenseReadOnlyRef(genIndex, out ReadOnlyRef<Transform> transformReadOnlyRef) == GenIndexResult.Success)
            {
                renderer.DrawText(transformReadOnlyRef.Value, text);
            }
        }
    }

    private static void DrawGuiText16(IRenderer renderer, ComponentRegistry componentRegistry)
    {        
        GenIndexList<GuiText16> textComponents = componentRegistry.Get<GuiText16>();
        Span<DenseEntry<GuiText16>> denseEntries = textComponents.GetDenseAsSpan();
        GenIndexList<Transform> transformComponents = componentRegistry.Get<Transform>();            

        for(int i = 0; i < denseEntries.Length; i++)
        {
            ref DenseEntry<GuiText16> denseEntry = ref denseEntries[i];
            ref GuiText16 text = ref denseEntry.Value;
            textComponents.GetGenIndex(denseEntry.sparseIndex, out GenIndex genIndex);
            
            if(transformComponents.GetDenseReadOnlyRef(genIndex, out ReadOnlyRef<Transform> transformReadOnlyRef) == GenIndexResult.Success)
            {
                renderer.DrawText(transformReadOnlyRef.Value, text);
            }
        }
    }

    private static void DrawGuiText4096(IRenderer renderer, ComponentRegistry componentRegistry)
    {        
        GenIndexList<GuiText4096> textComponents = componentRegistry.Get<GuiText4096>();
        Span<DenseEntry<GuiText4096>> denseEntries = textComponents.GetDenseAsSpan();
        GenIndexList<Transform> transformComponents = componentRegistry.Get<Transform>();            

        for(int i = 0; i < denseEntries.Length; i++)
        {
            ref DenseEntry<GuiText4096> denseEntry = ref denseEntries[i];
            ref GuiText4096 text = ref denseEntry.Value;
            textComponents.GetGenIndex(denseEntry.sparseIndex, out GenIndex genIndex);
            
            if(transformComponents.GetDenseReadOnlyRef(genIndex, out ReadOnlyRef<Transform> transformReadOnlyRef) == GenIndexResult.Success)
            {
                renderer.DrawText(transformReadOnlyRef.Value, text);
            }
        }
    }
}