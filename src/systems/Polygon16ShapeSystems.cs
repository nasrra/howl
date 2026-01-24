using System;
using Howl.ECS;
using Howl.Generic;
using Howl.Graphics;
using Howl.Math;

namespace Howl.Systems;

public static class Polygon16ShapeSystems
{

    /// <summary>
    /// Creates a new DrawSystem for drawing a Polygon16Shape.
    /// </summary>
    /// <param name="renderer">The renderer used for drawing.</param>
    /// <param name="componentRegistry">The component registry that contains the necessary data.</param>
    /// <returns>The created DrawSystem instance.</returns>
    public static DrawSystem DrawSystem(IRenderer renderer, ComponentRegistry componentRegistry)
    {
        return dt =>
        {
            GenIndexList<Polygon16Shape> shapeGenIndexList = componentRegistry.Get<Polygon16Shape>();
            Span<DenseEntry<Polygon16Shape>> denseEntries = shapeGenIndexList.GetDenseAsSpan();

            GenIndexList<Transform> transformGenIndexList = componentRegistry.Get<Transform>();

            for(int i = 0; i < denseEntries.Length; i++)
            {
                ref DenseEntry<Polygon16Shape> denseEntry = ref denseEntries[i];
                ref Polygon16Shape shape = ref denseEntry.Value;
                GenIndex genIndex = shapeGenIndexList.GetGenIndex(denseEntry.sparseIndex);

                if (transformGenIndexList.GetDenseRef(genIndex, out Ref<Transform> transform) == GenIndexResult.Success)
                {
                    renderer.DrawWireframeShape(transform, shape);
                }
            }
        };
    }
}