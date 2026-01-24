using System;
using Howl.Math;

namespace Howl.Graphics;

public struct Polygon16Shape
{
    /// <summary>
    /// Gets and sets the polygon data.
    /// </summary>
    public Polygon16 Polygon;

    /// <summary>
    /// Gets and sets the colour used when drawing.
    /// </summary>
    public Colour Colour;

    /// <summary>
    /// Gets and sets the origin.
    /// </summary>
    public Vector2 Origin;

    /// <summary>
    /// Constructs a Polygon16Shape.
    /// </summary>
    /// <param name="polygon">The polygon data.</param>
    /// <param name="colour">The colour used when drawing.</param>
    /// <param name="origin">The origin of the shape.</param>
    /// <param name="drawMode">The draw mode..</param>
    public Polygon16Shape(Polygon16 polygon, Colour colour, Vector2 origin)
    {
        Polygon = polygon;
        Colour = colour;
        Origin = origin;
    }

    /// <summary>
    /// Gets a stored vertex.
    /// </summary>
    /// <param name="vertexId">The id of the vertex.</param>
    /// <returns></returns>
    public unsafe Vector2 GetVertex(int vertexId)
    {   
        if(vertexId >= Polygon.VerticesCount || vertexId < 0)
        {
            throw new OverflowException($"Cannot get index '{vertexId}' from Polygon16 with a vertices count of '{Polygon.VerticesCount}'");
        }

        return new Vector2(Polygon.XVertices[vertexId], Polygon.YVertices[vertexId]) - Origin;
    }
}