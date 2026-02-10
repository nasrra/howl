// using System;
// using Howl.ECS;
// using Howl.Generic;
// using Howl.Math;

// namespace Howl.Graphics.Text;

// public static class TextSystems
// {
//     /// <summary>
//     /// Registers all necessary components for this system.
//     /// </summary>
//     /// <param name="worldComponentRegistry">The world-component registry to register to.</param>
//     /// <param name="guiComponentRegistry">The gui-component registtry to register to.</param>
//     public static void RegisterComponents(ComponentRegistry worldComponentRegistry, ComponentRegistry guiComponentRegistry)
//     {
//         worldComponentRegistry.RegisterComponent<Text16>();
//         worldComponentRegistry.RegisterComponent<Text4096>();
//         worldComponentRegistry.RegisterComponent<Transform>();

//         guiComponentRegistry.RegisterComponent<Text16>();
//         guiComponentRegistry.RegisterComponent<Text4096>();
//         guiComponentRegistry.RegisterComponent<Transform>();
//     }

//     /// <summary>
//     /// Creates a new DrawSystem for drawing text.
//     /// </summary>
//     /// <param name="renderer">The renderer used for drawing.</param>
//     /// <param name="worldComponentRegistry">The world-component registry that contains the necessary data.</param>
//     /// <returns></returns>
//     public static DrawSystem DrawSystem(IRenderer renderer, ComponentRegistry worldComponentRegistry)
//     {
//         return deltaTime =>
//         {
//             DrawText16(renderer, worldComponentRegistry);
//             DrawText4096(renderer, worldComponentRegistry);
//         };
//     }

//     /// <summary>
//     /// Creates a new DrawGuiSystem for drawing gui text.
//     /// </summary>
//     /// <param name="renderer">The renderer used for drawing.</param>
//     /// <param name="guiComponentRegistry">The gui-component registry that contains the necessary data.</param>
//     /// <returns></returns>
//     public static DrawGuiSystem DrawGuiSystem(IRenderer renderer, ComponentRegistry guiComponentRegistry)
//     {
//         return deltaTime =>
//         {
//             DrawText16(renderer, guiComponentRegistry);
//             DrawText4096(renderer, guiComponentRegistry);
//         };
//     }

//     private static void DrawText16(IRenderer renderer, ComponentRegistry componentRegistry)
//     {        
//         GenIndexList<Text16> textComponents = componentRegistry.Get<Text16>();
//         Span<DenseEntry<Text16>> denseEntries = textComponents.GetDenseAsSpan();
//         GenIndexList<Transform> transformComponents = componentRegistry.Get<Transform>();            

//         for(int i = 0; i < denseEntries.Length; i++)
//         {
//             ref DenseEntry<Text16> denseEntry = ref denseEntries[i];
//             ref Text16 text = ref denseEntry.Value;
//             textComponents.GetGenIndex(denseEntry.sparseIndex, out GenIndex genIndex);
            
//             if(transformComponents.GetDenseReadOnlyRef(genIndex, out ReadOnlyRef<Transform> transformReadOnlyRef).Fail())
//                 continue;

//             renderer.DrawText(transformReadOnlyRef.Value, text);
//         }
//     }

//     private static void DrawText4096(IRenderer renderer, ComponentRegistry componentRegistry)
//     {        
//         GenIndexList<Text4096> textComponents = componentRegistry.Get<Text4096>();
//         Span<DenseEntry<Text4096>> denseEntries = textComponents.GetDenseAsSpan();
//         GenIndexList<Transform> transformComponents = componentRegistry.Get<Transform>();            

//         for(int i = 0; i < denseEntries.Length; i++)
//         {
//             ref DenseEntry<Text4096> denseEntry = ref denseEntries[i];
//             ref Text4096 text = ref denseEntry.Value;
//             textComponents.GetGenIndex(denseEntry.sparseIndex, out GenIndex genIndex);
            
//             if(transformComponents.GetDenseReadOnlyRef(genIndex, out ReadOnlyRef<Transform> transformReadOnlyRef).Fail())
//                 continue;

//             renderer.DrawText(transformReadOnlyRef.Value, text);
//         }
//     }

// }