using System;
using Howl.ECS;
using Howl.Generic;
using Howl.Graphics;
using Howl.Math;

namespace Howl.Graphics.Systems;

public static class CircleShapeSystems
{
    /// <summary>
    /// Creates a new DrawSystem for drawing CircleShape's.
    /// </summary>
    /// <param name="renderer">The renderer used for drawing.</param>
    /// <param name="componentRegistry">The component registry that contains the necessary data..</param>
    /// <returns>The created DrawSystem instance.</returns>
    public static DrawSystem DrawSystem(IRenderer renderer, ComponentRegistry componentRegistry)
    {
        return dt =>
        {
            GenIndexList<CircleShape> shapeGenIndexList = componentRegistry.Get<CircleShape>();
            Span<DenseEntry<CircleShape>> denseEntries = shapeGenIndexList.GetDenseAsSpan();

            GenIndexList<Transform> transformGenIndexList = componentRegistry.Get<Transform>();

            for(int i = 0; i < denseEntries.Length; i++)
            {
                ref DenseEntry<CircleShape> dense = ref denseEntries[i];
                ref CircleShape shape = ref dense.Value;
                shapeGenIndexList.GetGenIndex(dense.sparseIndex, out GenIndex genIndex);

                if(transformGenIndexList.GetDenseRef(genIndex, out Ref<Transform> transform).Fail(out var result))
                    continue;

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