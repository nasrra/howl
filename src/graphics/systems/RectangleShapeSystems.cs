using System;
using Howl.ECS;
using Howl.Generic;
using Howl.Graphics;
using Howl.Math;

namespace Howl.Graphics.Systems;

public static class RectangleShapeSystems{
    
    /// <summary>
    /// Creates a new DrawSystem for drawing RectangleShape's.
    /// </summary>
    /// <param name="renderer">The renderer used for drawing.</param>
    /// <param name="componentRegistry">The component registry that constains the necessary data.</param>
    /// <returns>The create DrawSystem instance.</returns>
    public static DrawSystem DrawSystem(IRenderer renderer, ComponentRegistry componentRegistry)
    {
        return dt =>
        {
            GenIndexList<RectangleShape> shapeGenIndexList = componentRegistry.Get<RectangleShape>();
            Span<DenseEntry<RectangleShape>> denseEntries = shapeGenIndexList.GetDenseAsSpan();
            
            GenIndexList<Transform> transformGenIndexList = componentRegistry.Get<Transform>();
            
            for(int i = 0; i < denseEntries.Length; i++)
            {
                ref DenseEntry<RectangleShape> denseEntry = ref denseEntries[i];
                ref RectangleShape shape = ref denseEntry.Value;
                shapeGenIndexList.GetGenIndex(denseEntry.sparseIndex, out GenIndex genIndex);

                // only draw if there is a transform associated with the rectangle.
                if(transformGenIndexList.GetDenseRef(genIndex, out Ref<Transform> transformRef).Fail())
                    continue;

                ref Transform transform = ref transformRef.Value;

                switch (shape.DrawMode)
                {
                    case DrawMode.Fill:
                        renderer.DrawFilledShape(transform, shape);
                    break;
                    case DrawMode.Wireframe:
                        renderer.DrawWireframeShape(transform, shape);
                    break;
                }
            }
        };
    }
}