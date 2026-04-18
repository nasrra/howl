using Howl.Ecs;
using Howl.Math;
using Howl.Math.Shapes;

namespace Howl.Graphics;

public struct Sprite
{
    /// <summary>
    ///     The slice of the texture - in pixels - used when drawing.
    /// </summary>
    public Rectangle SourceRectangle; 

    /// <summary>
    ///     The colour used to tint the sprite.
    /// </summary>
    public Colour ColourTint; 

    /// <summary>
    ///     The origin - in pixels - relative to the source rectangle.
    /// </summary>
    public Vector2 Origin;

    /// <summary>
    ///     The scaling vector.
    /// </summary>
    public Vector2 Scale;

    /// <summary>
    ///     The id of texture used when drawing.
    /// </summary>
    public int TextureId; 

    /// <summary>
    ///     The coordinate space to draw in. 
    /// </summary>
    public WorldSpace WorldSpace;

    /// <summary>
    ///     The layer depth.
    /// </summary>
    public float LayerDepth;

    /// <summary>
    ///     Constructs a Sprite.
    /// </summary>
    /// <param name="sourceRectangle">The slice of the texture - in pixels - used when drawing.</param>
    /// <param name="colourTint">The colour used to tint the sprite</param>
    /// <param name="origin">The origin - in pixels - relative to the source rectangle.</param>
    /// <param name="scale">The scaling vector.</param>
    /// <param name="textureId">The id of texture used when drawing.</param>
    /// <param name="worldSpace">The coordinate space this sprite will be drawn in.</param>
    /// <param name="layerDepth">The layer depth.</param>
    public Sprite(Rectangle sourceRectangle, Colour colourTint, Vector2 origin, Vector2 scale, int textureId, WorldSpace worldSpace,
        float layerDepth
    )
    {
        SourceRectangle = sourceRectangle;
        ColourTint = colourTint;
        Origin = origin;
        Scale = scale;
        TextureId = textureId;
        WorldSpace = worldSpace;
        LayerDepth = layerDepth;
    }
}