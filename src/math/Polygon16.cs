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

    public Polygon16(Span<Vector2> vertices)
    {
        if(vertices.Length > MaxVertices)
        {
            throw new InvalidOperationException($"Polygon16 can only store {MaxVertices} amount of vertices; not {vertices.Length}");
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
