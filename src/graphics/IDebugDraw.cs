using Howl.Math;

namespace Howl.Graphics;

public interface IDebugDraw
{
    /// <summary>
    /// Draws a rectangle to the screen.
    /// </summary>
    public void DrawRectangle(Rectangle rectangle, Color color);
}