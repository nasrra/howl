using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Howl.Vendors.MonoGame.Graphics;
using Howl.Vendors.MonoGame.Math;

namespace Howl.Vendors.MonoGame.Graphics;

public static class VertexPositionColourExtensions
{
    /// <summary>
    /// Converts a <see cref="Howl.Graphics.VertexPositionColour"/> to a <see cref="Microsoft.Xna.Framework.Graphics.VertexPositionColor"/>
    /// </summary>
    /// <param name="howl">the <see cref="Howl.Graphics.VertexPositionColour"/> to convert.</param>
    /// <returns>the constructed <see cref="Microsoft.Xna.Framework.Graphics.VertexPositionColor"/></returns>
    public static Microsoft.Xna.Framework.Graphics.VertexPositionColor ToMonoGame(this Howl.Graphics.VertexPositionColour howl)
    {
        return new Microsoft.Xna.Framework.Graphics.VertexPositionColor(howl.Position.ToMonoGame(), howl.Colour.ToMonoGame());
    }

    /// <summary>
    /// Converts a list of <see cref="Howl.Graphics.VertexPositionColour"/> to an array of <see cref="Microsoft.Xna.Framework.Graphics.VertexPositionColor"/>.
    /// </summary>
    /// <param name="list">the list of <see cref="Howl.Graphics.VertexPositionColour"/> to convert.</param>
    /// <returns>the newly instantiated array of <see cref="Microsoft.Xna.Framework.Graphics.VertexPositionColor"/>.</returns>
    public static Microsoft.Xna.Framework.Graphics.VertexPositionColor[] ToMonoGameArray(this List<Howl.Graphics.VertexPositionColour> list)
    {
        Microsoft.Xna.Framework.Graphics.VertexPositionColor[] array = new Microsoft.Xna.Framework.Graphics.VertexPositionColor[list.Count];
        ReadOnlySpan<Howl.Graphics.VertexPositionColour> span = CollectionsMarshal.AsSpan(list);
        for(int i = 0; i < span.Length; i++)
        {
            array[i] = span[i].ToMonoGame(); 
        }
        return array;
    }
}