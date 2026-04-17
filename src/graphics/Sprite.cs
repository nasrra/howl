using Howl.Ecs;
using Howl.Math;
using Howl.Math.Shapes;

namespace Howl.Graphics;

public struct Sprite
{
    /// <summary>
    /// Gets and sets the slice of the texture - in pixels - used when drawing.
    /// </summary>
    public Rectangle SourceRectangle; 

    /// <summary>
    /// Gets and sets the colour tint.
    /// </summary>
    public Colour ColourTint; 

    /// <summary>
    /// Gets and set the origin - in pixels - relative to the source rectangle.
    /// </summary>
    public Vector2 Origin;

    /// <summary>
    /// Gets and sets the scale.
    /// </summary>
    public Vector2 Scale;

    /// <summary>
    /// Gets and sets the texture used when drawing.
    /// </summary>
    public GenIndex Texture; 

    /// <summary>
    /// Gets and sets the world space to draw in. 
    /// </summary>
    public WorldSpace WorldSpace;

    /// <summary>
    /// Gets and sets the layer depth.
    /// </summary>
    public float LayerDepth;

    /// <summary>
    /// Constructs a Sprite.
    /// </summary>
    /// <param name="sourceRectangle">The slice of the texture - in pixels - used when drawing.</param>
    /// <param name="colourTint">The colour tint</param>
    /// <param name="origin">The origin - in pixels - relative to the source rectangle.</param>
    /// <param name="scale">The scale.</param>
    /// <param name="texture">The texture used when drawing.</param>
    /// <param name="worldSpace">The world space this sprite will be drawn in.</param>
    /// <param name="layerDepth">The layer depth.</param>
    public Sprite(
        Rectangle sourceRectangle, 
        Colour colourTint, 
        Vector2 origin, 
        Vector2 scale, 
        GenIndex texture,
        WorldSpace worldSpace,
        float layerDepth
    )
    {
        SourceRectangle = sourceRectangle;
        ColourTint = colourTint;
        Origin = origin;
        Scale = scale;
        Texture = texture;
        WorldSpace = worldSpace;
        LayerDepth = layerDepth;
    }
}