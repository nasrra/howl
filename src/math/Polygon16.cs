using System;

namespace Howl.Math;

public unsafe struct Polygon16
{

    /// <summary>
    /// The maximum amount of vertices a Polygon16 can store.
    /// </summary>
    public const int MaxVertices = 16;

    /// <summary>
    /// Gets and sets the x-coordinate vertices of this polygon.
    /// </summary>
    public fixed float XVertices[MaxVertices];
    
    /// <summary>
    /// Gets and sets the y-coordinate vertices of this polygon.
    /// </summary>
    public fixed float YVertices[MaxVertices];

    private int verticesCount;

    /// <summary>
    /// Gets the amount of vertices that this polygon stores.
    /// </summary>
    public int VerticesCount => verticesCount;

    /// <summary>
    /// Constructs a Polygon16
    /// </summary>
    /// <param name="vertices">The vertices to insert into this polygon.</param>
    /// <exception cref="InvalidOperationException">thrown when the passed vertices span length is unsupported.</exception>
    public Polygon16(Span<Vector2> vertices)
    {
        if(vertices.Length > MaxVertices)
        {
            throw new ArgumentException($"Polygon16 cannot store '{vertices.Length}' amount of vertices. The amount of vertices length must be between '{0}' and '{MaxVertices}'");
        }

        verticesCount = System.Math.Min(vertices.Length, MaxVertices);
        fixed (float* xDst = XVertices)
        {
            fixed(float* yDst = YVertices)
            {                
                for (int i = 0; i < verticesCount; i++)
                {
                    xDst[i] = vertices[i].X;
                    yDst[i] = vertices[i].Y;
                }
            }
        }
    }
}
