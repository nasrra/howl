using System;
using Microsoft.Xna.Framework.Graphics;

namespace Howl.Vendors.MonoGame;

public class DebugDrawState
{
    /// <summary>
    ///     The pritive vertex data for drawing.
    /// </summary>
    /// <remarks>
    ///     Note: this collection contains a Nil element.
    /// </remarks>
    public StackArray<VertexPositionColor> PrimitiveVertices;
    
    /// <summary>
    ///     The primitive vertex indice data for drawing.
    /// </summary>
    /// <remarks>
    ///     Note: this collection contains a Nil element.
    /// </remarks>
    public StackArray<int> PrimitiveIndices;

    /// <summary>
    ///     Gets the total number of elements in all the dimensions of the backing arrays.
    /// </summary>
    public int Length;

    /// <summary>
    ///     The count of valid vertex data elements; starting from index one.
    /// </summary>
    public int Count;

    /// <summary>
    ///     Whether this instance has been disposed of.
    /// </summary>
    public bool Disposed;

    /// <summary>
    ///     Creates a new DebugDrawState instance.
    /// </summary>
    /// <param name="length">the lenght of the backing arrays.</param>
    public DebugDrawState(int length)
    {
        PrimitiveVertices = new (length);
        PrimitiveIndices = new (length);
    }

    ~DebugDrawState()
    {
        DebugDraw.Dispose(this);
    }
}